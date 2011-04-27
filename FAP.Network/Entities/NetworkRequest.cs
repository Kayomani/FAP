using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAP.Network.Entities
{
    public class NetworkRequest
    {
        public string Verb { set; get; }
        public string Data { set; get; }
        public string Param { set; get; }
    }
}
