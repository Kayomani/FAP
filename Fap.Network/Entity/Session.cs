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

        
        private bool isUpload;
        private int lastUseTicks = 0;
        private bool inUse;
        private Node host;
        private object sync = new object();

        public Socket Socket { set; get; }

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

        private void NotifyChange(string path)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(path));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
