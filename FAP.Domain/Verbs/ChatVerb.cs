using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Network.Entities;

namespace FAP.Domain.Verbs
{
    public class ChatVerb : BaseVerb, IVerb
    {
        public Network.Entities.NetworkRequest CreateRequest()
        {
            NetworkRequest req = new NetworkRequest();
            req.Data = Serialize<ChatVerb>(this);
            req.Verb = "CHAT";
            return req;
        }

        public Network.Entities.NetworkRequest ProcessRequest(NetworkRequest r)
        {
            ReceiveResponse(r);
            return CreateRequest();
        }

        public bool ReceiveResponse(Network.Entities.NetworkRequest r)
        {
            ChatVerb inc = Deserialise<ChatVerb>(r.Data);
            Nickname = inc.Nickname;
            Message = inc.Message;
            SourceID = inc.SourceID;
            return true;
        }

        public string Nickname { set; get; }
        public string Message { set; get; }
        public string SourceID { set; get; }
    }
}
