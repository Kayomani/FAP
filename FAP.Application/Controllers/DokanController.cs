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
        private readonly TimeSpan BROWSE_CACHE_TIME = new TimeSpan(0,0,15);//10 seconds
        private readonly int FILE_READAHEAD_SIZE = 262144;//0.25mb
        private readonly int LARGE_READ_SIZE = 1000000;//980kb 
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

        /// <summary>
        /// Remove expired items in the cache
        /// </summary>
        public void CleanUp()
        {
            //Clean read cache
            lock (readCache)
            {
                foreach (var item in readCache.ToList())
                {
                    if (item.Value.Expires < DateTime.Now)
                    {
                        lock (item.Value.Lock)
                        {
                            readCache.Remove(item.Key);
                            item.Value.Data = null;
                            if (null != item.Value.Response)
                            {
                                try
                                {
                                    using (var resp = item.Value.Response)
                                    {
                                        using (var stream = resp.GetResponseStream())
                                            stream.Close();
                                    }
                                }
                                catch { }
                                item.Value.Response = null;
                            }
                        }
                    }
                }
            }
            //Clean file listing cache
            try
            {
                readLock.EnterReadLock();
                foreach (var item in _browsingcache.ToList())
                {
                    if (item.Value.Expires < DateTime.Now)
                    {
                        lock (item.Value.Lock)
                        {
                            _browsingcache.Remove(item.Key);
                            if (null != item.Value.Info)
                                item.Value.Info.Clear();
                        }
                    }
                }
            }
            finally
            {
                readLock.ExitReadLock();
            }
        }

        public int ReadFile(string filename, byte[] buffer, ref uint readBytes, long offset, DokanFileInfo info)
        {

            FileInformation fileInfo = new FileInformation();
            if (DokanNet.DOKAN_SUCCESS == GetFileInformation(filename, fileInfo, null) && null != fileInfo)
            {
                if (fileInfo.Length <= offset || offset < 0)
                {
                    LogManager.GetLogger("faplog").Error("Dokan ReadFile too far {0} {1} to {2} ", filename, offset, offset + buffer.Length);
                    readBytes = 0;
                    return DokanNet.DOKAN_SUCCESS;
                }
            }


            ReadAheadCache cacheItem;
            lock (readCache)
            {
                if (readCache.ContainsKey(filename))
                {
                    cacheItem = readCache[filename];
                }
                else
                {
                    cacheItem = new ReadAheadCache();
                    readCache.Add(filename, cacheItem);
                }
            }

            LogManager.GetLogger("faplog").Error("Dokan ReadFile {0} {1} at {2} to {3}", cacheItem.Type, filename, offset, offset + buffer.Length);

            try
            {
                Monitor.Enter(cacheItem.Lock);
                //If cache item is valid then check to see if we already have the data we need
                if (cacheItem.Expires > DateTime.Now && cacheItem.Type == ReadAheadCacheType.Random)
                {
                    long location = offset - cacheItem.Offset;
                    if (offset >= cacheItem.Offset && offset < cacheItem.Offset + cacheItem.Data.Length && location + buffer.Length < cacheItem.DataSize)
                    {
                        for (int i = 0; i < buffer.Length; i++)
                            buffer[i] = cacheItem.Data[i + location];
                        LogManager.GetLogger("faplog").Trace("Dokan ReadFile (cached) {0} Size {1} Offset {2}", filename, buffer.Length, offset);
                        readBytes = (uint)buffer.Length;
                        return DokanNet.DOKAN_SUCCESS;
                    }

                    
                }
                //Check Read type
                switch (cacheItem.Type)
                {
                    case ReadAheadCacheType.Unset:
                        //If a large buffer is being used its likely we are doing a copy
                        if (buffer.Length > LARGE_READ_SIZE) // 960kb
                            cacheItem.Type = ReadAheadCacheType.Sequential;
                        else
                            cacheItem.Type = ReadAheadCacheType.Random;

                        LogManager.GetLogger("faplog").Error("Dokan ReadFile new {0} {1}", cacheItem.Type.ToString(), filename);
                        break;
                    case ReadAheadCacheType.Sequential:
                        if (cacheItem.Offset != offset)
                        {
                            cacheItem.Response.GetResponseStream().Close();
                            cacheItem.Response.GetResponseStream().Dispose();
                            cacheItem.Response.Close();
                            cacheItem.Response = null;
                            cacheItem.Type = ReadAheadCacheType.Random;
                            cacheItem.SequentialReads = 0;
                            LogManager.GetLogger("faplog").Error("Dokan ReadFile To Seq {0} {1}", cacheItem.Type.ToString(), filename);
                        }
                        break;
                    case ReadAheadCacheType.Random:
                        if (cacheItem.SequentialReads > 4)
                        {
                            cacheItem.Type = ReadAheadCacheType.Sequential;
                            cacheItem.DataSize = 0;
                            cacheItem.Data = new byte[0];
                            cacheItem.Offset = 0;
                            LogManager.GetLogger("faplog").Error("Dokan ReadFile To Random {0} {1}", cacheItem.Type.ToString(), filename);
                        }
                        break;

                }
                try
                {
                    string path;
                    var node = ParsePath(filename, out path);
                    if (null != node)
                    {
                        string url = path.Replace('\\', '/');
                        if (!url.StartsWith("/"))
                            url = "/" + url;


                        if (cacheItem.Type == ReadAheadCacheType.Sequential)
                        {
                            if (null == cacheItem.Response)
                            {
                                //Create new download session
                                var req = (HttpWebRequest)
                                      WebRequest.Create(getDownloadUrl(node) + url);
                                req.Headers.Add("FAP-SOURCE", _model.LocalNode.ID);
                                MethodInfo method = typeof(WebHeaderCollection).GetMethod("AddWithoutValidate",
                                                                                           BindingFlags.Instance |
                                                                                           BindingFlags.NonPublic);
                                string key = "Range";
                                string val = string.Format("bytes={0}", offset);
                                method.Invoke(req.Headers, new object[] { key, val });
                                cacheItem.Response = (HttpWebResponse)req.GetResponse();
                                cacheItem.Offset = 0;
                            }

                            if (cacheItem.Response.StatusCode == HttpStatusCode.OK || cacheItem.Response.StatusCode == HttpStatusCode.PartialContent)
                            {
                                var stream = cacheItem.Response.GetResponseStream();
                                int read = 0;

                                while (true)
                                {
                                    read += stream.Read(buffer, read, buffer.Length - read);
                                    //Reached full buffer or end of file
                                    if (read == buffer.Length || cacheItem.Response.ContentLength == cacheItem.Offset + read)
                                        break;
                                }

                                cacheItem.Expires = DateTime.Now + BROWSE_CACHE_TIME;
                                cacheItem.Offset += read;
                                readBytes = (uint)read;
                                return DokanNet.DOKAN_SUCCESS;
                            }
                            else
                            {
                                cacheItem.Type = ReadAheadCacheType.Unset;
                                cacheItem.Response.GetResponseStream().Close();
                                cacheItem.Response.GetResponseStream().Dispose();
                                cacheItem.Response.Close();
                            }
                        }
                        else
                        {
                            //Random access read
                            var req = (HttpWebRequest)
                                      WebRequest.Create(getDownloadUrl(node) + url);
                            req.Headers.Add("FAP-SOURCE", _model.LocalNode.ID);
                            req.Pipelined = true;
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

                            using (var resp = (HttpWebResponse)req.GetResponse())
                            {

                                if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.PartialContent)
                                {
                                    //Check for sequential reads
                                    if ((cacheItem.Offset + cacheItem.DataSize == offset || cacheItem.Offset + buffer.Length == offset) && cacheItem.DataSize != 0)
                                        cacheItem.SequentialReads++;
                                    else
                                        cacheItem.SequentialReads = 0;

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

                                        cacheItem.Expires = DateTime.Now + BROWSE_CACHE_TIME;

                                        {
                                            int read = 0;
                                            for (long i = 0; i < cacheItem.DataSize && i < buffer.Length; i++)
                                            {
                                                buffer[i] = cacheItem.Data[i];
                                                read++;
                                            }

                                            //Do not save oversize buffers
                                            /* if (cacheItem.Data.Length > LARGE_READ_SIZE)
                                             {
                                                 cacheItem.Data = new byte[0];
                                                 cacheItem.DataSize = 0;
                                                 cacheItem.Expires = DateTime.MinValue;
                                                 LogManager.GetLogger("faplog").Error("Dokan ReadFile Clear large buffer");
                                             }
                                             else*/
                                            cacheItem.Expires = DateTime.Now + BROWSE_CACHE_TIME;

                                            readBytes = (uint)read;
                                            LogManager.GetLogger("faplog").Error("Dokan ReadFile Random buffer {0} {1} :: {2}", cacheItem.Offset, cacheItem.Offset + cacheItem.DataSize, cacheItem.SequentialReads);
                                            return DokanNet.DOKAN_SUCCESS;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch {
                    //Clear down
                    cacheItem.Response = null;
                    cacheItem.Offset = 0;
                    cacheItem.DataSize = 0;
                    cacheItem.Data = new byte[0];
                    cacheItem.Expires = DateTime.MinValue;
                }
            }
            finally
            {
                Monitor.Exit(cacheItem.Lock);
            }
            return DokanNet.ERROR_FILE_NOT_FOUND;
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
                    cache = new BrowsingCache() { Expires = DateTime.MinValue};
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

                if (cache.Expires > DateTime.Now)
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
                        cache.Expires = DateTime.Now + BROWSE_CACHE_TIME;
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
            //LogManager.GetLogger("faplog").Trace("Dokan OpenDirectory {0}",filename);
            if (null != GetDirectoryWithCache(filename))
                return DokanNet.DOKAN_SUCCESS;
            return DokanNet.ERROR_PATH_NOT_FOUND;
        }

        public int CreateDirectory(string filename, DokanFileInfo info)
        {
            return DokanNet.ERROR_ACCESS_DENIED;
        }

        public int Cleanup(string filename, DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        public int CloseFile(string filename, DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
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
            return DokanNet.DOKAN_ERROR;
        }

        public int FlushFileBuffers(string filename, DokanFileInfo info)
        {
            return DokanNet.DOKAN_ERROR;
        }

        public int GetFileInformation(string fullPath, FileInformation fileinfo, DokanFileInfo info)
        {
            //LogManager.GetLogger("faplog").Trace("Dokan GetFileInformation {0}", fullPath);

            string fileName = Path.GetFileName(fullPath);
            string path = Path.GetDirectoryName(fullPath);

            if (string.IsNullOrEmpty(fileName))
            {
                string[] split = new string[0];
                if (null != path)
                    split = path.Split("\\".ToArray(), StringSplitOptions.RemoveEmptyEntries);

                if (split.Length == 0)
                {
                    //Root dir
                    fileinfo.Attributes = FileAttributes.Directory;
                    fileinfo.CreationTime = DateTime.Now;
                    fileinfo.FileName = "\\";
                    fileinfo.LastAccessTime = DateTime.Now;
                    fileinfo.LastWriteTime = DateTime.Now;
                    fileinfo.Length = 0;
                    return DokanNet.DOKAN_SUCCESS;
                }
                else
                {
                    fileName = split[split.Length - 1];
                    path = path.Substring(0, path.Length - fileName.Length);
                }
            }


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
            //LogManager.GetLogger("faplog").Trace("Dokan FindFiles {0}", path);

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
            return DokanNet.DOKAN_ERROR;
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
            return DokanNet.DOKAN_SUCCESS;
        }

        public int UnlockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
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
            return DokanNet.DOKAN_SUCCESS;
        }
        #endregion

        public class BrowsingCache
        {
            public DateTime Expires { set; get; }
            public List<BrowsingFile> Info { set; get; }
            public object Lock = new object();
        }

        public class ReadAheadCache
        {
            public ReadAheadCache() { Type = ReadAheadCacheType.Unset; }

            public ReadAheadCacheType Type { set; get; }
            public int SequentialReads { set; get; }
            //Shared
            public long Offset { set; get; }

            //Random
            public DateTime Expires { set; get; }
            public byte[] Data { set; get; }
            
            public int DataSize { set; get; }
            //Sequential
            public HttpWebResponse Response { set; get; }

            public object Lock = new object();
        }

        public enum ReadAheadCacheType {Random,Sequential,Unset};

   }
}
