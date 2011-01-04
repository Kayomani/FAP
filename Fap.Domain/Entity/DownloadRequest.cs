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
using System.IO;
using System.Xml.Serialization;

namespace Fap.Domain.Entity
{
    public class DownloadRequest : INotifyPropertyChanged
    {
        private string host;
        private string path;
        private DateTime added;
        private bool isFolder;
        [XmlIgnore]
        private string status;
        [XmlIgnore]
        private int nextTryTime;
        
        [XmlIgnore]
        public int NextTryTime
        {
            set
            {
                nextTryTime = value;
                NotifyChange("NextTryTime");
            }
            get
            {
                return nextTryTime;
            }
        }
        [XmlIgnore]
        public string Status
        {
            set
            {
                status = value;
                NotifyChange("Status");
            }
            get
            {
                return status;
            }
        }


        public bool IsFolder
        {
            set
            {
                isFolder = value;
                NotifyChange("IsFolder");
            }
            get
            {
                return isFolder;
            }
        }


        public string Host
        {
            set
            {
                host = value;
                NotifyChange("Host");
            }
            get
            {
                return host;
            }
        }

        /// <summary>
        /// Path inside the download folder
        /// </summary>
        public string LocalPath
        {
            set;
            get;
        }

        public string FullPath
        {
            set
            {
                path = value;
                NotifyChange("FullPath");
                NotifyChange("FolderPath");
                NotifyChange("FileName");
            }
            get
            {
                return path;
            }
        }

        [XmlIgnore]
        public string FolderPath
        {
            get
            {
                int length = FileName.Length;
                if (FullPath.Length > length)
                {
                    return path.Substring(0, path.Length - length);
                }
                return FullPath;
            }
        }

        [XmlIgnore]
        public string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(FullPath))
                    return string.Empty;
                return Path.GetFileName(FullPath);
            }
        }

        public DateTime Added
        {
            set
            {
                added = value;
                NotifyChange("Added");
            }
            get
            {
                return added;
            }
        }

        private void NotifyChange(string path)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(path));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
