using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Domain.Entities;
using System.Runtime.Serialization;
using FAP.Network.Entities;

namespace FAP.Domain.Verbs
{
    public class InfoVerb: BaseVerb, IVerb
    {
        private Node node = new Node();

        [DataMember]
        public Node Node
        {
            set { node = value; }
            get { return node; }
        }


        public Node GetValidatedNode()
        {
            if (null != Node)
            {
                if (!Node.ContainsKey("Nickname"))
                    return null;
            }
            return Node;
        }

        /// <summary>
        /// Called by a end client to send an update a server
        /// </summary>
        /// <returns></returns>
        public NetworkRequest CreateRequest()
        {
            NetworkRequest req = new NetworkRequest();
            req.Data = Serialize<InfoVerb>(this);
            req.Verb = "INFO";
            return req;
        }

        /// <summary>
        /// Called by either the server or an end client to decode incoming data
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public NetworkRequest ProcessRequest(NetworkRequest r)
        {
            InfoVerb inc = Deserialise<InfoVerb>(r.Data);
            Node = inc.Node;
            return null;
        }

        public bool ReceiveResponse(NetworkRequest r)
        {
            try
            {
                InfoVerb inc = Deserialise<InfoVerb>(r.Data);
                Node = inc.Node;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
