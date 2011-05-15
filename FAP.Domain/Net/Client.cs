#region Copyright Kayomani 2011.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
/**
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or any 
    later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * */
#endregion
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

namespace FAP.Domain.Net
{
    public class Client
    {
        private Node callingNode;

        private readonly int DEFAULT_TIMEOUT = 30000;//30 seconds

        public Client(Node _callingNode)
        {
            callingNode = _callingNode;
        }

        public bool Execute(IVerb verb, Node destinationNode)
        {
            return Execute(verb, destinationNode, DEFAULT_TIMEOUT);
        }

        public bool Execute(IVerb verb, string destination)
        {
            return Execute(verb, destination, string.Empty, DEFAULT_TIMEOUT);
        }

        public bool Execute(IVerb verb, string destination, int timeout)
        {
            return Execute(verb, destination, string.Empty, timeout);
        }

        public bool Execute(NetworkRequest req, Node destination)
        {
            return Execute(req, destination, 30000);
        }

        public bool Execute(NetworkRequest req, Node destination, int timeout)
        {
            destination.LastUpdate = Environment.TickCount;
            NetworkRequest output = new NetworkRequest();
            if (!string.IsNullOrEmpty(destination.Secret) && string.IsNullOrEmpty(req.AuthKey))
                req.AuthKey = destination.Secret;
            return DoRequest(destination.Location, req, out output, timeout);
        }

        public bool Execute(IVerb verb, string destination, string authKey, int timeout)
        {
            return Execute(verb, new Node() { Location = destination, Secret = authKey }, timeout);
        }

        public bool Execute(IVerb verb, Node destination, int timeout)
        {
            try
            {
                NetworkRequest request = verb.CreateRequest();
                NetworkRequest output = new NetworkRequest();
                destination.LastUpdate = Environment.TickCount;

                if (!string.IsNullOrEmpty(destination.Secret) && string.IsNullOrEmpty(request.AuthKey))
                    request.AuthKey = destination.Secret;

                if (!DoRequest(destination.Location, request, out output, timeout))
                    return false;

                if (!verb.ReceiveResponse(output))
                    return false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DoRequest(string url, NetworkRequest input, out NetworkRequest result, int timeout)
        {
            result = new NetworkRequest();

            if (callingNode != null)
                input.SourceID = callingNode.ID;

            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(Multiplexor.Encode(url, input.Verb, input.Param));
                req.Timeout = timeout;

                //Add headers
                req.UserAgent = Model.AppVersion;
                //Add fap headers
                if (!string.IsNullOrEmpty(input.AuthKey))
                    req.Headers.Add("FAP-AUTH", input.AuthKey);
                if (!string.IsNullOrEmpty(input.SourceID))
                    req.Headers.Add("FAP-SOURCE", input.SourceID);
                if (!string.IsNullOrEmpty(input.OverlordID))
                    req.Headers.Add("FAP-OVERLORD", input.OverlordID);

                //If we need to send data then do a post
                if (string.IsNullOrEmpty(input.Data))
                {
                    req.Method = "GET";
                    req.ContentLength = 0;
                }
                else
                {
                    req.ContentType = "application/json";
                    req.Method = "POST";
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input.Data);
                    req.ContentLength = bytes.Length;
                    System.IO.Stream os = req.GetRequestStream();
                    os.Write(bytes, 0, bytes.Length); //Push it out there
                    os.Flush();
                }

                //Get the response
                System.Net.HttpWebResponse resp = (System.Net.HttpWebResponse)req.GetResponse();
                req.Timeout = 100000;
                if (resp == null)
                    return false;
                //If data was returned then get it from the stream
                if (resp.ContentLength > 0)
                {
                    using (Stream s = resp.GetResponseStream())
                    {
                        using (System.IO.StreamReader sr = new System.IO.StreamReader(s, Encoding.UTF8))
                        {
                            result.Data = sr.ReadToEnd().Trim();
                        }
                    }
                }

                //Get the headers
                foreach (var header in resp.Headers.AllKeys)
                {
                    switch (header)
                    {
                        case "FAP-AUTH":
                            result.AuthKey = resp.Headers[header];
                            break;
                        case "FAP-SOURCE":
                            result.SourceID = resp.Headers[header];
                            break;
                        case "FAP-OVERLORD":
                            result.OverlordID = resp.Headers[header];
                            break;
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
