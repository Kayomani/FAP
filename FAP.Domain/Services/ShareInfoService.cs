#region Copyright Kayomani 2011.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.

/**
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or any 
    later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * */

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using FAP.Domain.Entities;
using FAP.Domain.Entities.FileSystem;
using Fap.Foundation;
using Fap.Foundation.Sorting;
using NLog;
using Directory = FAP.Domain.Entities.FileSystem.Directory;
using File = System.IO.File;

namespace FAP.Domain.Services
{
    public class RootShare
    {
        public string ID { set; get; }
        public Directory Data { set; get; }
    }

    public class ShareInfoService
    {
        public static readonly string SaveLocation =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\FAP\ShareInfo\";

        private readonly Model model;
        private readonly List<RootShare> shares = new List<RootShare>();

        public ShareInfoService(Model m)
        {
            model = m;
        }

        public void Load()
        {
            shares.Clear();

            foreach (Share share in model.Shares.ToList())
            {
                try
                {
                    var d = new Directory();
                    d.Load(share.ID);
                    shares.Add(new RootShare {ID = share.ID, Data = d});
                }
                catch (Exception e)
                {
                    LogManager.GetLogger("faplog").Debug("Failed to load share info for" + share.Name, e);
                    ThreadPool.QueueUserWorkItem(DoRefreshPath, share);
                }
            }
        }

        private void DoRefreshPath(object o)
        {
            var s = o as Share;
            if (null != s)
                RefreshPath(s);
        }

        public void RenameShareByID(string ID, string destinationName)
        {
            RootShare search = shares.Where(s => s.ID == ID).FirstOrDefault();
            if (null != search)
            {
                search.Data.Name = destinationName;
                search.Data.Save(ID);
            }
        }

        public Directory RefreshPath(Share share)
        {
            try
            {
                model.GetAntiShutdownLock();
                lock (share)
                {
                    RootShare rs = shares.Where(s => s.ID == share.ID).FirstOrDefault();
                    if (null == rs)
                    {
                        rs = new RootShare();
                        rs.ID = share.ID;
                        rs.Data = new Directory();
                        shares.Add(rs);
                    }
                    rs.Data.Name = share.Name;
                    rs.Data.Size = 0;
                    rs.Data.ItemCount = 0;

                    RefreshFileInfo(new DirectoryInfo(share.Path), rs.Data);
                    try
                    {
                        rs.Data.Save(share.ID);
                    }
                    catch (Exception e)
                    {
                        LogManager.GetLogger("faplog").Warn("Failed save share info for " + share.Name, e);
                    }
                    return rs.Data;
                }
            }
            finally
            {
                model.ReleaseAntiShutdownLock();
            }
        }

        public void RemoveShareByID(string id)
        {
            RootShare search = shares.Where(s => s.ID == id).FirstOrDefault();
            if (null != search)
                shares.Remove(search);
            string path = SaveLocation + Convert.ToBase64String(Encoding.UTF8.GetBytes(id)) + ".cache";
            if (File.Exists(path))
                File.Delete(path);
        }


        /// <summary>
        /// Warning, ensure to clean any returned directory that is virtual to prevent a memory leak
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isVirtual"></param>
        /// <returns></returns>
        public Directory GetPath(string path, out bool isVirtual)
        {
            isVirtual = false;

            if (path.StartsWith("/"))
                path = path.Substring(1);
            string[] items = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

            if (items.Length == 0)
                return null;


            switch (shares.Where(n => n.Data != null && n.Data.Name == items[0]).Count())
            {
                case 0:
                    //Dir not found
                    return null;
                case 1:
                    {
                        //Single directory
                        Directory dir = shares.Where(n => n.Data.Name == items[0]).FirstOrDefault().Data;
                        for (int i = 1; i < items.Length; i++)
                        {
                            if (null == dir)
                                break;
                            dir = dir.SubDirectories.Where(d => d.Name == items[i]).FirstOrDefault();
                        }
                        return dir;
                    }
                default:
                    //Multiple directories - Return a virtual directory
                    //Only return data if we find the path atleast once
                    bool foundPath = false;
                    var virtualDir = new Directory();

                    //Scan each share and add info
                    foreach (RootShare src in shares.Where(n => null != n.Data && n.Data.Name == items[0]))
                    {
                        Directory dir = src.Data;
                        for (int i = 1; i < items.Length; i++)
                        {
                            if (null == dir)
                                break;
                            dir = dir.SubDirectories.Where(d => d.Name == items[i]).FirstOrDefault();
                        }
                        if (null != dir)
                        {
                            virtualDir.Files.AddRange(dir.Files);
                            virtualDir.SubDirectories.AddRange(dir.SubDirectories);
                            virtualDir.Name = dir.Name;
                            foundPath = true;
                        }
                    }

                    if (!foundPath)
                        return null;
                    //Create stats
                    virtualDir.ItemCount = virtualDir.Files.Count + virtualDir.SubDirectories.Count;
                    virtualDir.LastModified = DateTime.Now.ToFileTime();
                    virtualDir.Size = virtualDir.Files.Sum(f => f.Size) + virtualDir.SubDirectories.Sum(d => d.Size);
                    //Sort output
                    virtualDir.Files = virtualDir.Files.OrderBy(s => s.Name).ToList();
                    virtualDir.SubDirectories = virtualDir.SubDirectories.OrderBy(s => s.Name).ToList();

                    isVirtual = true;
                    return virtualDir;
            }
        }

        /// <summary>
        /// Retrieve file system information for a path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="noCache"></param>
        /// <param name="distinct"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        public bool GetPath(string path, bool noCache, bool distinct, out List<BrowsingFile> results)
        {
            results = new List<BrowsingFile>();

            //At the root so just return a list of shares
            if (string.IsNullOrEmpty(path) || path == "/")
            {
                var ms = from s in model.Shares
                             orderby s.Name
                             group s by s.Name
                                 into g
                                 select new
                                 {
                                     Name = g.Key,
                                     Size = g.Sum(s => s.Size),
                                     LastModified = g.Count() > 1 ? DateTime.Now : g.First().LastRefresh
                                 };

                foreach (var share in ms)
                {
                    results.Add(new BrowsingFile()
                                    {
                                        IsFolder = true,
                                        Size = share.Size,
                                        LastModified = share.LastModified,
                                        Name = share.Name
                                    });
                }

                return true;
            }

            string[] posiblePaths;
            bool isVirtual = false;

            if (ToLocalPath(path, out posiblePaths))
            {
                Directory scanInfo = GetPath(path, out isVirtual);
                //Have cache info and cache allowed
                if (null != scanInfo && !noCache)
                {
                    foreach (var dir in scanInfo.SubDirectories)
                    {
                        results.Add(new BrowsingFile
                                        {
                                            IsFolder = true,
                                            Size = dir.Size,
                                            Name = dir.Name,
                                            LastModified = DateTime.FromFileTime(dir.LastModified)
                                        });
                    }

                    foreach (var file in scanInfo.Files)
                    {
                        results.Add(new BrowsingFile
                                        {
                                            IsFolder = false,
                                            Size = file.Size,
                                            Name = file.Name,
                                            LastModified = DateTime.FromFileTime(file.LastModified)
                                        });
                    }

                    //Virtual info so clear down to ensure GC works.
                    if (isVirtual)
                    {
                        scanInfo.SubDirectories.Clear();
                        scanInfo.Files.Clear();
                    }
                }
                else
                {
                    //No cache info or cache not allowed, try to pull file information directly.

                    foreach (string posiblePath in posiblePaths)
                    {
                        string fsPath = posiblePath.Replace('/', '\\');
                        if (!fsPath.EndsWith("\\"))
                            fsPath += "\\";

                        //Check for parent folder usage.
                        string checkedPath = Path.GetFullPath(fsPath);
                        //If the evaluated path is different then someone tried to use '..' or similar.
                        if (fsPath != checkedPath)
                            continue;

                        try
                        {
                            var directory = new DirectoryInfo(checkedPath);
                            DirectoryInfo[] directories = directory.GetDirectories();
                            //Get directories
                            foreach (DirectoryInfo dir in directories)
                            {
                                results.Add(new BrowsingFile
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
                                results.Add(new BrowsingFile
                                                {
                                                    IsFolder = false,
                                                    Size = file.Length,
                                                    Name = file.Name,
                                                    LastModified = file.LastWriteTime
                                                });
                            }
                        }
                        catch { }
                    }

                    if (posiblePaths.Length > 1)
                        isVirtual = true;
                }

                if (distinct)
                {
                    //Check for file folder overlap where multiple directories were used.

                    var folders = new TreeSort<BrowsingFile>();
                    var files = new TreeSort<BrowsingFile>();

                    foreach (var browsingFile in results)
                    {
                        if (browsingFile.IsFolder)
                        {
                            var search = folders.GetValue(browsingFile.Name);
                            if (search.Count == 0)
                            {
                                //Folder not found so add it to the list.
                                folders.PutValue(browsingFile.Name, browsingFile);
                            }
                            else
                            {
                                //Folder already exists, append info.
                                var bf = search.First();
                                bf.Size += browsingFile.Size;
                                if (bf.LastModified < browsingFile.LastModified)
                                    bf.LastModified = browsingFile.LastModified;
                            }

                        }
                        else
                        {
                            var search = files.GetValue(browsingFile.Name);
                            //Only add file the first instance of a file.  Duplicates are hidden.
                            if (search.Count == 0)
                                files.PutValue(browsingFile.Name, browsingFile);
                        }
                    }

                    //Clear existing results
                    results.Clear();
                    //Re-add distinct sorted results.
                    
                    //Folders
                    var fsort = folders.GetAllNodes();
                    for(int i=fsort.Count-1;i>=0;i--)
                    {
                         var search = fsort[i].Values.FirstOrDefault();
                        if (null != search)
                            results.Add(search);
                    }

                  

                    //Files
                    var filesort = files.GetAllNodes();
                    for (int i = filesort.Count - 1; i >= 0; i--)
                    {
                        var search = filesort[i].Values.FirstOrDefault();
                        if (null != search)
                            results.Add(search);
                    }
                }
                return true;
            }

            return false;
        }

        public bool ToLocalPath(string input, out string[] output)
        {
            var result = new List<string>();
            string[] split = input.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length > 0)
            {
                foreach (Share root in model.Shares.Where(s => s.Name == split[0]))
                {
                    var sb = new StringBuilder();
                    sb.Append(root.Path);
                    if (!root.Path.EndsWith("\\"))
                        sb.Append("\\");
                    for (int i = 1; i < split.Length; i++)
                    {
                        sb.Append(split[i]);
                        if (i + 1 < split.Length)
                            sb.Append("\\");
                    }
                    result.Add(sb.ToString());
                }


                if (result.Count > 0)
                {
                    output = result.ToArray();
                    return true;
                }
            }
            output = new string[0];
            return false;
        }

        public long GetSize(string path)
        {
            bool isVirtual = false;
            Directory info = GetPath(path, out isVirtual);
            if (null == info)
                return 0;
            if (isVirtual)
            {
                info.SubDirectories.Clear();
                info.Files.Clear();
            }
            return info.Size;
        }


        /// <summary>
        /// Refresh memory cache with information about the actual files
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="model"></param>
        private void RefreshFileInfo(DirectoryInfo directory, Directory model)
        {
            try
            {
                long newSize = 0;
                long objCount = 0;

                //Check file list
                var newFileList = new List<Entities.FileSystem.File>();
                FileInfo[] sysfiles = directory.GetFiles();
                foreach (FileInfo finfo in sysfiles)
                {
                    newSize += finfo.Length;
                    objCount++;

                    var cf = new Entities.FileSystem.File();
                    cf.Name = finfo.Name;
                    cf.Size = finfo.Length;
                    cf.LastModified = finfo.LastWriteTime.ToFileTime();
                    newFileList.Add(cf);
                }
                //Update model with file info
                List<Entities.FileSystem.File> oldList = model.Files;
                model.Files = newFileList;
                oldList.Clear();
                sysfiles = null;

                //Check folder info.
                var newDirList = new List<Directory>();
                var oldDirs = new Dictionary<string, Directory>();

                foreach (Directory d in model.SubDirectories)
                    oldDirs.Add(d.Name, d);

                DirectoryInfo[] dirs = directory.GetDirectories();
                //Add,refresh
                foreach (DirectoryInfo dir in dirs)
                {
                    Directory sub = null;
                    if (oldDirs.ContainsKey(dir.Name))
                        sub = oldDirs[dir.Name];
                    else
                    {
                        sub = new Directory();
                        sub.Name = dir.Name;
                    }

                    sub.LastModified = dir.LastWriteTime.ToFileTime();
                    RefreshFileInfo(dir, sub);
                    newDirList.Add(sub);
                    //Add totals to the parent
                    objCount += sub.ItemCount;
                    newSize += sub.Size;
                }
                //Update model
                List<Directory> oldDirList = model.SubDirectories;
                model.SubDirectories = newDirList;
                oldDirList.Clear();

                //Sum totals
                model.Size = newSize;
                model.ItemCount = objCount;
            }
            catch
            {
            }
        }

        #region Search

        public List<SearchResult> Search(string expression, int limit, long modifiedBefore, long modifiedAfter,
                                         double smallerThan, double largerThan)
        {
            var results = new List<SearchResult>();

            var matcher = new StringMatcher(expression);

            foreach (RootShare share in shares.ToList())
            {
                if (
                    !SearchRecursive(share.Data, matcher, string.Empty, results, limit, modifiedBefore, modifiedAfter,
                                     smallerThan, largerThan))
                    break;
            }
            return results;
        }

        private bool SearchRecursive(Directory dir, StringMatcher matcher, string currentPath,
                                     List<SearchResult> results, int limit, long modifiedBefore, long modifiedAfter,
                                     double smallerThan, double largerThan)
        {
            foreach (Entities.FileSystem.File file in dir.Files)
            {
                if (matcher.IsMatch(file.Name))
                {
                    if ((modifiedBefore == 0 || file.LastModified < modifiedBefore) &&
                        (modifiedAfter == 0 || file.LastModified > modifiedAfter) &&
                        (smallerThan == 0 || file.Size < smallerThan) &&
                        (largerThan == 0 || file.Size > largerThan))
                    {
                        results.Add(new SearchResult
                                        {
                                            FileName = file.Name,
                                            Modified = DateTime.FromFileTime(file.LastModified),
                                            Path =
                                                string.IsNullOrEmpty(currentPath)
                                                    ? dir.Name
                                                    : currentPath + "/" + dir.Name,
                                            IsFolder = false,
                                            Size = file.Size
                                        });


                        if (results.Count >= limit)
                            return false;
                    }
                }
            }

            foreach (Directory d in dir.SubDirectories)
            {
                if (matcher.IsMatch(d.Name))
                {
                    if ((modifiedBefore == 0 || d.LastModified < modifiedBefore) &&
                        (modifiedAfter == 0 || d.LastModified > modifiedAfter) &&
                        (smallerThan == 0 || d.Size < smallerThan) &&
                        (largerThan == 0 || d.Size > largerThan))
                    {
                        results.Add(new SearchResult
                                        {
                                            FileName = d.Name,
                                            Modified = DateTime.FromFileTime(d.LastModified),
                                            Path =
                                                string.IsNullOrEmpty(currentPath)
                                                    ? dir.Name
                                                    : currentPath + "/" + dir.Name,
                                            IsFolder = true,
                                            Size = d.Size
                                        });
                        if (results.Count >= limit)
                            return false;
                    }
                }
            }

            foreach (Directory subdir in dir.SubDirectories)
            {
                if (string.IsNullOrEmpty(currentPath))
                {
                    if (
                        !SearchRecursive(subdir, matcher, dir.Name, results, limit, modifiedBefore, modifiedAfter,
                                         smallerThan, largerThan))
                        return false;
                }
                else
                {
                    if (
                        !SearchRecursive(subdir, matcher, currentPath + "/" + dir.Name, results, limit, modifiedBefore,
                                         modifiedAfter, smallerThan, largerThan))
                        return false;
                }
            }
            return true;
        }

        public class StringMatcher
        {
            private readonly string[] split;

            public StringMatcher(string expression)
            {
                if (string.IsNullOrEmpty(expression))
                    split = new string[0];
                else
                    split = expression.Split(new[] {'*'}, StringSplitOptions.RemoveEmptyEntries);
            }

            public bool IsMatch(string name)
            {
                int index = 0;

                for (int i = 0; i < split.Length; i++)
                {
                    //Performance comparision:
                    //http://blogs.msdn.com/b/noahc/archive/2007/06/29/string-equals-performance-comparison.aspx
                    index = name.IndexOf(split[i], index, StringComparison.OrdinalIgnoreCase);
                    if (-1 == index)
                        return false;
                }
                return index != -1;
            }
        }

        #endregion
    }
}