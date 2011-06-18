using System;

namespace FAP.Domain.Net
{
    public class LanPeer
    {
        public DetectedNode Node { set; get; }
        public DateTime LastConnectionTime { set; get; }
        public DateTime LastUpdate { set; get; }
    }
}