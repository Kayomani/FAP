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
using HttpServer;
using FAP.Domain.Entities;
using Fap.Foundation;
using FAP.Domain.Verbs;
using System.Net;
using System.IO;
using HttpServer.Messages;
using FAP.Network.Services;
using FAP.Network.Entities;
using FAP.Network;
using NLog;
using FAP.Domain.Verbs.Multicast;
using FAP.Domain.Net;
using Fap.Foundation.Services;
using System.Net.Sockets;
using System.Threading;

namespace FAP.Domain.Handlers
{
    public class FAPServerHandler : IFAPHandler
    {
        private object sync = new object();
        private LANPeerFinderService peerFinder;

        private Model model;
        private Overlord serverNode;
        private Entities.Network network;

        private BackgroundSafeObservable<ClientStream> connectedNodes = new BackgroundSafeObservable<ClientStream>();
        private BackgroundSafeObservable<Node> externalNodes = new BackgroundSafeObservable<Node>();
        private BackgroundSafeObservable<Uplink> activeOverlords = new BackgroundSafeObservable<Uplink>();
        private BackgroundSafeObservable<string> connectingIDs = new BackgroundSafeObservable<string>();

        private MulticastServerService multicastServer = new MulticastServerService();
        private MulticastClientService multicastClient;

        public FAPServerHandler(IPAddress host, int port, Model m,MulticastClientService c,LANPeerFinderService p)
        {
            peerFinder = p;
            serverNode = new Overlord();
            serverNode.Nickname = "Overlord";
            serverNode.Host = host.ToString();
            serverNode.Port = port;
            serverNode.Online = true;
            serverNode.ID = IDService.CreateID();
            model = m;
            m.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_PropertyChanged);
            serverNode.GenerateStrength(m.OverlordPriority);
            network = new Entities.Network();
            multicastClient = c;
            multicastClient.OnMultiCastRX += new MulticastClientService.MultiCastRX(multicastClient_OnMultiCastRX);
        }

        private void multicastClient_OnMultiCastRX(string cmd)
        {
            if (cmd.StartsWith(WhoVerb.Message))
            {
                multicastServer.TriggerAnnounce();
            }
        }

        public void Start(string networkId, string networkName)
        {
            peerFinder.Start();
            network.NetworkID = networkId;
            network.NetworkName = networkName;
            HelloVerb verb = new HelloVerb();
            multicastServer.Start(verb.CreateRequest(serverNode.Location, network.NetworkName, network.NetworkID, serverNode.Strength));
        }


        private void ProcessLanConnections(object no)
        {
            while (true)
            {
                var localNodes = peerFinder.Peers.ToList();

                foreach (var peer in localNodes)
                {
                    //If not already connected to that peer then connect
                    if (activeOverlords.Where(o => o.Destination.Location != peer.Address).Count() == 0)
                    {
                        LogManager.GetLogger("faplog").Info("Server connecting as client to external overlord at {0}", peer.Address);
                        ConnectVerb verb = new ConnectVerb();
                        verb.Address = serverNode.Location;
                        verb.ClientType = ClientType.Overlord;

                        Client client = new Client(serverNode);
                        if (client.Execute(verb, peer.Address, 5000))
                        {
                            //Connected as client on an external overlord
                            Uplink uplink = new Uplink(model.LocalNode, new Node() { ID = peer.ID, Location = peer.Address,NodeType = ClientType.Overlord });
                            uplink.OnDisconnect += new Uplink.Disconnect(uplink_OnDisconnect);
                            activeOverlords.Add(uplink);
                            uplink.Start();
                            LogManager.GetLogger("faplog").Info("Server connected to client to external overlord at {0}", peer.Address);
                        }
                        else
                        {
                            //Failed to connect ot the external overlord
                            peerFinder.RemovePeer(peer);
                        }
                    }
                }
                Thread.Sleep(3000);
            }
        }

        private void uplink_OnDisconnect(Uplink s)
        {
            //A remote overlord has disconnected, notify local clients of all associated peering going offline.
            lock (sync)
            {
                activeOverlords.Remove(s);
                UpdateVerb verb = new UpdateVerb();
                foreach (var node in externalNodes)
                {
                    if (node.OverlordID == s.Destination.ID)
                    {
                        verb.Nodes.Add(new Node(){ID = node.ID,Online =false});
                    }
                }
                verb.Nodes.Add(new Node(){ID = s.Destination.ID,Online =false});
                TransmitToLocalClients(verb.CreateRequest());
            }
        }


        public void Stop()
        {
            multicastServer.Stop();
        }

        private void m_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "OverlordPriority":
                    serverNode.GenerateStrength(model.OverlordPriority);
                    break;
            }
        }

        public bool Handle(RequestEventArgs e)
        {
            NetworkRequest req = Multiplexor.Decode(e.Request);
            LogManager.GetLogger("faplog").Info("Server rx: {0} p: {1} source: {2} overlord: {3}", req.Verb, req.Param, req.SourceID, req.OverlordID);
            switch (req.Verb)
            {
                case "INFO":
                    return HandleClient(req,e);
                case "CONNECT":
                    return HandleConnect(req,e);
                case "CHAT":
                    return HandleChat(req, e);
                case "COMPARE":
                    return HandleCompare(e, req);
                case "SEARCH":
                    return HandleSearch(e, req);
                case "UPDATE":
                    return HandleUpdate(e, req);
            }
            return false;
        }

        #region Helper methods
       

        private void TransmitToLocalClients(NetworkRequest r)
        {
            foreach (var peer in connectedNodes.ToList().Where(p => p.Node.NodeType == ClientType.Client))
                peer.AddMessage(r);
        }

        private void TransmitToAllOverlords(NetworkRequest r)
        {
            foreach (var peer in activeOverlords)
                peer.AddMessage(r);
        }
        #endregion

        /// <summary>
        /// Handle updates from local clients, external overlords and peers on external overlords from their overlord.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        private bool HandleUpdate(RequestEventArgs e, NetworkRequest req)
        {
            try
            {
                UpdateVerb verb = new UpdateVerb();
                verb.ProcessRequest(req);

                //Is the call from a local client?
                var localClient = connectedNodes.ToList().Where(n => n.Node.ID == req.SourceID && n.Node.Secret == req.AuthKey && n.Node.NodeType != ClientType.Overlord).FirstOrDefault();
                if (null != localClient)
                {
                    //Only allow updates about itself
                    var client = verb.Nodes.Where(n => n.ID == localClient.Node.ID).FirstOrDefault();
                    if (null != client && verb.Nodes.Count==1)
                    {
                        LogManager.GetLogger("faplog").Info("Server got update from local client {0}", client.ID);

                        //Copy to local store
                        foreach (var value in verb.Nodes[0].Data)
                            localClient.Node.SetData(value.Key, value.Value);
                        //Retransmit
                        TransmitToLocalClients(req);
                        TransmitToAllOverlords(req);
                        SendResponse(e, null);
                        //Has the client disconnected?
                        if (!localClient.Node.Online)
                        {
                            localClient.Kill();
                            connectedNodes.Remove(localClient);
                        }
                        return true;
                    }
                }
                else
                {
                    //Is the update from an external overlord?
                    var overlord = connectedNodes.ToList().Where(n => n.Node.ID == req.OverlordID && n.Node.Secret == req.AuthKey && n.Node.NodeType == ClientType.Overlord).FirstOrDefault();
                    if (null != overlord)
                    {
                        LogManager.GetLogger("faplog").Info("Server got update from external overlord {0}", overlord.Node.ID);
                        //Check each update
                        UpdateVerb nverb = new UpdateVerb();
                        foreach (var update in verb.Nodes)
                        {
                            if (!string.IsNullOrEmpty(update.ID))
                            {
                                lock (sync)
                                {
                                    //Is the update about the overlord itself?
                                    var osearch = activeOverlords.Where(o => o.Destination.ID == update.ID && o.Destination.Secret == req.AuthKey).FirstOrDefault();
                                    if (null != osearch)
                                    {
                                        LogManager.GetLogger("faplog").Info("Server got update from external about itself: {0}", osearch.Destination.ID);
                                        //Copy to local store
                                        foreach (var value in update.Data)
                                            osearch.Destination.SetData(value.Key, value.Value);
                                        //Retransmit changes
                                        nverb.Nodes.Add(update);

                                        //Overlord going offline
                                        if (!osearch.Destination.Online)
                                        {
                                            osearch.OnDisconnect -= new Uplink.Disconnect(uplink_OnDisconnect);
                                            osearch.Kill();
                                            //Remove associated external nodes
                                            foreach (var enode in externalNodes.ToList())
                                            {
                                                if (enode.OverlordID == osearch.Destination.OverlordID)
                                                {
                                                    externalNodes.Remove(enode);
                                                    //Only signal disconnect is the node isnt a local node
                                                    //I.e. they connected locally without disconnecting externally.
                                                    var search = connectedNodes.Where(n => n.Node.ID == enode.ID).FirstOrDefault();
                                                    if (null == search)
                                                    {
                                                        //The node isn't connected locally so notify local clients of disconnect.
                                                        nverb.Nodes.Add(new Node() { ID = enode.ID, Online = false });
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        LogManager.GetLogger("faplog").Info("Server got update from external server about : {0}", update.ID);

                                        //Update about an external node from an external overlord
                                        var search = externalNodes.Where(n => n.ID == update.ID).FirstOrDefault();
                                        if (null == search)
                                        {
                                            if (!string.IsNullOrEmpty(update.ID))
                                            {
                                                //New external node
                                                search = new Node();
                                                //Copy to local store
                                                foreach (var value in update.Data)
                                                    search.SetData(value.Key, value.Value);
                                                search.OverlordID = req.OverlordID;
                                                externalNodes.Add(search);
                                                nverb.Nodes.Add(update);
                                            }
                                        }
                                        else
                                        {
                                            //Copy to local store
                                            foreach (var value in update.Data)
                                                search.SetData(value.Key, value.Value);
                                            //Has the external node changed to a different overlord?
                                            if (search.OverlordID != req.OverlordID)
                                            {
                                                search.OverlordID = req.OverlordID;
                                                update.OverlordID = req.OverlordID;
                                            }
                                            //Retransmit changes
                                            nverb.Nodes.Add(update);
                                        }
                                    }
                                }
                            }
                        }
                        //Only transmit external node info to local clients
                        if (nverb.Nodes.Count > 0)
                            TransmitToLocalClients(nverb.CreateRequest());
                        SendResponse(e, null);
                        return true;
                    }
                }
            }
            catch { }
            LogManager.GetLogger("faplog").Warn("Server received an invalid update");
            return false;
        }

        private bool HandleSearch(RequestEventArgs e, NetworkRequest req)
        {
            //We dont do this on a server..
            SearchVerb verb = new SearchVerb(null);
            var result = verb.ProcessRequest(req);
            byte[] data = Encoding.Unicode.GetBytes(result.Data);
            var generator = new ResponseWriter();
            e.Response.ContentLength.Value = data.Length;
            generator.SendHeaders(e.Context, e.Response);
            e.Context.Stream.Write(data, 0, data.Length);
            e.Context.Stream.Flush();
            data = null;
            return true;
        }
            
        private bool HandleCompare(RequestEventArgs e, NetworkRequest req)
        {
            CompareVerb verb = new CompareVerb(model);

            var result = verb.ProcessRequest(req);
            byte[] data = Encoding.Unicode.GetBytes(result.Data);
            var generator = new ResponseWriter();
            e.Response.ContentLength.Value = data.Length;
            generator.SendHeaders(e.Context, e.Response);
            e.Context.Stream.Write(data, 0, data.Length);
            e.Context.Stream.Flush();
            data = null;

            return true;
        }


        private bool HandleChat(NetworkRequest r, RequestEventArgs e)
        {
            //If an overlord id is set then this has come from an external overlord
            if (string.IsNullOrEmpty(r.OverlordID))
            {
                r.OverlordID = serverNode.ID;
                TransmitToLocalClients(r);
                TransmitToAllOverlords(r);
            }
            else
            {
                TransmitToLocalClients(r);
            }
            SendResponse(e, null);
            return true;
        }

        private bool HandleConnect(NetworkRequest r, RequestEventArgs e)
        {
            string address = string.Empty;

            try
            {
                ConnectVerb iv = new ConnectVerb();
                iv.ProcessRequest(r);
                address = iv.Address;
                //Only allow one connect attempt at once
                lock (sync)
                {
                    if (connectingIDs.Contains(address))
                        return false;
                    connectingIDs.Add(address);
                }

                //Connect to the remote client 
                InfoVerb verb = new InfoVerb();
                Client client = new Client(serverNode);

                if (!client.Execute(verb, address))
                    return false;
                //Connected ok
                ClientStream c = new ClientStream();
                c.OnDisconnect += new ClientStream.Disconnect(c_OnDisconnect);
                Node n = verb.GetValidatedNode();
                if (null == n)
                    return false;
                n.Location = iv.Address;
                n.Online = true;
                n.NodeType = iv.ClientType;
                n.OverlordID = serverNode.ID;
                n.Secret = iv.Secret;
                connectedNodes.Add(c);
                //Find client servers
                ThreadPool.QueueUserWorkItem(new WaitCallback(ScanClientAsync), n);
                //return ok
                SendResponse(e, null);

                //Send network info
                if (n.NodeType == ClientType.Overlord)
                {
                    //Only send local nodes
                    UpdateVerb update = new UpdateVerb();
                    update.Nodes.Add(serverNode as Node);
                    c.Start(n, serverNode);
                    foreach (var peer in connectedNodes.ToList())
                        update.Nodes.Add(peer.Node);
                    c.AddMessage(update.CreateRequest());
                }
                else
                {
                    //None overlord client.  Send local nodes and external ones.
                    UpdateVerb update = new UpdateVerb();
                    update.Nodes.Add(serverNode as Node);
                    c.Start(n, serverNode);

                    lock (sync)
                    {
                        //Send local nodes
                        foreach (var peer in connectedNodes)
                            update.Nodes.Add(peer.Node);
                        //Send nodes on external overlords
                        foreach (var peer in externalNodes)
                        {
                            //Only send if the peer isnt locally connected
                            var search = connectedNodes.Where(ns => ns.Node.ID == peer.ID).FirstOrDefault();
                            if (null != search)
                                update.Nodes.Add(peer);
                        }
                        //Send external overlords
                        foreach (var peer in activeOverlords)
                            update.Nodes.Add(peer.Destination);
                    }

                    c.AddMessage(update.CreateRequest());
                }
                return true;
            }
            catch { }
            finally
            {
                connectingIDs.Remove(address);
            }
            return false;
        }

        private void c_OnDisconnect(ClientStream s)
        {
            try
            {
                lock (sync)
                {
                    if (connectedNodes.Contains(s))
                    {
                        connectedNodes.Remove(s);
                        s.OnDisconnect -= new ClientStream.Disconnect(c_OnDisconnect);
                        UpdateVerb info = new UpdateVerb();
                        info.Nodes.Add(new Node() { ID = s.Node.ID, Online = false });
                        TransmitToAllOverlords(info.CreateRequest());
                        TransmitToLocalClients(info.CreateRequest());
                    }
                }
            }
            catch { }
        }

        private bool HandleClient(NetworkRequest r,RequestEventArgs e)
        {
            InfoVerb verb = new InfoVerb();
            verb.Node = serverNode;
            SendResponse(e, Encoding.Unicode.GetBytes(verb.CreateRequest().Data));
            return true;
        }

        private void SendResponse(RequestEventArgs e, byte[] data)
        {
            e.Response.Status = HttpStatusCode.OK;
            if (null != data)
                e.Response.ContentLength.Value = data.Length;
            var generator = new ResponseWriter();
            generator.SendHeaders(e.Context, e.Response);
            if (data != null && data.Length > 0)
            {
                e.Context.Stream.Write(data, 0, data.Length);
                e.Context.Stream.Flush();
            }
        }

        #region Client port service scanner
        private void ScanClientAsync(object o)
        {
            ScanClient(o as Node);
        }
        /// <summary>
        /// Scan the client machine for services such as HTTP or samba shares
        /// </summary>
        /// <param name="n"></param>
        private void ScanClient(Node n)
        {
            //Check for HTTP
            string webTitle = string.Empty;
            try
            {
                WebClient wc = new WebClient();
                string html = wc.DownloadString("http://" + n.Host);

                if (!string.IsNullOrEmpty(html))
                {
                    webTitle = RegexEx.FindMatches("<title>.*</title>", html).FirstOrDefault();
                    if (!string.IsNullOrEmpty(html) && webTitle.Length > 14)
                    {
                        webTitle = webTitle.Substring(7);
                        webTitle = webTitle.Substring(0, webTitle.Length - 8);
                    }
                }

                if (string.IsNullOrEmpty(webTitle))
                    webTitle = "Web";
            }
            catch { }

            //Check for FTP
            string ftp = string.Empty;
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(n.Host, 21);
                ftp = "FTP";
                StringBuilder sb = new StringBuilder();
                long start = Environment.TickCount + 3000;
                byte[] data = new byte[20000];
                client.ReceiveBufferSize = data.Length;

                while (start > Environment.TickCount && client.Connected)
                {
                    if (client.GetStream().DataAvailable)
                    {
                        int length = client.GetStream().Read(data, 0, data.Length);
                        sb.Append(Encoding.ASCII.GetString(data, 0, length));
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }
                client.Close();

                string title = sb.ToString();
                if (!string.IsNullOrEmpty(title))
                    ftp = title;
                data = null;
            }
            catch { }

            //Check for samba shares

            string samba = string.Empty;
            try
            {
                var shares = ShareCollection.GetShares(n.Host);
                StringBuilder sb = new StringBuilder();
                foreach (SambaShare share in shares)
                {
                    if (share.IsFileSystem && share.ShareType == ShareType.Disk)
                    {
                        try
                        {
                            //Make sure its readable
                            System.IO.DirectoryInfo[] Flds = share.Root.GetDirectories();
                            if (sb.Length > 0)
                                sb.Append("|");
                            sb.Append(share.NetName);

                        }
                        catch { }
                    }
                }
                samba = sb.ToString();
            }
            catch { }


            Node r = new Node();
            r.SetData("HTTP", webTitle);
            r.SetData("FTP", ftp);
            r.SetData("Shares", samba);
            r.ID = n.ID;
            r.OverlordID = serverNode.ID;

            UpdateVerb verb = new UpdateVerb();
            verb.Nodes.Add(r);
            TransmitToLocalClients(verb.CreateRequest());
            TransmitToAllOverlords(verb.CreateRequest());
        }
        #endregion

    }
}
