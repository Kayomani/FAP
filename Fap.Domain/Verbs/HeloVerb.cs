#region Copyright Kayomani 2010.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
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
using Fap.Network;
using Fap.Network.Entity;

namespace Fap.Domain.Verbs
{
    public class HeloVerb : IVerb
    {
        private Node node;


        public HeloVerb(Node n)
        {
            node = n;
        }

        public Network.Entity.Request CreateRequest()
        {
            Request r = new Request();
            r.Command = "HELO";
            if (string.IsNullOrEmpty(ListenLocation) || -1 == ListenLocation.IndexOf(':'))
                throw new Exception("Tried to create helo without a listen address set");
            r.Param = ListenLocation;
            if (string.IsNullOrEmpty(node.ID))
                throw new Exception("Helo create failed, no local node ID");
            r.AdditionalHeaders.Add("ID", node.ID);
            r.AdditionalHeaders.Add("Clients", Clients.ToString());
            r.AdditionalHeaders.Add("Index", Index.ToString());
            return r;
        }

        public Network.Entity.Response ProcessRequest(Network.Entity.Request r)
        {
            ListenLocation = r.Param;
            ID = r.AdditionalHeaders["ID"];
            Clients = int.Parse(r.AdditionalHeaders["Clients"]);
            Index = int.Parse(r.AdditionalHeaders["Index"]);
            return null;
        }

        public bool ReceiveResponse(Network.Entity.Response r)
        {
            return false;
        }


        public string ListenLocation { set; get; }
        public string ID { set; get; }
        public int Clients { set; get; }
        public int Index { set; get; }
    }
}
