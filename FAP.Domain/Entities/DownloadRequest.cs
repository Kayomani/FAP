using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace FAP.Domain.Entities
{
    public class DownloadRequest : BaseEntity
    {
        private string clientid;
        private string nickname;
        private string path;
        private long size;
        private DateTime added;
        private bool isFolder;
        private int nextTryTime;
        private DownloadRequestState state;
        private string localPath;

        public string LocalPath
        {
            set
            {
                localPath = value;
                NotifyChange("LocalPath");
            }
            get
            {
                return localPath;
            }
        }

        public long Size
        {
            set
            {
                size = value;
                NotifyChange("Size");
            }
            get
            {
                return size;
            }
        }

        [XmlIgnore]
        public DownloadRequestState State
        {
            set
            {
                state = value;
                NotifyChange("State");
            }
            get
            {
                return state;
            }
        }

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

        public string ClientID
        {
            set
            {
                clientid = value;
                NotifyChange("ClientID");
            }
            get
            {
                return clientid;
            }
        }

        public string Nickname
        {
            set
            {
                nickname = value;
                NotifyChange("Nickname");
            }
            get
            {
                return nickname;
            }
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
    }
}
