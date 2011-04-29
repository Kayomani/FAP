using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Domain.Entities;
using System.Runtime.Serialization;
using FAP.Network.Entities;

namespace FAP.Domain.Verbs
{
    public class UpdateVerb : BaseVerb, IVerb
    {
        private List<Node> nodes = new List<Node>();

        [DataMember]
        public List<Node> Nodes
        {
            set { nodes = value; }
            get { return nodes; }
        }

        

        /// <summary>
        /// Called by a end client to send an update a server
        /// </summary>
        /// <returns></returns>
        public NetworkRequest CreateRequest()
        {
            NetworkRequest req = new NetworkRequest();
            req.Data = Serialize<UpdateVerb>(this);
            req.Verb = "UPDATE";
            return req;
        }

        /// <summary>
        /// Called by either the server or an end client to decode incoming data
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public NetworkRequest ProcessRequest(NetworkRequest r)
        {
            UpdateVerb inc = Deserialise<UpdateVerb>(r.Data);
            nodes = inc.Nodes;
            return null;
        }

        public bool ReceiveResponse(NetworkRequest r)
        {
            try
            {
                UpdateVerb inc = Deserialise<UpdateVerb>(r.Data);
                nodes = inc.Nodes;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
