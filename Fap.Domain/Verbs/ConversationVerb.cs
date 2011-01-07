using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fap.Network;
using Fap.Network.Entity;
using Fap.Domain.Entity;

namespace Fap.Domain.Verbs
{
    public class ConversationVerb: VerbBase, IVerb
    {
        private Model model;

        public ConversationVerb(Model m)
        {
            model = m;
        }

        public Request CreateRequest()
        {
            Request r = new Request();
            r.Command = "CONVERSATION";
            r.Param = SourceID;
            r.AdditionalHeaders.Add("Nickname", Nickname);
            r.AdditionalHeaders.Add("Message", Message);
            return r;
        }

        public Network.Entity.Response ProcessRequest(Network.Entity.Request r)
        {
            Response response = new Response();
            response.RequestID = r.RequestID;
            response.Status = 0;

            Nickname = GetValueSafe(r.AdditionalHeaders, "Nickname");
            Message = GetValueSafe(r.AdditionalHeaders, "Message");
            SourceID = r.Param;

            var peer = model.Peers.Where(p => p.ID == SourceID).FirstOrDefault();
            if (null == peer || !model.ReceiveConverstation(SourceID, Message))
              response.Status = 1;
            return response;
        }

        public bool ReceiveResponse(Network.Entity.Response r)
        {
            return r!=null && r.Status == 0;
        }

        public string Nickname { set; get; }
        public string Message { set; get; }
        public string SourceID { set; get; }
    }
}
