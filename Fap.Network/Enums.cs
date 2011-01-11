using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fap.Network
{
    public enum ConnectionState { Disconnected, Connecting, Connected };
    public class OverlordLimits
    {
        public const int HIGH_PRIORITY = 100;
        public const int NORMAL_PRIORITY = 50;
        public const int LOW_PRIORITY = 25;
    }
}
