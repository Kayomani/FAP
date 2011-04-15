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
using Fap.Foundation;

namespace Fap.Network.Entity
{
    public class Overlord: Node
    {
        private SafeObservable<Uplink> peers = new SafeObservable<Uplink>();
        private int strength;
        private int maxPeers;

        public Overlord()
            : base()
        {
            NodeType = ClientType.Overlord;
        }

        public int Strength
        {
            set
            {
                strength = value;
                NotifyChange("Strength");
            }
            get { return strength; }
        }

        public int MaxPeers
        {
            set
            {
                maxPeers = value;
                NotifyChange("MaxPeers");
            }
            get { return maxPeers; }
        }

        public SafeObservable<Uplink> Peers
        {
            set
            {
                peers = value;
                NotifyChange("Peers");
            }
            get { return peers; }
        }
    }
}
