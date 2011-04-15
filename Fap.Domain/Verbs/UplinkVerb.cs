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
            InfoVerb i = new InfoVerb(model);
            return i.ProcessRequest(r);
        }

        public bool ReceiveResponse(Network.Entity.Response r)
        {
            try
            {
                if (r.Status == 0 && r.ContentSize == 0)
                {
                    //If we don't have the full specification then fail the response.
                    foreach (var data in r.AdditionalHeaders)
                        model.SetData(data.Key, data.Value);
                    return true;
                }
            }
            catch { }
            return false;
        }

        public Node Model
        {
            set { model = value; }
            get { return model; }
        }
    }
}
