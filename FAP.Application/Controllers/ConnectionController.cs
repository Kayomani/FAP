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
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Autofac;
using FAP.Domain;
using FAP.Domain.Entities;
using FAP.Domain.Net;
using FAP.Domain.Verbs;
using FAP.Domain.Verbs.Multicast;
using Fap.Foundation;
using Fap.Foundation.Services;
using FAP.Network.Entities;
using FAP.Network.Services;
using NLog;
using IContainer = Autofac.IContainer;

namespace FAP.Application.Controllers
{
    /// <summary>
    /// Handles automatically connecting the client to a server on the lan
    /// </summary>
    public class ConnectionController
    {
        private static readonly object sync = new object();
        private readonly BackgroundSafeObservable<LanPeer> attemptedPeers = new BackgroundSafeObservable<LanPeer>();
        private readonly Model model;
        private readonly MulticastServerService mserver;
        private readonly LANPeerFinderService peerFinder;
        private readonly Node transmitted = new Node();
        private readonly AutoResetEvent workerEvent = new AutoResetEvent(true);
        private bool run = true;

        public ConnectionController(IContainer c)
        {
            model = c.Resolve<Model>();
            mserver = c.Resolve<MulticastServerService>();
            peerFinder = c.Resolve<LANPeerFinderService>();
            setupLocalNetwork();
        }

        private void setupLocalNetwork()
        {
            // Domain.Entities.Network network = new Domain.Entities.Network();
            model.Network.NetworkName = "Local";
            model.Network.NetworkID = "Local";
            model.Network.State = ConnectionState.Disconnected;
            model.PropertyChanged += model_PropertyChanged;
            model.LocalNode.PropertyChanged += LocalNode_PropertyChanged;
            // model.Networks.Add(network);
        }

        private void LocalNode_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(CheckModelChangesAsync);
        }

        private void model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "State")
                workerEvent.Set();
        }

        public void SendMessage(string message)
        {
            var verb = new ChatVerb();
            verb.Message = message;
            verb.Nickname = model.LocalNode.Nickname;
            verb.SourceID = model.LocalNode.ID;
            ThreadPool.QueueUserWorkItem(SendMessageAsync, verb.CreateRequest());
        }

        private void SendMessageAsync(object o)
        {
            try
            {
                if (model.Network.State == ConnectionState.Connected)
                {
                    var client = new Client(model.LocalNode);
                    if (!client.Execute((NetworkRequest) o, model.Network.Overlord))
                    {
                        if (model.Network.State == ConnectionState.Connected)
                            model.Network.State = ConnectionState.Disconnected;
                    }
                }
                else
                {
                    LogManager.GetLogger("faplog").Warn("Could not send message as you are not conencted");
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger("faplog").Error("Failed to send chat message", e);
            }
        }

        public void Start()
        {
            peerFinder.Start();
            ThreadPool.QueueUserWorkItem(ProcessLanConnection);
        }


        public void Exit()
        {
            run = false;
            workerEvent.Set();
            Disconnect();
        }

        public void Disconnect()
        {
            //Notify log off
            if (model.Network.State == ConnectionState.Connected)
            {
                var c = new Client(model.LocalNode);
                var verb = new UpdateVerb();
                verb.Nodes.Add(new Node {ID = model.LocalNode.ID, Online = false});
                c.Execute(verb, model.Network.Overlord, 3000);

                //Remove peer so we dont reconnect straight away most likely
                DetectedNode peer =
                    peerFinder.Peers.Where(p => p.Address == model.Network.Overlord.Location).FirstOrDefault();
                if (null != peer)
                    peerFinder.RemovePeer(peer);
                model.Network.State = ConnectionState.Disconnected;
            }
        }

        private void ProcessLanConnection(object o)
        {
            mserver.SendMessage(WhoVerb.CreateRequest());
            Domain.Entities.Network network = model.Network;
            network.PropertyChanged += network_PropertyChanged;
            while (run)
            {
                if (network.State != ConnectionState.Connected)
                {
                    //Not connected so connect automatically..

                    //Regenerate local secret to stop any updates if we reconnecting..
                    network.Overlord = new Node();
                    network.Overlord.Secret = IDService.CreateID();
                    //Clear old peers
                    network.Nodes.Clear();

                    //Build up a prioritised server list
                    var availibleNodes = new List<DetectedNode>();

                    List<DetectedNode> detectedPeers = peerFinder.Peers.ToList();

                    //Prioritise a server we havent connected to already
                    foreach (DetectedNode peer in detectedPeers)
                    {
                        if (attemptedPeers.Where(s => s.Node == peer).Count() == 0)
                            availibleNodes.Add(peer);
                    }
                    foreach (LanPeer peer in attemptedPeers.OrderByDescending(x => x.LastConnectionTime))
                    {
                        availibleNodes.Add(peer.Node);
                    }

                    while (network.State != ConnectionState.Connected && availibleNodes.Count > 0)
                    {
                        DetectedNode node = availibleNodes[0];
                        availibleNodes.RemoveAt(0);
                        if (!Connect(network, node))
                            peerFinder.RemovePeer(node);
                    }
                }
                if (network.State == ConnectionState.Connected)
                {
                    CheckModelChanges();
                    //Check for network timeout

                    if ((Environment.TickCount - model.Network.Overlord.LastUpdate) > Model.UPLINK_TIMEOUT)
                    {
                        //We havent recently sent/recieved so went a noop so check we are still connected.
                        var req = new NetworkRequest
                                      {
                                          Verb = "NOOP",
                                          SourceID = model.LocalNode.ID,
                                          AuthKey = model.Network.Overlord.Secret
                                      };
                        var client = new Client(model.LocalNode);
                        if (!client.Execute(req, model.Network.Overlord, 4000))
                        {
                            if (network.State == ConnectionState.Connected)
                            {
                                Disconnect();
                            }
                        }
                    }

                    workerEvent.WaitOne(10000);
                }
                else
                    workerEvent.WaitOne(100);
            }
        }


        private void CheckModelChangesAsync(object o)
        {
            CheckModelChanges();
        }

        /// <summary>
        /// Whilst connected to a network 
        /// </summary>
        public void CheckModelChanges()
        {
            if (model.Network.State == ConnectionState.Connected)
            {
                UpdateVerb verb = null;
                lock (sync)
                {
                    var data = new Dictionary<string, string>();
                    foreach (var entry in model.LocalNode.Data)
                    {
                        if (transmitted.IsKeySet(entry.Key))
                        {
                            if (transmitted.GetData(entry.Key) != entry.Value)
                            {
                                data.Add(entry.Key, entry.Value);
                            }
                        }
                        else
                        {
                            data.Add(entry.Key, entry.Value);
                        }
                    }
                    //Data has changed, transmit the changes.
                    if (data.Count > 0)
                    {
                        verb = new UpdateVerb();
                        var n = new Node();
                        n.ID = model.LocalNode.ID;
                        foreach (var change in data)
                        {
                            n.SetData(change.Key, change.Value);
                            transmitted.SetData(change.Key, change.Value);
                        }
                        verb.Nodes.Add(n);
                    }
                }
                if (null != verb)
                {
                    var c = new Client(model.LocalNode);
                    if (!c.Execute(verb, model.Network.Overlord))
                        model.Network.State = ConnectionState.Disconnected;
                }
            }
        }

        private void network_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //When the network state changes then reconnect if needed.
            if (e.PropertyName == "State")
                workerEvent.Set();
        }

        private bool Connect(Domain.Entities.Network net, DetectedNode n)
        {
            try
            {
                LogManager.GetLogger("faplog").Info("Client connecting to {0}", n.Address);
                net.State = ConnectionState.Connecting;

                var verb = new ConnectVerb();
                verb.ClientType = ClientType.Client;
                verb.Address = model.LocalNode.Location;
                verb.Secret = IDService.CreateID();
                var client = new Client(model.LocalNode);

                transmitted.Data.Clear();
                foreach (var info in model.LocalNode.Data.ToList())
                    transmitted.SetData(info.Key, info.Value);

                net.Overlord = new Node();
                net.Overlord.Location = n.Address;
                net.Overlord.Secret = verb.Secret;
                LogManager.GetLogger("faplog").Debug("Client using secret {0}", verb.Secret);
                if (client.Execute(verb, n.Address))
                {
                    net.State = ConnectionState.Connected;
                    net.Overlord.ID = verb.OverlordID;
                    LogManager.GetLogger("faplog").Info("Client connected");
                    return true;
                }
                else
                {
                    net.Overlord = new Node();
                }
            }
            catch
            {
                net.State = ConnectionState.Disconnected;
            }
            return false;
        }
    }
}