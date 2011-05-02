using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Network.Entities;
using FAP.Domain.Entities.FileSystem;
using FAP.Domain.Entities;
using FAP.Domain.Services;
using System.IO;

namespace FAP.Domain.Verbs
{
    public class BrowseVerb : BaseVerb, IVerb
    {
        private ShareInfoService infoService;
        private Model model;

        public bool NoCache { set; get; }
        public string Path { set; get; }

        public BrowseVerb(Model m, ShareInfoService i)
        {
            model = m;
            infoService = i;
            Results = new List<BrowsingFile>();
        }


        public List<BrowsingFile> Results { set; get; }


        public NetworkRequest CreateRequest()
        {
            NetworkRequest req = new NetworkRequest();
            req.Verb = "BROWSE";
            req.Data = Serialize<BrowseVerb>(this);
            return req;
        }

        public NetworkRequest ProcessRequest(NetworkRequest r)
        {
            BrowseVerb verb = Deserialise<BrowseVerb>(r.Data);

            if (string.IsNullOrEmpty(verb.Path))
            {
                //If we are given no path then provide a list of virtual directories
                lock (model.Shares)
                {
                    foreach (var share in model.Shares.ToList().OrderBy(sh => sh.Name))
                    {
                        Results.Add(new BrowsingFile()
                                   {
                                       IsFolder = true,
                                       Name = share.Name,
                                       Size = share.Size,
                                       LastModified = share.LastRefresh
                                   });
                    }
                }
            }
            else
            {
                try
                {
                    string path = string.Empty;
                    if (infoService.ToLocalPath(verb.Path, out path))
                    {
                        var scanInfo = infoService.GetPath(verb.Path); ;

                        if (null != scanInfo && !verb.NoCache)
                        {
                            foreach (var dir in scanInfo.SubDirectories)
                            {
                                Results.Add(new BrowsingFile()
                                {
                                    IsFolder = true,
                                    Size = dir.Size,
                                    Name = dir.Name,
                                    LastModified = DateTime.FromFileTime(dir.LastModified)
                                });
                            }

                            foreach (var file in scanInfo.Files)
                            {
                                Results.Add(new BrowsingFile()
                                            {
                                                IsFolder = false,
                                                Size = file.Size,
                                                Name = file.Name,
                                                LastModified = DateTime.FromFileTime(file.LastModified)
                                            });
                            }
                        }
                        else
                        {
                            if (null != path)
                                path = path.Replace('/', '\\');
                            DirectoryInfo directory = new DirectoryInfo(path);
                            DirectoryInfo[] directories = directory.GetDirectories();
                            //Get directories
                            foreach (var dir in directories)
                            {
                                var info = scanInfo.SubDirectories.Where(d => d.Name == dir.Name).FirstOrDefault();
                                if (null != info)
                                {
                                    Results.Add(new BrowsingFile()
                                       {
                                           IsFolder = true,
                                           Size = info.Size,
                                           Name = dir.Name,
                                           LastModified = dir.LastWriteTime
                                       });
                                }
                                else
                                {
                                    Results.Add(new BrowsingFile()
                                    {
                                        IsFolder = true,
                                        Size = 0,
                                        Name = dir.Name,
                                        LastModified = dir.LastWriteTime
                                    });
                                }
                            }
                            //Get files
                            FileInfo[] files = directory.GetFiles();
                            foreach (var file in files)
                            {
                                Results.Add(new BrowsingFile()
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
                catch { }
            }

            r.Data = Serialize<BrowseVerb>(this);
            return r;
        }


        public bool ReceiveResponse(NetworkRequest r)
        {
            try
            {
                BrowseVerb verb = Deserialise<BrowseVerb>(r.Data);
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
    }
}
