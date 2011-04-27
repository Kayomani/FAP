using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using FAP.Domain.Services;

namespace FAP.Domain.Entities.FileSystem
{
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
