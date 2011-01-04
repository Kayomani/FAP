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

namespace Fap.Network.Entity
{
    public class RemoteClient : INotifyPropertyChanged
    {
        private string host;
        private int port;
        private string nickname;
        private string description;
        private long shareSize = 0;
        private string avatarBase64;

        private object sync = new object();
        private long lastAccess = 0;

        public long LastAccess
        {
            set
            {
                lock (sync)
                    lastAccess = value;
            }
            get
            {
                lock (sync)
                    return lastAccess;
            }
        }

        public string Location { get { return Host + ":" + Port; } }
        public string Host
        {
            get
            {
                return host;
            }
            set
            {
                host = value;
                NotifyChange("Host");
                NotifyChange("Location");
            }
        }

        public int Port
        {
            get { return port; }
            set
            {
                port = value;
                NotifyChange("Port");
                NotifyChange("Location");
            }
        }

        public string Nickname
        {
            get { return nickname; }
            set
            {
                nickname = value;
                NotifyChange("Nickname");
            }
        }
        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                NotifyChange("Description");
            }

        }
        public long ShareSize
        {
            get { return shareSize; }
            set
            {
                shareSize = value;
                NotifyChange("ShareSize");
                NotifyChange("ShareSizeString");
            }
        }


        public string ShareSizeString
        {
            get { return Utility.FormatBytes(ShareSize); }
        }

        public string AvatarBase64
        {
            get { return avatarBase64; }
            set
            {
                avatarBase64 = value;
                NotifyChange("AvatarBase64");
                NotifyChange("Avatar");
            }

        }

        public byte[] Avatar
        {
            set
            {
                AvatarBase64 = System.Convert.ToBase64String(value);
            }
            get
            {
                if (string.IsNullOrEmpty(AvatarBase64))
                    return new byte[0];
                return System.Convert.FromBase64String(AvatarBase64);
            }
        }

        public void Dispose()
        {
            avatarBase64 = string.Empty;
        }

        private void NotifyChange(string path)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(path));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
