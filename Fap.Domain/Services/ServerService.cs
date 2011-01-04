#region Copyright Kayomani 2010.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
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
using Fap.Network.Services;
using Autofac;
using System.Net;
using System.Net.Sockets;
using Fap.Domain.Verbs;
using Fap.Network;
using Fap.Network.Entity;
using Fap.Foundation;
using System.Threading;
using Fap.Domain.Entity;

namespace Fap.Domain.Services
{
    public class ServerService
    {
        private FAPListener listener;
        private Model model;

        public ServerService(IContainer c)
        {
            model = c.Resolve<Model>();
            listener = c.Resolve<FAPListener>();
            listener.OnReceiveRequest += new FAPListener.ReceiveRequest(listener_OnReceiveRequest);
        }

        private bool listener_OnReceiveRequest(Request r, Socket s)
        {
            switch (r.Command)
            {
                case "CLIENT":
                    HandleClientInfo(r, s);
                    break;
                case "CHAT":
                    HandleChat(r, s);
                    break;
                default:
                    VerbFactory factory = new VerbFactory();
                    var verb = factory.GetVerb(r.Command, model);
                    s.Send(Mediator.Serialize(verb.ProcessRequest(r)));
                    return false;
            }
           return false;
        }


        private bool IsOverlordKey(string key)
        {
            foreach (var network in model.Networks.ToList())
            {
                if (network.Secret == key)
                    return true;

                var node = model.Peers.Where(p => p.Secret == key).FirstOrDefault();
                if (null != node)
                    return true;
            }
            return false;
        }





        private void HandleChat(Request r, Socket s)
        {
            if (IsOverlordKey(r.RequestID))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(DateTime.Now.ToShortTimeString());
                sb.Append(" ");

                ChatVerb verb = new ChatVerb();
                verb.ProcessRequest(r);
                //No nickname supplied so try and find it
                if (string.IsNullOrEmpty(verb.Nickname))
                {
                    var search = model.Peers.Where(i => i.ID == verb.SourceID).FirstOrDefault();
                    if (search == null || string.IsNullOrEmpty(search.Nickname))
                    {
                        sb.Append(verb.SourceID);
                    }
                    else
                    {
                        sb.Append(search.Nickname);
                    }
                }
                else
                {
                    sb.Append(verb.Nickname);
                }
                sb.Append(": ");
                sb.Append(verb.Message);
                model.Messages.Add(sb.ToString());
            }
        }


        private void HandleClientInfo(Request r, Socket s)
        {
            if (IsOverlordKey(r.RequestID))
            {
                var search = model.Peers.Where(i => i.ID == r.Param).FirstOrDefault();
                if (search == null)
                {
                    search = new Node();
                    foreach (var param in r.AdditionalHeaders)
                        search.SetData(param.Key, param.Value);
                    model.Peers.Add(search);
                }
                else
                {
                    foreach (var param in r.AdditionalHeaders)
                        search.SetData(param.Key, param.Value);
                }
            }
        }


        public void Start()
        {
            var address = GetLocalAddress();
            listener.Start(address, 95);
            model.Node.Host = address.ToString();
            model.Node.Port = 95;
        }

        private IPAddress GetLocalAddress()
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            IPAddress a = localIPs[0];

            foreach (var ip in localIPs)
            {
                if (!IPAddress.IsLoopback(ip) && ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    a = ip;
                    break;
                }
            }
            return a;
        }
    }
}
