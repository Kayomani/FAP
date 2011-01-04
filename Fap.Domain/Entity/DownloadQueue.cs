#region Copyright Kayomani 2010.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
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
using Fap.Foundation;
using System.IO;
using System.Xml.Serialization;

namespace Fap.Domain.Entity
{
    [Serializable]
    public class DownloadQueue
    {
        private SafeObservable<DownloadRequest> queue = new SafeObservable<DownloadRequest>();

        private readonly string saveLocation;


        public DownloadQueue()
        {
            saveLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\FAP\DownloadQueue.xml";
        }


        public SafeObservable<DownloadRequest> List
        {
            get
            {
                return queue;
            }
        }

        public void Save()
        {
            if (!Directory.Exists(Path.GetDirectoryName(saveLocation)))
                Directory.CreateDirectory(Path.GetDirectoryName(saveLocation));

            XmlSerializer serializer = new XmlSerializer(typeof(DownloadQueue));
            using (TextWriter textWriter = new StreamWriter(saveLocation))
            {
                serializer.Serialize(textWriter, this);
                textWriter.Flush();
                textWriter.Close();
            }
        }

        public void Load()
        {
            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(DownloadQueue));
                using (TextReader textReader = new StreamReader(saveLocation))
                {
                    DownloadQueue m = (DownloadQueue)deserializer.Deserialize(textReader);
                    textReader.Close();
                    queue = m.List;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to read download queue,", e);
            }
        }
    }
}
