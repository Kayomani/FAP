using FAP.Network.Entities;
using Newtonsoft.Json;

namespace FAP.Domain.Verbs
{
    public interface IConversationController
    {
        bool HandleMessage(string id, string nickname, string message);
    }

    public class ConversationVerb : BaseVerb, IVerb
    {
        public string Nickname { set; get; }
        public string Message { set; get; }

        [JsonIgnore]
        public string SourceID { set; get; }

        #region IVerb Members

        public NetworkRequest CreateRequest()
        {
            var req = new NetworkRequest();
            req.Verb = "CONVERSTATION";
            req.Data = Serialize(this);
            return req;
        }

        public NetworkRequest ProcessRequest(NetworkRequest r)
        {
            var verb = Deserialise<ConversationVerb>(r.Data);
            Nickname = verb.Nickname;
            Message = verb.Message;
            SourceID = r.SourceID;
            r.Data = string.Empty;
            return r;
        }

        public bool ReceiveResponse(NetworkRequest r)
        {
            return true;
        }

        #endregion
    }
}