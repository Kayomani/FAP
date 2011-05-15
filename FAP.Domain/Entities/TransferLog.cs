using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAP.Domain.Entities
{
    public class TransferLog : BaseEntity
    {
        private string nickname;
        private string filename;
        private string path;
        private long size;
        private int speed;
        private DateTime completed;
        private DateTime added;

        public string Nickname
        {
            set { nickname = value; NotifyChange("Nickname"); }
            get { return nickname; }
        }

        public string Filename
        {
            set { filename = value; NotifyChange("Filename"); }
            get { return filename; }
        }

        public string Path
        {
            set { path = value; NotifyChange("Path"); }
            get { return path; }
        }

        public long Size
        {
            set { size = value; NotifyChange("Size"); }
            get { return size; }
        }

        public int Speed
        {
            set { speed = value; NotifyChange("Speed"); }
            get { return speed; }
        }

        public DateTime Completed
        {
            set { completed = value; NotifyChange("Completed"); }
            get { return completed; }
        }

        public DateTime Added
        {
            set { added = value; NotifyChange("Added"); }
            get { return added; }
        }
    }
}
