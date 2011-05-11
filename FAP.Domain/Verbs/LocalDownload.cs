using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Network.Entities;

namespace FAP.Domain.Verbs
{
    public class AddDownload : BaseVerb, IVerb
    {
        public NetworkRequest CreateRequest()
        {
            NetworkRequest req = new NetworkRequest();
            req.Verb = "ADDDOWNLOAD";
            req.Param = URL;
            return req;
        }

        public NetworkRequest ProcessRequest(NetworkRequest r)
        {
            URL = r.Param;
            return new NetworkRequest();
        }

        public bool ReceiveResponse(NetworkRequest r)
        {
            return true;
        }

        public string URL { set; get; }
    }
}
