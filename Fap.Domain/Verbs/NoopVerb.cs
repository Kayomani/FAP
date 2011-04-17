using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fap.Network.Entity;
using Fap.Network;

namespace Fap.Domain.Verbs
{
    /// <summary>
    /// No operation - Used to stop connections timing out.
    /// </summary>
    public class NoopVerb : VerbBase, IVerb
    {
        public Network.Entity.Request CreateRequest()
        {
            Network.Entity.Request r = new Request();
            r.Command = "NOOP";
            return r;
        }

        public Network.Entity.Response ProcessRequest(Network.Entity.Request r)
        {
            Network.Entity.Response res = new Response();
            res.RequestID = r.RequestID;
            return res;
        }

        public bool ReceiveResponse(Network.Entity.Response r)
        {
            return true;
        }
    }
}
