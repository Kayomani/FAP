using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fap.Network;
using Fap.Network.Entity;

namespace Fap.Domain.Verbs
{
    /// <summary>
    /// This is used to tell other clients you have disconnected and is transmitted via a overlord.
    /// </summary>
    public class DisconnectVerb : VerbBase, IVerb
    {
        private Node node;

        public DisconnectVerb(Node n)
        {
            node = n;
        }

        public Request CreateRequest()
        {
            Request r = new Request();
            r.Command = "DISCONNECT";
            r.Param = node.ID;
            return r;
        }

        public Network.Entity.Response ProcessRequest(Network.Entity.Request r)
        {
            return null;
        }

        public bool ReceiveResponse(Network.Entity.Response r)
        {
            throw new NotImplementedException();
        }
    }
}
