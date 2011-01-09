using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fap.Network;
using Fap.Network.Entity;

namespace Fap.Domain.Verbs
{
    public class DisconnectVerb : VerbBase, IVerb
    {

        public Request CreateRequest()
        {
            Request r = new Request();
            r.Command = "DISCONENCT";
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
