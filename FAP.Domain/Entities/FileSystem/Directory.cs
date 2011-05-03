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
using System.IO;
using System.Xml.Serialization;
using FAP.Domain.Services;
using Newtonsoft.Json;

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
            //Json save
            /* Json save is 100% slower but half the size
             * if (!System.IO.Directory.Exists(Path.GetDirectoryName(ShareInfoService.SaveLocation)))
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(ShareInfoService.SaveLocation));

            System.IO.File.WriteAllText(ShareInfoService.SaveLocation + Convert.ToBase64String(Encoding.Unicode.GetBytes(Name)) + ".dat", JsonConvert.SerializeObject(this));*/

            //XML save
            /*if (!System.IO.Directory.Exists(Path.GetDirectoryName(ShareInfoService.SaveLocation)))
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(ShareInfoService.SaveLocation));**/

            XmlSerializer serializer = new XmlSerializer(typeof(Directory));
            using (TextWriter textWriter = new StreamWriter(ShareInfoService.SaveLocation + Convert.ToBase64String(Encoding.Unicode.GetBytes(Name)) + ".dat"))
            {
                serializer.Serialize(textWriter, this);
                textWriter.Flush();
                textWriter.Close();
            }

           // using(Stream file = System.IO.File.Open(ShareInfoService.SaveLocation + Convert.ToBase64String(Encoding.Unicode.GetBytes(Name)) + ".dat",FileMode.OpenOrCreate))
           // {
           //     ProtoBuf.Serializer.Serialize<Directory>(file, this);
           // }
        }

        public void Load(string name)
        {
            try
            {
                //Json load
                /*Directory m = JsonConvert.DeserializeObject<Directory>(System.IO.File.ReadAllText(name));
                Name = m.Name;
                Size = m.Size;
                FileCount = m.FileCount;
                SubDirectories = m.SubDirectories;
                Files = m.Files;*/

                //Xml load
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

                /**using (Stream stream = System.IO.File.Open(name, FileMode.Open))
                {
                    Directory m = ProtoBuf.Serializer.Deserialize<Directory>(stream);
                    Name = m.Name;
                    Size = m.Size;
                    FileCount = m.FileCount;
                    SubDirectories = m.SubDirectories;
                    Files = m.Files;
                }**/
            }
            catch (Exception e)
            {
                //Error loading the file - May be the old format so just delete the old file
                try
                {
                    System.IO.File.Delete(name);
                }
                catch { }
                throw new Exception("Failed to read share info for " + name, e);
            }
        }
    }
}
