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
using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.IO;

namespace FAP.Domain.Entities
{
    [DataContract]  
    public class BaseEntity : INotifyPropertyChanged
    {
        protected readonly string DATA_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\FAP\";
        private static readonly string BACKUP_EXT = ".bak";
        protected void NotifyChange(string path)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(path));
        }

        protected void SafeSave(object o, string fileName, Formatting f)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new Exception("Unable to save as no filename was specified.");
            if (!Directory.Exists(DATA_FOLDER))
                Directory.CreateDirectory(DATA_FOLDER);

            var obj = JsonConvert.SerializeObject(o, f);

            File.WriteAllText(DATA_FOLDER + fileName, obj);
            File.WriteAllText(DATA_FOLDER + fileName + BACKUP_EXT, obj);
            obj = null;
        }

        protected T SafeLoad<T>(string fileName)
        {
            try
            {
                if (File.Exists(DATA_FOLDER + fileName))
                    return  JsonConvert.DeserializeObject<T>(File.ReadAllText(DATA_FOLDER + fileName));
            }
            catch { }

            try
            {
                if (File.Exists(DATA_FOLDER + fileName + BACKUP_EXT))
                    return JsonConvert.DeserializeObject<T>(File.ReadAllText(DATA_FOLDER + fileName + BACKUP_EXT));
            }
            catch { }
            throw new Exception("Unable to read " + fileName);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
