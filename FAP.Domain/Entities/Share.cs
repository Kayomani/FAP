using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAP.Domain.Entities
{
    public class Share : BaseEntity
    {
        private string path;
        private string name;
        private long size;
        private long fileCount;
        private string status;
        private DateTime refresh;

        public DateTime LastRefresh
        {
            set { refresh = value; NotifyChange("LastRefresh"); }
            get { return refresh; }
        }

        public string Path
        {
            set { path = value; NotifyChange("Path"); }
            get { return path; }
        }
        public string Name
        {
            set { name = value; NotifyChange("Name"); }
            get { return name; }
        }

        public long Size
        {
            set { size = value; NotifyChange("Size"); }
            get { return size; }
        }

        public long FileCount
        {
            set { fileCount = value; NotifyChange("FileCount"); }
            get { return fileCount; }
        }

        public string Status
        {
            get { return status; }
            set { status = value; NotifyChange("Status"); }
        }
    }
}
