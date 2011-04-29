using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Domain.Entities;
using FAP.Network.Entities;

namespace FAP.Domain.Verbs
{
    public class ConnectVerb : BaseVerb, IVerb
    {
        public NetworkRequest CreateRequest()
        {
            NetworkRequest r = new NetworkRequest();
            r.Verb = "CONNECT";
            r.Param = "127.0.0.1:30";
            return r;
        }

        public NetworkRequest ProcessRequest(NetworkRequest r)
        {
            throw new NotImplementedException();
        }

        public bool ReceiveResponse(NetworkRequest r)
        {
            return true;
        }
    }
}
