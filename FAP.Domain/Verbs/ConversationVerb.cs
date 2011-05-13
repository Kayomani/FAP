using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Network.Entities;
using FAP.Domain.Entities;
using Newtonsoft.Json;

namespace FAP.Domain.Verbs
{
    public interface IConversationController
    {
        bool HandleMessage(string id, string nickname, string message);
    }
    public class ConversationVerb : BaseVerb, IVerb
    {
        public NetworkRequest CreateRequest()
        {
            NetworkRequest req = new NetworkRequest();
            req.Verb = "CONVERSTATION";
            req.Data = Serialize<ConversationVerb>(this);
            return req;
        }

        public Network.Entities.NetworkRequest ProcessRequest(Network.Entities.NetworkRequest r)
        {
            ConversationVerb verb = Deserialise<ConversationVerb>(r.Data);
            Nickname = verb.Nickname;
            Message = verb.Message;
            SourceID = r.SourceID;
            r.Data = string.Empty;
            return r;
        }

        public bool ReceiveResponse(Network.Entities.NetworkRequest r)
        {
            return true;
        }

        public string Nickname { set; get; }
        public string Message { set; get; }
        [JsonIgnore]
        public string SourceID { set; get; }
    }
}
