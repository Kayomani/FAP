using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAP.Domain.Network
{
    public class DetectedNode
    {
        public string Address { set; get; }
        public string NetworkName { set; get; }
        public string ID { set; get; }
        public int Priority { set; get; }
    }
}
