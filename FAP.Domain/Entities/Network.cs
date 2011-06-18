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

using Fap.Foundation;

namespace FAP.Domain.Entities
{
    public class Network : BaseEntity
    {
        private readonly SafeObservedCollection<Node> nodes = new SafeObservedCollection<Node>();
        private string networkID;
        private string networkName;
        private Node overlord;
        private ConnectionState state = ConnectionState.Disconnected;

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
            set
            {
                networkName = value;
                NotifyChange("NetworkName");
            }
            get { return networkName; }
        }

        public string NetworkID
        {
            set
            {
                networkID = value;
                NotifyChange("NetworkID");
            }
            get { return networkID; }
        }

        public Node Overlord
        {
            set
            {
                overlord = value;
                NotifyChange("Overlord");
            }
            get { return overlord; }
        }
    }
}