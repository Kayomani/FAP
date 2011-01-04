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
using System.Net.Sockets;
using Fap.Foundation;

namespace Fap.Network.Entity
{
    public class Session : INotifyPropertyChanged
    {
        private readonly int SessionExpireTime = 15000;//15 Seconds

        private string user;
        private string status;
        private long speed;
        private long length;
        private long transfered;
        private Node host;
        private bool isUpload;
        private int lastUseTicks = 0;

        private bool inUse;
        private object sync = new object();

        public Socket Socket { set; get; }
        // public SocketAsyncEventArgs SocketAsyncEventArgs { set; get; }


        public Session()
        {

        }


        public bool Stale
        {
            get
            {
                lock (sync)
                {
                    int diff = Environment.TickCount - lastUseTicks;
                    return (diff > SessionExpireTime);
                }
            }
        }

        public bool InUse
        {
            set
            {
                lock (sync)
                {
                    inUse = value;
                    lastUseTicks = Environment.TickCount;
                }
                NotifyChange("InUse");
            }
            get
            {
                lock (sync)
                {
                    return inUse;
                }
            }
        }


        public bool IsUpload
        {
            set
            {
                isUpload = value;
                NotifyChange("IsUpload");
            }
            get { return isUpload; }
        }

        public Node Host
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


        public string User
        {
            set
            {
                user = value;
                NotifyChange("User");
            }
            get
            {
                return user;
            }
        }


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

        public long Speed
        {
            set
            {
                speed = value;
                NotifyChange("Speed");
                NotifyChange("SpeedString");
            }
            get
            {
                return speed;
            }
        }

        public string SpeedString
        {
            get { return Utility.FormatBytes(speed) + "/s"; }
        }

        public long Length
        {
            set
            {
                length = value;
                NotifyChange("Length");
                NotifyChange("LengthString");
                NotifyChange("PercentXfer");
            }
            get
            {
                return length;
            }
        }


        public int PercentXfer
        {
            get
            {
                if (Length == 0)
                    return 0;
                int value = (int)(((double)transfered / length) * 100);
                return value;
            }
        }

        public string LengthString
        {
            get { return Utility.FormatBytes(length); }
        }

        public long Transfered
        {
            set
            {
                transfered = value;
                NotifyChange("Transfered");
                NotifyChange("TransferedString");
                NotifyChange("PercentXfer");
            }
            get
            {
                return transfered;
            }
        }

        public string TransferedString
        {
            get { return Utility.FormatBytes(transfered); }
        }

        private void NotifyChange(string path)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(path));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
