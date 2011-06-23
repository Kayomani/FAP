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
        private readonly int FILE_READAHEAD_SIZE = 512000;//0.5mb
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
                {
                    cache = _browsingcache[path];
                    //Item is cached so just return it
                    if (cache.Expires > Environment.TickCount)
                    {
                        lock (cache.Lock)
                            return cache.Info.ToList();
                    }
                }
                else
                {
                    cache = new BrowsingCache() { Expires = Environment.TickCount + BROWSE_CACHE_TIME};
                    _browsingcache.Add(path, cache);
                }
                //Not cached, get lock and download.
                Monitor.Enter(cache.Lock);
            }
            finally
            {
                readLock.ExitReadLock();
            }
            try
            {

                string nodePath;
                var node = ParsePath(path, out nodePath);
                if (null != node)
                {
                    var c = new Client(_model.LocalNode);
                    var cmd = new BrowseVerb(null);
                    cmd.Path = "/" + nodePath.Replace('\\', '/');
                    cmd.NoCache = false;
                    LogManager.GetLogger("faplog").Warn("Dokan ## Get info for {0}", path);
                    if (c.Execute(cmd, node))
                    {
                        cache.Info = cmd.Results;
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

        public int ReadFile(string filename, byte[] buffer, ref uint readBytes, long offset, DokanFileInfo info)
        {
            LogManager.GetLogger("faplog").Trace("Dokan ReadFile {0}", filename);
            return DokanNet.DOKAN_ERROR;
            try
            {
                string path;
                var node = ParsePath(filename, out path);
                if (null != node)
                {
                    var req = (HttpWebRequest)
                              WebRequest.Create(Multiplexor.Encode(getDownloadUrl(node), "GET", path));
                    // req.UserAgent = Model.AppVersion;
                    // req.Headers.Add("FAP-SOURCE", model.LocalNode.ID);

                    // req.Timeout = 300000;
                    // req.ReadWriteTimeout = 3000000;
                    //If we are resuming then add range
                    if (offset != 0)
                    {
                        //Yes Micrsoft if you read this...  OH WHY IS ADDRANGE ONLY AN INT?? We live in an age where we might actually download more than 2gb
                        //req.AddRange(fileStream.Length);

                        //Hack
                        MethodInfo method = typeof (WebHeaderCollection).GetMethod("AddWithoutValidate",
                                                                                   BindingFlags.Instance |
                                                                                   BindingFlags.NonPublic);
                        string key = "Range";
                        string val = string.Format("bytes={0}", offset);
                        method.Invoke(req.Headers, new object[] {key, val});
                    }

                    var resp = (HttpWebResponse) req.GetResponse();

                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                    }
                }
            }
            catch
            {
            }
            return DokanNet.DOKAN_ERROR;
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
            if (null != info)
            {
                var search = bf.Where(f => f.Name == fileName).FirstOrDefault();
                if (null != search)
                {
                    fileinfo.Attributes = FileAttributes.Normal;
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
        }
   }
}
