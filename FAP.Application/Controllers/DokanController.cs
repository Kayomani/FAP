using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using Dokan;
using FAP.Domain;
using FAP.Domain.Entities;
using FAP.Domain.Entities.FileSystem;
using FAP.Domain.Net;
using FAP.Domain.Services;
using FAP.Domain.Verbs;
using FAP.Network;
using NLog;
using Fap.Foundation;

namespace FAP.Application.Controllers
{
    public class DokanController : DokanOperations
    {
        private Model _model;
        private const char DriveLetter = 't';

        private Dictionary<string,BrowsingCache> _browsingcache = new Dictionary<string, BrowsingCache>();
        private  ReaderWriterLockSlim readLock = new ReaderWriterLockSlim();
        private readonly int BROWSE_CACHE_TIME = 10000;//10 seconds
        private readonly int FILE_READAHEAD_SIZE = 262144;//0.5mb
        private Dictionary<string, ReadAheadCache> readCache = new Dictionary<string, ReadAheadCache>();

        public DokanController(Model m)
        {
            _model =m;
        }

        public void Stop()
        {
           int result = DokanNet.DokanUnmount(DriveLetter);
        }

        public void Start()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(StartAsync));
        }

        private  void StartAsync(object o)
        {
            var options = new DokanOptions
            {
                DebugMode = false,
                MountPoint = DriveLetter.ToString(),
                ThreadCount = 10,
                RemovableDrive = false,
                NetworkDrive = false,
                VolumeLabel = "FAP",
                UseKeepAlive = true
            };
            var status = DokanNet.DokanMain(options, this);
            switch (status)
            {
                case DokanNet.DOKAN_DRIVE_LETTER_ERROR:
                    LogManager.GetLogger("faplog").Warn("Dokan drive letter error.");
                    break;
                case DokanNet.DOKAN_DRIVER_INSTALL_ERROR:
                    LogManager.GetLogger("faplog").Warn("Dokan Driver install error");
                    break;
                case DokanNet.DOKAN_MOUNT_ERROR:
                    LogManager.GetLogger("faplog").Warn("Dokan Mount error");
                    break;
                case DokanNet.DOKAN_START_ERROR:
                    LogManager.GetLogger("faplog").Warn("Dokan Start error");
                    break;
                case DokanNet.DOKAN_ERROR:
                    LogManager.GetLogger("faplog").Warn("Dokan Unknown error");
                    break;
                case DokanNet.DOKAN_SUCCESS:
                    LogManager.GetLogger("faplog").Warn("Dokan Success");
                    break;
                default:
                    LogManager.GetLogger("faplog").Warn("Dokan Unknown status: %d", status);
                    break;

            }
        }


        public int ReadFile(string filename, byte[] buffer, ref uint readBytes, long offset, DokanFileInfo info)
        {
            

            ReadAheadCache cacheItem;
            lock(readCache)
            {
                if(readCache.ContainsKey(filename))
                {
                    cacheItem = readCache[filename];
                }
                else
                {
                    cacheItem = new ReadAheadCache();
                    readCache.Add(filename,cacheItem);
                }
            }
            try
            {
                Monitor.Enter(cacheItem.Lock);
                //If cache item is valid then check to see if we already have the data we need
                if (cacheItem.Expires > Environment.TickCount)
                {
                    if (offset >= cacheItem.Offset && offset < cacheItem.Offset + cacheItem.Data.Length)
                    {
                        int read = 0;
                        for (long internalOffset = offset - cacheItem.Offset; internalOffset < cacheItem.DataSize && read < buffer.Length; internalOffset++)
                        {
                            buffer[read] = cacheItem.Data[internalOffset];
                            read++;
                        }
                        if (read > 0)
                        {
                            LogManager.GetLogger("faplog").Trace("Dokan ReadFile (cached) {0} Size {1} Offset {2}", filename, buffer.Length, offset);
                            readBytes = (uint)read;
                            return DokanNet.DOKAN_SUCCESS;
                        }
                    }
                }

                //Not in buffer
                LogManager.GetLogger("faplog").Trace("Dokan ReadFile {0} Size {1} Offset {2}", filename, buffer.Length, offset);

                try
                {
                    string path;
                    var node = ParsePath(filename, out path);
                    if (null != node)
                    {
                        string url = path.Replace('\\', '/');
                        if (!url.StartsWith("/"))
                            url = "/" + url;

                        var req = (HttpWebRequest)
                                  WebRequest.Create(getDownloadUrl(node) + url);
                        // req.UserAgent = Model.AppVersion;
                        // req.Headers.Add("FAP-SOURCE", model.LocalNode.ID);
                       // req.UserAgent = Model.AppVersion;
                        req.Headers.Add("FAP-SOURCE", _model.LocalNode.ID);
                        req.Pipelined = true;
                        // req.Timeout = 10000;
                      //   req.ReadWriteTimeout = 10000;
                        //If we are resuming then add range
                       // if (offset != 0)
                        //{
                            //Yes Micrsoft if you read this...  OH WHY IS ADDRANGE ONLY AN INT?? We live in an age where we might actually download more than 2gb
                            //req.AddRange(fileStream.Length);

                            //Hack
                            MethodInfo method = typeof(WebHeaderCollection).GetMethod("AddWithoutValidate",
                                                                                       BindingFlags.Instance |
                                                                                       BindingFlags.NonPublic);
                            string key = "Range";

                            long readSize = FILE_READAHEAD_SIZE;
                             //If request is bigger then read ahead then increase request size
                            if (buffer.LongLength > readSize)
                            {
                                //Upto the free file size limit
                                if (Model.FREE_FILE_LIMIT > buffer.LongLength)
                                    readSize = buffer.LongLength;
                                else
                                    readSize = Model.FREE_FILE_LIMIT;
                            }

                            string val = string.Format("bytes={0}-{1}", offset, offset + readSize);
                            method.Invoke(req.Headers, new object[] { key, val });
                        //}

                        using(var resp = (HttpWebResponse)req.GetResponse())
                        {

                            if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.PartialContent)
                            {
                                cacheItem.Data = new byte[readSize];
                                cacheItem.Offset = offset;
                                using (var r = resp.GetResponseStream())
                                {
                                    cacheItem.DataSize = 0;
                                    int position = 0;
                                    while (position < resp.ContentLength)
                                    {
                                        int read = r.Read(cacheItem.Data, position, cacheItem.Data.Length - position);
                                        cacheItem.DataSize += read;
                                        position += read;
                                    }


                                    cacheItem.Expires = Environment.TickCount + BROWSE_CACHE_TIME;
                                    if (cacheItem.DataSize != resp.ContentLength)
                                    {

                                    }

                                    {
                                        int read = 0;
                                        for (long i = 0; i < cacheItem.DataSize && i < buffer.Length; i++)
                                        {
                                            buffer[i] = cacheItem.Data[i];
                                            read++;
                                        }

                                        //Do not save oversize buffers
                                        if (cacheItem.Data.Length > FILE_READAHEAD_SIZE)
                                        {
                                            cacheItem.Data = new byte[0];
                                            cacheItem.Expires = 0;
                                        }
                                        else
                                            cacheItem.Expires = Environment.TickCount + BROWSE_CACHE_TIME;

                                        readBytes = (uint)read;
                                        return DokanNet.DOKAN_SUCCESS;
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
            }
            finally
            {
                Monitor.Exit(cacheItem.Lock);
            }
            return DokanNet.DOKAN_ERROR;
        }

        private Node ParsePath(string path, out string nodePath)
        {
            nodePath = string.Empty;
            string[] split = path.Split("\\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (split.Length > 0)
            {
                var node = _model.Network.Nodes.ToList().Where(n => n.Nickname == split[0]).FirstOrDefault();
                if (null != node)
                {
                    //Find path
                    if (path.Length > split[0].Length + 2)
                        nodePath = path.Substring(split[0].Length + 2);
                    return node;
                }
            }
            return null;
        }

        private List<BrowsingFile> GetDirectoryWithCache(string path)
        {

            if (string.IsNullOrEmpty(path) || path == "\\")
            {
                List<BrowsingFile> results = new List<BrowsingFile>();

                //Root, send client list.
                foreach (var client in _model.Network.Nodes.ToList().Where(n => n.NodeType == ClientType.Client))
                {
                    var fi = new BrowsingFile
                    {
                       Size = client.ShareSize,
                       Name = client.Nickname,
                       LastModified = DateTime.Now,
                       IsFolder = true
                    };
                    if (!string.IsNullOrEmpty(client.Nickname))
                        results.Add(fi);

                }
                return results;
            }

            BrowsingCache cache;
            try
            {
                readLock.EnterReadLock();
                if (_browsingcache.ContainsKey(path))
                    cache = _browsingcache[path];
                else
                {
                    cache = new BrowsingCache() { Expires = 0};
                    _browsingcache.Add(path, cache);
                }
               
            }
            finally
            {
                readLock.ExitReadLock();
            }

            try
            {
                Monitor.Enter(cache.Lock);
                //Item is cached so just return it

                if (cache.Expires > Environment.TickCount)
                {

                    if (null == cache.Info)
                        return null;
                    return cache.Info.ToList();
                }

                string nodePath;
                var node = ParsePath(path, out nodePath);
                if (null != node)
                {
                    var c = new Client(_model.LocalNode);
                    var cmd = new BrowseVerb(null);
                    cmd.Path = "/" + nodePath.Replace('\\', '/');
                    cmd.NoCache = false;
                    LogManager.GetLogger("faplog").Trace("Dokan ## Get info for {0}", path);
                    if (c.Execute(cmd, node))
                    {
                        cache.Info = cmd.Results;
                        cache.Expires = Environment.TickCount + BROWSE_CACHE_TIME;
                        return cache.Info.ToList();
                    }
                }
                return null;
            }
            finally
            {
                Monitor.Exit(cache.Lock);
            }
        }

        #region Dokan Ops
        public int CreateFile(string filename, System.IO.FileAccess access, System.IO.FileShare share, System.IO.FileMode mode, System.IO.FileOptions options, DokanFileInfo info)
        {
            return DokanNet.ERROR_ACCESS_DENIED;
        }

        public int OpenDirectory(string filename, DokanFileInfo info)
        {
            LogManager.GetLogger("faplog").Trace("Dokan OpenDirectory {0}",filename);
            if (null != GetDirectoryWithCache(filename))
                return DokanNet.DOKAN_SUCCESS;
            return DokanNet.ERROR_PATH_NOT_FOUND;
        }

        public int CreateDirectory(string filename, DokanFileInfo info)
        {
            return DokanNet.DOKAN_ERROR;
        }

        public int Cleanup(string filename, DokanFileInfo info)
        {
            return 0;
        }

        public int CloseFile(string filename, DokanFileInfo info)
        {
            return 0;
        }

        private string getDownloadUrl(Node remoteNode)
        {
            var sb = new StringBuilder();
            sb.Append("http://");
            sb.Append(remoteNode.Location);
            if (!remoteNode.Location.EndsWith("/"))
                sb.Append("/");
            return sb.ToString();
        }

        public int WriteFile(string filename, byte[] buffer, ref uint writtenBytes, long offset, DokanFileInfo info)
        {
            return 0;
        }

        public int FlushFileBuffers(string filename, DokanFileInfo info)
        {
            return DokanNet.DOKAN_ERROR;
        }

        public int GetFileInformation(string fullPath, FileInformation fileinfo, DokanFileInfo info)
        {
            LogManager.GetLogger("faplog").Trace("Dokan GetFileInformation {0}", fullPath);

            string fileName = Path.GetFileName(fullPath);
            string path = Path.GetDirectoryName(fullPath);

            var bf = GetDirectoryWithCache(path);
            if (null != bf)
            {
                var search = bf.Where(f => f.Name == fileName).FirstOrDefault();
                if (null != search)
                {
                    fileinfo.Attributes =search.IsFolder?  FileAttributes.Directory:FileAttributes.Normal;
                    fileinfo.CreationTime = search.LastModified;
                    fileinfo.FileName = fileName;
                    fileinfo.LastAccessTime = search.LastModified;
                    fileinfo.LastWriteTime = search.LastModified;
                    fileinfo.Length = search.Size;
                    return DokanNet.DOKAN_SUCCESS;
                }
            }
            return DokanNet.DOKAN_ERROR;
        }

        public int FindFiles(string path, System.Collections.ArrayList files, DokanFileInfo info)
        {
            LogManager.GetLogger("faplog").Trace("Dokan FindFiles {0}", path);

            var bf = GetDirectoryWithCache(path);
            if (null != bf)
            {
                foreach (var item in bf)
                {
                    files.Add(new FileInformation
                    {
                        Attributes = item.IsFolder ? FileAttributes.Directory : FileAttributes.Normal,
                        CreationTime = item.LastModified,
                        LastAccessTime = item.LastModified,
                        LastWriteTime = item.LastModified,
                        Length = item.Size,
                        FileName = item.Name
                    });
                }
                return DokanNet.DOKAN_SUCCESS;

            }
            return DokanNet.DOKAN_ERROR;
        }

        public int SetFileAttributes(string filename, System.IO.FileAttributes attr, DokanFileInfo info)
        {
            return DokanNet.ERROR_ACCESS_DENIED;
        }

        public int SetFileTime(string filename, DateTime ctime, DateTime atime, DateTime mtime, DokanFileInfo info)
        {
            return DokanNet.ERROR_ACCESS_DENIED;
        }

        public int DeleteFile(string filename, DokanFileInfo info)
        {
            return DokanNet.ERROR_ACCESS_DENIED;
        }

        public int DeleteDirectory(string filename, DokanFileInfo info)
        {
            return DokanNet.ERROR_ACCESS_DENIED;
        }

        public int MoveFile(string filename, string newname, bool replace, DokanFileInfo info)
        {
            return DokanNet.ERROR_ACCESS_DENIED;
        }

        public int SetEndOfFile(string filename, long length, DokanFileInfo info)
        {
            return DokanNet.ERROR_ACCESS_DENIED;
        }

        public int SetAllocationSize(string filename, long length, DokanFileInfo info)
        {
            return DokanNet.ERROR_ACCESS_DENIED;
        }

        public int LockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            return 0;
        }

        public int UnlockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            return 0;
        }

        public int GetDiskFreeSpace(ref ulong freeBytesAvailable, ref ulong totalBytes, ref ulong totalFreeBytes, DokanFileInfo info)
        {
            var networkSize = (ulong)_model.Network.Nodes.Sum(n => n.ShareSize);
            totalBytes = (ulong)(networkSize * 10);
            freeBytesAvailable = totalBytes - networkSize;
            totalFreeBytes = totalBytes - networkSize;
            return 0;
        }

        public int Unmount(DokanFileInfo info)
        {
            return 0;
        }
        #endregion

        public class BrowsingCache
        {
            public int Expires { set; get; }
            public List<BrowsingFile> Info { set; get; }
            public object Lock = new object();
        }

        public class ReadAheadCache
        {
            public int Expires { set; get; }
            public byte[] Data { set; get; }
            public long Offset { set; get; }
            public int DataSize { set; get; }
            public object Lock = new object();
        }
   }
}
