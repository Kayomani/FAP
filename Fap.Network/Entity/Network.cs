using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Fap.Network.Entity
{
    public class Network : INotifyPropertyChanged
    {
        private string networkID;
        private string overlord;
        private string name;

        private string secret;
        private Fap.Network.ConnectionState state;


        public Fap.Network.ConnectionState State
        {
            set { state = value; NotifyChange("State"); }
            get { return state; }
        }

        public string Secret
        {
            set { secret = value; NotifyChange("Secret"); }
            get { return secret; }
        }

        public string ID
        {
            set { networkID = value; NotifyChange("ID"); }
            get { return networkID; }
        }

        public string OverlordID
        {
            set { overlord = value; NotifyChange("OverlordID"); }
            get { return overlord; }
        }

        public string Name
        {
            set { name = value; NotifyChange("Name"); }
            get { return name; }
        }

        private void NotifyChange(string path)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(path));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
