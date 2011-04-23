using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Fap.Domain.Entity;
using NLog;
using System.Xml.Serialization;

namespace Fap.Domain.Services
{
    public class ShareInfoService
    {
        private Dictionary<string, Directory> shares = new Dictionary<string, Directory>();
        public static readonly string SaveLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\FAP\ShareInfo\";


        public void Load()
        {
            shares.Clear();
            try
            {
                if (System.IO.Directory.Exists(SaveLocation))
                {
                    foreach (var file in System.IO.Directory.GetFiles(SaveLocation))
                    {
                        Directory d = new Directory();
                        d.Load(file);
                        shares.Add(d.Name,d);
                    }
                }
            }
            catch(Exception e)
            {
                LogManager.GetLogger("faplog").WarnException("Failed to load share info", e);
            }
        }

        public Directory RefreshPath(Share share)
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
            info.FileCount = 0;
            info.Clean();
            GetDirectorySizeRecursive(new DirectoryInfo(share.Path), info);
            try
            {
                info.Save();
            }
            catch (Exception e)
            {
                LogManager.GetLogger("faplog").WarnException("Failed save share info for "+ share.Name, e);
            }
            System.GC.Collect();
            return info;
        }


        private void Test()
        {
            Dictionary<string, File> results = new Dictionary<string, File>();

            string name = "james";

            foreach (var share in shares)
                SearchRecursive(share.Value, name, share.Value.Name, results);
        }


        private void SearchRecursive(Directory dir, string name, string currentPath, Dictionary<string, File> results)
        {

            foreach (var file in dir.Files)
            {
                if (file.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                    results.Add(currentPath + "/" + file.Name, file);
            }

            foreach (var d in dir.SubDirectories)
            {
                if (d.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                    results.Add(currentPath + "/" + d.Name, d);
            }

            foreach (var subdir in dir.SubDirectories)
                SearchRecursive(subdir, name, currentPath + "/" + dir.Name, results);
        }

        private void GetDirectorySizeRecursive(DirectoryInfo directory, Directory model)
        {
            try
            {
                // Examine all contained files.
                FileInfo[] files = directory.GetFiles();
                foreach (FileInfo file in files)
                {
                    model.Size += file.Length;
                    model.FileCount++;
                }
                //Get Directory info
                DirectoryInfo[] dirs = directory.GetDirectories();
                foreach (DirectoryInfo dir in dirs)
                {
                    Directory sub = new Directory();
                    sub.Name = dir.Name;
                    sub.LastModified = dir.LastWriteTime.ToFileTime();
                    GetDirectorySizeRecursive(dir, sub);
                    model.SubDirectories.Add(sub);
                    //Add totals ot the parent
                    model.FileCount += sub.FileCount;
                    model.Size += sub.Size;
                }
                foreach(var file in directory.GetFiles())
                {
                    File sub = new File();
                    sub.Name = file.Name;
                    sub.Size = file.Length;
                    sub.LastModified = file.LastWriteTime.ToFileTime();
                    model.Files.Add(sub);
                }
            }
            catch { }
        }

        public void RemoveShare(string name)
        {
            if (shares.ContainsKey(name))
                shares.Remove(name);
            string path = ShareInfoService.SaveLocation + Convert.ToBase64String(Encoding.Unicode.GetBytes(name)) + ".dat";
            if(System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }

        public Directory GetPath(string path)
        {
            string[] items = path.Split('\\');

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

        public long GetSize(string path)
        {
            var info = GetPath(path);
            if (null == info)
                return 0;
            return info.Size;
        }

        [Serializable]
        public class File
        {
            public string Name { set; get; }
            public long Size { set; get; }
            public long LastModified { set; get; }
        }

        [Serializable]
        public class Directory : File
        {
            public long FileCount { set; get; }
            public List<Directory> SubDirectories { set; get; }
            public List<File> Files { set; get; }
            public Directory() { SubDirectories = new List<Directory>(); Files = new List<File>(); }

            public void Clean()
            {
                foreach (var dir in SubDirectories)
                    dir.Clean();
                SubDirectories.Clear();
                Files.Clear();
            }

            public void Save()
            {
                if (!System.IO.Directory.Exists(Path.GetDirectoryName(ShareInfoService.SaveLocation)))
                    System.IO.Directory.CreateDirectory(Path.GetDirectoryName(ShareInfoService.SaveLocation));

                XmlSerializer serializer = new XmlSerializer(typeof(Directory));
                using (TextWriter textWriter = new StreamWriter(ShareInfoService.SaveLocation + Convert.ToBase64String(Encoding.Unicode.GetBytes(Name)) + ".dat"))
                {
                    serializer.Serialize(textWriter, this);
                    textWriter.Flush();
                    textWriter.Close();
                }
            }

            public void Load(string name)
            {
                try
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(Directory));
                    using (TextReader textReader = new StreamReader(name))
                    {
                        Directory m = (Directory)deserializer.Deserialize(textReader);
                        Name = m.Name;
                        Size = m.Size;
                        FileCount = m.FileCount;
                        SubDirectories = m.SubDirectories;
                        Files = m.Files;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to read config", e);
                }
            }
        }
    }
}
