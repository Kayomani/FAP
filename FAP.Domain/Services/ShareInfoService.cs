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
using System.Linq;
using System.Text;
using FAP.Domain.Entities.FileSystem;
using System.IO;
using File = FAP.Domain.Entities.FileSystem.File;
using Directory = FAP.Domain.Entities.FileSystem.Directory;
using NLog;
using FAP.Domain.Entities;
using System.Text.RegularExpressions;
using System.Threading;

namespace FAP.Domain.Services
{
    public class ShareInfoService
    {
        private Dictionary<string, Directory> shares = new Dictionary<string, Directory>();
        public static readonly string SaveLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\FAP\ShareInfo\";
        private Model model;

        public ShareInfoService(Model m)
        {
            model = m;
        }

        public void Load()
        {
            shares.Clear();
            try
            {
                if (System.IO.Directory.Exists(SaveLocation))
                {
                    foreach (var file in System.IO.Directory.GetFiles(SaveLocation))
                    {
                        if (file.EndsWith(".info"))
                        {
                            try
                            {
                                Directory d = new Directory();
                                d.Load(file);
                                shares.Add(d.Name, d);
                            }
                            catch (Exception e)
                            {
                                LogManager.GetLogger("faplog").DebugException("Failed to load share info from " + file, e);
                            }
                        }
                    }
                }
                //Check that each share has had info loaded for it
                foreach (var share in model.Shares)
                {
                    if (!shares.ContainsKey(share.Name))
                    {
                        //Share info has failed to load
                        ThreadPool.QueueUserWorkItem(new WaitCallback(DoRefreshPath), share);
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger("faplog").WarnException("Failed to load share info", e);
            }
        }

        private void DoRefreshPath(object o)
        {
            Share s = o as Share;
            if (null != s)
                RefreshPath(s);
        }

        public void RenameShare(string name, string destination)
        {
            Directory info = shares[name];
            shares.Remove(name);
            shares.Add(destination, info);

            info.Name = destination;
            RemoveShare(name);
            info.Save();
        }

        public Directory RefreshPath(Share share)
        {
            try
            {
                model.GetAntiShutdownLock();
                lock (share)
                {
                    Directory info = null;
                    if (shares.ContainsKey(share.Name))
                        info = shares[share.Name];
                    else
                    {
                        info = new Directory();
                        shares.Add(share.Name, info);
                    }
                    info.Name = share.Name;
                    info.Size = 0;
                    info.ItemCount = 0;
                    RefreshFileInfo(new DirectoryInfo(share.Path), info);
                    try
                    {
                        info.Save();
                    }
                    catch (Exception e)
                    {
                        LogManager.GetLogger("faplog").WarnException("Failed save share info for " + share.Name, e);
                    }
                    System.GC.Collect();
                    return info;
                }
            }
            finally
            {
                model.ReleaseAntiShutdownLock();
            }
        }

        public void RemoveShare(string name)
        {
            if (shares.ContainsKey(name))
                shares.Remove(name);
            string path = ShareInfoService.SaveLocation + Convert.ToBase64String(Encoding.UTF8.GetBytes(name)) + ".dat";
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }

        public Directory GetPath(string path)
        {
            if (path.StartsWith("/"))
                path = path.Substring(1);
            string[] items = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (items.Length == 0 || !shares.ContainsKey(items[0]))
                return null;
            Directory dir = shares[items[0]];
            for (int i = 1; i < items.Length; i++)
            {
                if (null == dir)
                    break;
                dir = dir.SubDirectories.Where(d => d.Name == items[i]).FirstOrDefault();
            }
            return dir;
        }

        public bool ToLocalPath(string input, out string output)
        {
            string[] split = input.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length > 0)
            {
                var share = model.Shares.Where(s => s.Name == split[0]).FirstOrDefault();
                if (null != share)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(share.Path);
                    if (!share.Path.EndsWith("\\"))
                        sb.Append("\\");
                    for (int i = 1; i < split.Length; i++)
                    {
                        sb.Append(split[i]);
                        if (i + 1 < split.Length)
                            sb.Append("\\");
                    }

                    output = sb.ToString();
                    return true;
                }
            }
            output = string.Empty;
            return false;
        }

        public long GetSize(string path)
        {
            var info = GetPath(path);
            if (null == info)
                return 0;
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
                List<File> newFileList = new List<File>();
                FileInfo[] sysfiles = directory.GetFiles();
                foreach (var finfo in sysfiles)
                {
                    newSize += finfo.Length;
                    objCount++;

                    File cf = new File();
                    cf.Name = finfo.Name;
                    cf.Size = finfo.Length;
                    cf.LastModified = finfo.LastWriteTime.ToFileTime();
                    newFileList.Add(cf);
                }
                //Update model with file info
                var oldList = model.Files;
                model.Files = newFileList;
                oldList.Clear();
                sysfiles = null;

                //Check folder info.
                List<Directory> newDirList = new List<Directory>();
                Dictionary<string, Directory> oldDirs = new Dictionary<string, Directory>();

                foreach (var d in model.SubDirectories)
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
                var oldDirList = model.SubDirectories;
                model.SubDirectories = newDirList;
                oldDirList.Clear();
                
                //Sum totals
                model.Size = newSize;
                model.ItemCount = objCount;
            }
            catch { }
        }

        #region Search
        public List<SearchResult> Search(string expression, int limit, long modifiedBefore, long modifiedAfter, double smallerThan, double largerThan)
        {
            List<SearchResult> results = new List<SearchResult>();

            StringMatcher matcher = new StringMatcher(expression);
            
            foreach (var share in shares.ToList())
            {
                if (!SearchRecursive(share.Value, matcher, string.Empty, results, limit,modifiedBefore,modifiedAfter,smallerThan,largerThan))
                    break;
            }
            return results;
        }

        private bool SearchRecursive(Directory dir, StringMatcher matcher, string currentPath, List<SearchResult> results, int limit, long modifiedBefore, long modifiedAfter, double smallerThan, double largerThan)
        {
            foreach (var file in dir.Files)
            {
                if (matcher.IsMatch(file.Name))
                {
                    if ((modifiedBefore == 0 || file.LastModified < modifiedBefore) &&
                     (modifiedAfter == 0 || file.LastModified > modifiedAfter) &&
                     (smallerThan == 0 || file.Size < smallerThan) &&
                     (largerThan == 0 || file.Size > largerThan))
                    {

                        results.Add(new SearchResult()
                        {
                            FileName = file.Name,
                            Modified = DateTime.FromFileTime(file.LastModified),
                            Path = string.IsNullOrEmpty(currentPath) ? dir.Name : currentPath + "/" + dir.Name,
                            IsFolder = false,
                            Size = file.Size
                        });


                        if (results.Count >= limit)
                            return false;
                    }
                }
            }

            foreach (var d in dir.SubDirectories)
            {
                if (matcher.IsMatch(d.Name))
                {
                    if ((modifiedBefore == 0 || d.LastModified < modifiedBefore) &&
                     (modifiedAfter == 0 || d.LastModified > modifiedAfter) &&
                     (smallerThan == 0 || d.Size < smallerThan) &&
                     (largerThan == 0 || d.Size > largerThan))
                    {
                        results.Add(new SearchResult()
                        {
                            FileName = d.Name,
                            Modified = DateTime.FromFileTime(d.LastModified),
                            Path = string.IsNullOrEmpty(currentPath) ? dir.Name : currentPath + "/" + dir.Name,
                            IsFolder = true,
                            Size = d.Size
                        });
                        if (results.Count >= limit)
                            return false;
                    }
                }
            }

            foreach (var subdir in dir.SubDirectories)
            {
                if (string.IsNullOrEmpty(currentPath))
                {
                    if (!SearchRecursive(subdir, matcher, dir.Name, results, limit, modifiedBefore, modifiedAfter, smallerThan, largerThan))
                        return false;
                }
                else
                {
                    if (!SearchRecursive(subdir, matcher, currentPath + "/" + dir.Name, results, limit, modifiedBefore, modifiedAfter, smallerThan, largerThan))
                        return false;
                }
            }
            return true;
        }

        public class StringMatcher
        {
            private string[] split;

            public StringMatcher(string expression)
            {
                if (string.IsNullOrEmpty(expression))
                    split = new string[0];
                else
                    split = expression.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
            }

            public bool IsMatch(string name)
            {
                int index = 0;

                for (int i = 0; i < split.Length; i++)
                {
                    //Performance comparision:
                    //http://blogs.msdn.com/b/noahc/archive/2007/06/29/string-equals-performance-comparison.aspx
                    index = name.IndexOf(split[i], index,StringComparison.OrdinalIgnoreCase);
                    if (-1 == index)
                        return false;
                }
                return index != -1;
            }
        }
        #endregion
    }
}
