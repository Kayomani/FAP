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
using Fap.Foundation;
using Newtonsoft.Json;

namespace FAP.Domain.Entities
{
    public class Overlord : Node
    {
        private int _maxPeers;
        private BackgroundSafeObservable<Node> _peers = new BackgroundSafeObservable<Node>();
        private int _strength;

        public Overlord()
        {
            NodeType = ClientType.Overlord;
        }

        [JsonIgnore]
        public int Strength
        {
            set
            {
                _strength = value;
                NotifyChange("Strength");
            }
            get { return _strength; }
        }

        [JsonIgnoreAttribute]
        public int MaxPeers
        {
            set
            {
                _maxPeers = value;
                NotifyChange("MaxPeers");
            }
            get { return _maxPeers; }
        }

        [JsonIgnoreAttribute]
        public BackgroundSafeObservable<Node> Peers
        {
            set
            {
                _peers = value;
                NotifyChange("Peers");
            }
            get { return _peers; }
        }

        public void GenerateStrength(OverlordPriority priority)
        {
            var r = new Random(Environment.TickCount);

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