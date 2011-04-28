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

        public ConnectionState State
        {
            set { state = value; NotifyChange("State"); }
            get { return state; }
        }

        public SafeObservedCollection<Node> Nodes
        {
            set { nodes = value; NotifyChange("Nodes"); }
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
    }
}
