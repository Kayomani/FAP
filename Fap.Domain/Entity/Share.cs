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
using System.ComponentModel;
using Fap.Foundation;
using System.Xml.Serialization;

namespace Fap.Domain.Entity
{
    [Serializable]
    public class Share : INotifyPropertyChanged
    {
        private string path;
        private string name;
        private long size;
        private long fileCount;
        [XmlIgnore]
        private object syncRoot = new object();
        private string status;

        public string Path
        {
            set { path = value; NotifyChange("Path");  }
            get { return path; }
        }
        public string Name
        {
            set { name = value; NotifyChange("Name"); }
            get { return name; }
        }

        public long Size
        {
            set { size = value; NotifyChange("Size"); NotifyChange("SizeString"); }
            get { return size; }
        }

        public long FileCount
        {
            set { fileCount = value; NotifyChange("FileCount"); }
            get { return fileCount; }
        }

        [XmlIgnore]
        public string Status
        {
            get { lock (syncRoot) return status; }
            set { lock (syncRoot) status = value; NotifyChange("Status"); }
        }

        public string SizeString
        {
            get
            {
                return Utility.FormatBytes(Size);
            }
        }

        

        private void NotifyChange(string path)
        {
            if(null!=PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(path));
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
