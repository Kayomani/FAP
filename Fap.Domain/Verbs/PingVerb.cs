using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fap.Network;
using Fap.Network.Entity;

namespace Fap.Domain.Verbs
{
    public class PingVerb : VerbBase, IVerb
    {
        private long startTime = 0;
        private Node node;

        public PingVerb(Node n)
        {
            node = n;
        }


        public Request CreateRequest()
        {
            Request r = new Request();
            r.Command = "PING";
            r.Param = node.ID;
            startTime = Environment.TickCount;
            return r;
        }

        public Network.Entity.Response ProcessRequest(Network.Entity.Request r)
        {
            Response response = new Response();
            response.RequestID = r.RequestID;
            response.Status = Status;
            return response;
        }

        public bool ReceiveResponse(Network.Entity.Response r)
        {
            Time = Environment.TickCount - startTime;
            Status = r.Status;
            return true;
        }

        public long Time { set; get; }
        public int Status { set; get; }
    }
}
