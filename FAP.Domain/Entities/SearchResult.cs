using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace FAP.Domain.Entities
{
    public class SearchResult: BaseEntity
    {
        private string fileName;
        private long size;
        private string user;
        private DateTime modified;
        private string path;
        private bool isFolder;
        private string clientID;

        public bool IsFolder
        {
            get { return isFolder; }
            set { isFolder = value; }
        }

        public string ClientID
        {
            get { return clientID; }
            set { clientID = value; }
        }

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; NotifyChange("FileName"); }
        }

        public long Size
        {
            get { return size; }
            set { size = value; NotifyChange("Size"); }
        }
        
        [JsonIgnore]
        public string User
        {
            get { return user; }
            set { user = value; NotifyChange("User"); }
        }

        public DateTime Modified
        {
            get { return modified; }
            set { modified = value; NotifyChange("Modified"); }
        }
       
        public string Path
        {
            get { return path; }
            set { path = value; NotifyChange("Path"); }
        }
    }
}
