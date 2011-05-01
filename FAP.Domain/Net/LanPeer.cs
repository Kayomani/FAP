using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAP.Domain.Net
{
    public class LanPeer
    {
        public DetectedNode Node { set; get; }
        public DateTime LastConnectionTime { set; get; }
        public DateTime LastUpdate { set; get; }
    }
}
