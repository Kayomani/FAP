using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FAP.Domain.Entities;
using FAP.Domain.Entities.FileSystem;
using FAP.Domain.Services;
using FAP.Network.Entities;
using Directory = FAP.Domain.Entities.FileSystem.Directory;
using File = FAP.Domain.Entities.FileSystem.File;

namespace FAP.Domain.Verbs
{
    public class BrowseVerb : BaseVerb, IVerb
    {
        private readonly ShareInfoService infoService;
        private readonly Model model;

        public BrowseVerb(Model m, ShareInfoService i)
        {
            model = m;
            infoService = i;
            Results = new List<BrowsingFile>();
        }

        public bool NoCache { set; get; }
        public string Path { set; get; }


        public List<BrowsingFile> Results { set; get; }

        #region IVerb Members

        public NetworkRequest CreateRequest()
        {
            var req = new NetworkRequest();
            req.Verb = "BROWSE";
            req.Data = Serialize(this);
            return req;
        }

        public NetworkRequest ProcessRequest(NetworkRequest r)
        {
            var verb = Deserialise<BrowseVerb>(r.Data);

            if (string.IsNullOrEmpty(verb.Path))
            {
                //If we are given no path then provide a list of virtual directories
                lock (model.Shares)
                {
                    var sent = new List<string>();

                    var shares = from s in model.Shares
                                 orderby s.Name
                                 group s by s.Name
                                 into g
                                 select new
                                            {
                                                Name = g.Key,
                                                Size = g.Sum(s => s.Size),
                                                LastModified = g.Count() > 1 ? DateTime.Now : g.First().LastRefresh
                                            };


                    foreach (var share in shares)
                    {
                        Results.Add(new BrowsingFile
                                        {
                                            IsFolder = true,
                                            Name = share.Name,
                                            Size = share.Size,
                                            LastModified = share.LastModified
                                        });
                        sent.Add(share.Name);
                    }
                }
            }
            else
            {
                try
                {
                    string[] posiblePaths;

                    if (infoService.ToLocalPath(verb.Path, out posiblePaths))
                    {
                        bool isVirtual = false;
                        Directory scanInfo = infoService.GetPath(verb.Path, out isVirtual);

                        if (null != scanInfo && !verb.NoCache)
                        {
                            foreach (Directory dir in scanInfo.SubDirectories)
                            {
                                Results.Add(new BrowsingFile
                                                {
                                                    IsFolder = true,
                                                    Size = dir.Size,
                                                    Name = dir.Name,
                                                    LastModified = DateTime.FromFileTime(dir.LastModified)
                                                });
                            }

                            foreach (File file in scanInfo.Files)
                            {
                                Results.Add(new BrowsingFile
                                                {
                                                    IsFolder = false,
                                                    Size = file.Size,
                                                    Name = file.Name,
                                                    LastModified = DateTime.FromFileTime(file.LastModified)
                                                });
                            }

                            if (isVirtual)
                            {
                                scanInfo.SubDirectories.Clear();
                                scanInfo.Files.Clear();
                            }
                        }
                        else
                        {
                            foreach (string posiblePath in posiblePaths)
                            {
                                string fsPath = posiblePath.Replace('/', '\\');
                                var directory = new DirectoryInfo(fsPath);
                                DirectoryInfo[] directories = directory.GetDirectories();
                                //Get directories
                                foreach (DirectoryInfo dir in directories)
                                {
                                    Results.Add(new BrowsingFile
                                                    {
                                                        IsFolder = true,
                                                        Size = 0,
                                                        Name = dir.Name,
                                                        LastModified = dir.LastWriteTime
                                                    });
                                }
                                //Get files
                                FileInfo[] files = directory.GetFiles();
                                foreach (FileInfo file in files)
                                {
                                    Results.Add(new BrowsingFile
                                                    {
                                                        IsFolder = false,
                                                        Size = file.Length,
                                                        Name = file.Name,
                                                        LastModified = file.LastWriteTime
                                                    });
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            r.Data = Serialize(this);
            return r;
        }


        public bool ReceiveResponse(NetworkRequest r)
        {
            try
            {
                var verb = Deserialise<BrowseVerb>(r.Data);
                NoCache = verb.NoCache;
                Path = verb.Path;
                Results = verb.Results;
                return true;
            }
            catch
            {
            }
            return false;
        }

        #endregion
    }
}