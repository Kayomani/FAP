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
using FAP.Domain.Entities;
using System.Runtime.Serialization;
using FAP.Network.Entities;

namespace FAP.Domain.Verbs
{
    public class UpdateVerb : BaseVerb, IVerb
    {
        private List<Node> nodes = new List<Node>();

        [DataMember]
        public List<Node> Nodes
        {
            set { nodes = value; }
            get { return nodes; }
        }

        

        /// <summary>
        /// Called by a end client to send an update a server
        /// </summary>
        /// <returns></returns>
        public NetworkRequest CreateRequest()
        {
            NetworkRequest req = new NetworkRequest();
            req.Data = Serialize<UpdateVerb>(this);
            req.Verb = "UPDATE";
            return req;
        }

        /// <summary>
        /// Called by either the server or an end client to decode incoming data
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public NetworkRequest ProcessRequest(NetworkRequest r)
        {
            UpdateVerb inc = Deserialise<UpdateVerb>(r.Data);
            nodes = inc.Nodes;
            return null;
        }

        public bool ReceiveResponse(NetworkRequest r)
        {
            return true;
        }
    }
}
