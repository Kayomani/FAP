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
