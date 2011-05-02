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

namespace FAP.Domain
{
    public enum OverlordPriority {Low,Normal, High};
    public enum ClientType { Client = 0, Server = 1, Overlord = 2, Unknown = 3 };
    public enum ConnectionState { Disconnected, Connecting, Connected };
    public enum DownloadRequestState { None = 0, Requesting = 1, Queued = 2, Downloading = 3, Downloaded = 4, Error =5 };
    public enum PeerSortType { Size = 0, Name = 1, Type = 2, Address = 3 };
    public enum NetSpeedType { Download, Upload };
}
