#region Copyright Kayomani 2011.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
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
using Fap.Foundation;
using System.Collections.ObjectModel;

namespace FAP.Domain.Entities
{
    public class Network: BaseEntity
    {
        private SafeObservedCollection<Node> nodes = new SafeObservedCollection<Node>();
        private string networkName;
        private string networkID;
        private ConnectionState state = ConnectionState.Disconnected;
        private Node overlord;

        public ConnectionState State
        {
            set
            {
                state = value;
                NotifyChange("State");
            }
            get { return state; }
        }

        public SafeObservedCollection<Node> Nodes
        {
            get { return nodes; }
        }

        public string NetworkName
        {
            set { networkName = value; NotifyChange("NetworkName"); }
            get { return networkName; }
        }

        public string NetworkID
        {
            set { networkID = value; NotifyChange("NetworkID"); }
            get { return networkID; }
        }

        public Node Overlord
        {
            set { overlord = value; NotifyChange("Overlord"); }
            get { return overlord; }
        }
    }
}
