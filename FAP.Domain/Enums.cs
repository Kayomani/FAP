using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAP.Domain
{
    public enum OverlordPriority {Low,Normal, High};
    public enum ClientType { Client = 0, Server = 1, Overlord = 2, Unknown = 3 };
    public enum ConnectionState { Disconnected, Connecting, Connected };
    public enum DownloadRequestState { None = 0, Requesting = 1, Queued = 2, Downloading = 3, Downloaded = 4 }
    public enum PeerSortType { Size = 0, Name = 1, Type = 2, Address = 3 };
}
