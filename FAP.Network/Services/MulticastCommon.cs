using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fap.Foundation;
using System.Net;

namespace FAP.Network.Services
{
    public class MulticastCommon 
    {
        protected readonly IPAddress broadcastAddress = IPAddress.Parse("239.1.1.1");
        protected readonly int broadcastPort = 12;
    }
}
