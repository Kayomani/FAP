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
using Newtonsoft.Json;

namespace FAP.Domain.Entities
{
    public class Overlord : Node
    {
        private BackgroundSafeObservable<Node> peers = new BackgroundSafeObservable<Node>();
        private int strength;
        private int maxPeers;

        public Overlord()
            : base()
        {
            NodeType = ClientType.Overlord;
        }

        [JsonIgnoreAttribute]
        public int Strength
        {
            set
            {
                strength = value;
                NotifyChange("Strength");
            }
            get { return strength; }
        }

        [JsonIgnoreAttribute]
        public int MaxPeers
        {
            set
            {
                maxPeers = value;
                NotifyChange("MaxPeers");
            }
            get { return maxPeers; }
        }

        [JsonIgnoreAttribute]
        public BackgroundSafeObservable<Node> Peers
        {
            set
            {
                peers = value;
                NotifyChange("Peers");
            }
            get { return peers; }
        }

        public void GenerateStrength(OverlordPriority priority)
        {
            Random r = new Random(Environment.TickCount);

            switch (priority)
            {
                case OverlordPriority.High:
                    Strength = r.Next(666, 1000);
                    MaxPeers = 500;
                    break;
                case OverlordPriority.Low:
                    Strength = r.Next(0, 333);
                    MaxPeers = 25;
                    break;
                case OverlordPriority.Normal:
                    Strength = r.Next(333, 666);
                    MaxPeers = 100;
                    break;
            }
        }
    }
}
