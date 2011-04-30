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
using FAP.Network.Services;
using Autofac;
using FAP.Domain.Entities;
using FAP.Domain.Verbs;
using FAP.Domain;
using System.Threading;
using FAP.Domain.Verbs.Multicast;
using NLog;
using FAP.Domain.Net;
using Fap.Foundation;
using FAP.Network.Entities;

namespace FAP.Application.Controllers
{
    public class ConnectionController
    {
        private MulticastClientService mclient;
        private MulticastServerService mserver;
        private Model model;

        private AutoResetEvent workerEvent = new AutoResetEvent(true);
        private List<DetectedNode> announcedAddresses = new List<DetectedNode>();
        private object sync = new object();
        private bool run = true;

        public ConnectionController(IContainer c)
        {
            mclient = c.Resolve<MulticastClientService>();
            model = c.Resolve<Model>();
            mserver = c.Resolve<MulticastServerService>();
            mclient.OnMultiCastRX += new MulticastClientService.MultiCastRX(mclient_OnMultiCastRX);
            mclient.StartListener();
            setupLocalNetwork();
        }

        private void setupLocalNetwork()
        {
           // Domain.Entities.Network network = new Domain.Entities.Network();
            model.Network.NetworkName = "Local";
            model.Network.NetworkID = "Local";
           // model.Networks.Add(network);
        }

        private void mclient_OnMultiCastRX(string cmd)
        {
            if (cmd.StartsWith(HelloVerb.Preamble))
            {
                HelloVerb verb = new HelloVerb();
                var node = verb.ParseRequest(cmd);
                if (null != node)
                {
                    lock (sync)
                    {
                        var search = announcedAddresses.Where(s => s.Address == node.Address).FirstOrDefault();
                        if (null == search)
                        {
                            announcedAddresses.Add(node);
                        }
                        else
                        {
                            search.ID = node.ID;
                            search.NetworkName = node.NetworkName;
                        }
                    }
                }
                workerEvent.Set();
            }
        }

        public void SendMessage(string message)
        {
            ChatVerb verb = new ChatVerb();
            verb.Message = message;
            verb.Nickname = model.LocalNode.Nickname;
            verb.SourceID = model.LocalNode.ID;
            ThreadPool.QueueUserWorkItem(new  WaitCallback(SendMessageAsync),verb.CreateRequest());
        }


        private void SendMessageAsync(object o)
        {
            try
            {
                if (model.Network.State == ConnectionState.Connected)
                {
                    Client client = new Client(model.LocalNode);
                    client.Execute((NetworkRequest)o, model.Network.Overlord);
                }
                else
                {
                    LogManager.GetLogger("faplog").Warn("Could not send message as you are not conencted");
                }
            }
            catch(Exception e)
            {
                LogManager.GetLogger("faplog").ErrorException("Failed to send chat message",e);
            }
        }

        public void Start()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessLanConnection));
        }


        public void Exit()
        {
            run = false;
            workerEvent.Set();

            //Notify log off
            if (model.Network.State == ConnectionState.Connected)
            {
                Client c = new Client(model.LocalNode);

            }
        }

        private void ProcessLanConnection(object o)
        {
            mserver.AddMessage(WhoVerb.CreateRequest());
            Domain.Entities.Network network = model.Network;
            network.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(network_PropertyChanged);
            while (run)
            {
                if (network.State != ConnectionState.Connected)
                {
                    //Not connected - get a detected overlord
                    DetectedNode node;
                    lock (sync)
                    {
                        node = announcedAddresses.FirstOrDefault();
                        if (null != node)
                        {
                            announcedAddresses.Remove(node);
                            Connect(network,node);
                        }
                    }
                }
                workerEvent.WaitOne(10000);
            }
        }

        private void network_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //When the network state changes then reconnect if needed.
            if (e.PropertyName == "State")
                workerEvent.Set();
        }

        private void Connect(Domain.Entities.Network net, DetectedNode n)
        {
            try
            {
                LogManager.GetLogger("faplog").Info("Client connecting to {0}", n.Address);
                net.State = ConnectionState.Connecting;
                ConnectVerb verb = new ConnectVerb();
                verb.Address = model.IPAddress + ":" + model.ClientPort;
                Client client = new Client(model.LocalNode);
                if (client.Execute(verb, n.Address))
                {
                    net.State = ConnectionState.Connected;
                    net.Overlord = new Node();
                    net.Overlord.Location = n.Address;
                    LogManager.GetLogger("faplog").Info("Client connected");
                }
            }
            catch
            {
                net.State = ConnectionState.Disconnected;
            }
        }
    }
}
