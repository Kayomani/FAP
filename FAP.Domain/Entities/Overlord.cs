using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fap.Foundation;

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
