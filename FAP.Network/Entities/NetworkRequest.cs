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

namespace FAP.Network.Entities
{
    public class NetworkRequest
    {
        public string Verb { set; get; }
        public string Data { set; get; }
        public string Param { set; get; }

        /// <summary>
        /// ID node generating the message
        /// </summary>
        public string SourceID { set; get; }
        /// <summary>
        /// Overlord ID which the client is connected to
        /// </summary>
        public string OverlordID { set; get; }
        /// <summary>
        /// Secret auth key for client->server and server->server security.
        /// </summary>
        public string AuthKey { set; get; }

        public NetworkRequest Clone()
        {
            NetworkRequest r = new NetworkRequest();
            r.Verb = Verb;
            r.Data = Data;
            r.Param = Param;
            r.SourceID = SourceID;
            r.OverlordID = OverlordID;
            r.AuthKey = AuthKey;
            return r;
        }
    }
}
