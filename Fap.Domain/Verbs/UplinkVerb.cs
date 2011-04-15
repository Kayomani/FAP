using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fap.Network;
using Fap.Network.Entity;

namespace Fap.Domain.Verbs
{
    public class UplinkVerb : VerbBase, IVerb
    {
        Node model;

        public UplinkVerb(Node m)
        {
            model = m;
        }

        public Network.Entity.Request CreateRequest()
        {
            Request response = new Request();
            response.RequestID = model.ID;
            response.Command = "UPLINK";
            return response;
        }

        public Response ProcessRequest(Request r)
        {
            Response response = new Response();
            response.RequestID = r.RequestID;
            return response;
        }

        public bool ReceiveResponse(Network.Entity.Response r)
        {
            Status = r.Status;
            return true;
        }
    }
}
