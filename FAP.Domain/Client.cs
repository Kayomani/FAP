using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Domain.Entities;
using System.Net;
using FAP.Domain.Verbs;
using FAP.Network;
using FAP.Network.Entities;
using System.IO;

namespace FAP.Domain
{
    public class Client
    {
        private Node callingNode;


        public Client(Node _callingNode)
        {
            callingNode = _callingNode;
        }

        public bool Execute(IVerb verb, Node destinationNode)
        {
            return Execute(verb, destinationNode.Host);
        }

        public bool Execute(IVerb verb, string destination)
        {
            return Execute(verb, destination, string.Empty);
        }

        public bool Execute(IVerb verb, string destination, string authKey)
        {
            try
            {
                NetworkRequest request = verb.CreateRequest();
                string data = string.Empty;

                if (!DoRequest(destination, request.Verb, request.Param, request.Data,authKey, out data))
                    return false;
                if (!verb.ReceiveResponse(new NetworkRequest() { Data = data }))
                    return false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Execute(NetworkRequest req, Node destination)
        {
            string output = string.Empty;
            return DoRequest(destination.Location, req.Verb, req.Param, req.Data, destination.Secret, out output);
        }

        public bool DoRequest(string url, string method, string param, string data, string authKey, out string result)
        {
            result = string.Empty;
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(Multiplexor.Encode(url, method, param));
                 req.Timeout= 30000;
                 //req.Pipelined = false;

                 //req.ConnectionGroupName = Guid.NewGuid().ToString();
                //Add headers
                req.UserAgent = Model.AppVersion;
                //Add fap headers
                if (!string.IsNullOrEmpty(authKey))
                    req.Headers.Add("FAP-AUTH", authKey);
                if(null!=callingNode)
                req.Headers.Add("FAP-SOURCE", callingNode.ID);
                //If we need to send data then do a post
                if (string.IsNullOrEmpty(data))
                {
                    req.Method = "GET";
                    req.ContentLength = 0;
                }
                else
                {
                    req.ContentType = "application/json";
                    req.Method = "POST";
                    byte[] bytes = System.Text.Encoding.ASCII.GetBytes(data);
                    req.ContentLength = bytes.Length;
                    System.IO.Stream os = req.GetRequestStream();
                    os.Write(bytes, 0, bytes.Length); //Push it out there
                    os.Flush();
                }

                
                System.Net.HttpWebResponse resp = (System.Net.HttpWebResponse)req.GetResponse();

                req.Timeout = 100000;
                if (resp == null)
                    return false;
                //If data was returned then get it from the stream
                if (resp.ContentLength > 0)
                {
                    using (Stream s = resp.GetResponseStream())
                    {
                        using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
                        {
                            result = sr.ReadToEnd().Trim();
                        }
                    }
                }
               
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
