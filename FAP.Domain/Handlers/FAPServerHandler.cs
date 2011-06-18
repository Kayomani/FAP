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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using FAP.Domain.Entities;
using FAP.Domain.Net;
using FAP.Domain.Verbs;
using FAP.Domain.Verbs.Multicast;
using Fap.Foundation;
using Fap.Foundation.Services;
using FAP.Network;
using FAP.Network.Entities;
using FAP.Network.Services;
using HttpServer;
using HttpServer.Headers;
using HttpServer.Messages;
using NLog;

namespace FAP.Domain.Handlers
{
    public class FAPServerHandler : IFAPHandler
    {
        //List of nodes 
        private readonly AutoResetEvent announcerSync = new AutoResetEvent(true);

        private readonly BackgroundSafeObservable<ClientStream> connectedClientNodes =
            new BackgroundSafeObservable<ClientStream>();

        //List of nodes provided by external overlords
        //List if client id's currently triyng ot connect
        private readonly BackgroundSafeObservable<string> connectingIDs = new BackgroundSafeObservable<string>();
        private readonly BackgroundSafeObservable<Uplink> extOverlordServers = new BackgroundSafeObservable<Uplink>();
        private readonly BackgroundSafeObservable<Node> externalNodes = new BackgroundSafeObservable<Node>();

        private readonly Logger logger;
        private readonly Model model;
        private readonly MulticastClientService multicastClient;
        private readonly MulticastServerService multicastServer;
        private readonly Entities.Network network;
        private readonly LANPeerFinderService peerFinder;
        private readonly Overlord serverNode;
        private readonly object sync = new object();

        private bool run = true;

        public FAPServerHandler(IPAddress host, int port, Model m, MulticastClientService c, LANPeerFinderService p,
                                MulticastServerService ms)
        {
            multicastServer = ms;
            logger = LogManager.GetLogger("faplog");
            peerFinder = p;
            serverNode = new Overlord();
            serverNode.Nickname = "Overlord";
            serverNode.Host = host.ToString();
            serverNode.Port = port;
            serverNode.Online = true;
            serverNode.ID = IDService.CreateID();
            model = m;
            m.PropertyChanged += m_PropertyChanged;
            serverNode.GenerateStrength(m.OverlordPriority);
            network = new Entities.Network();
            multicastClient = c;
            multicastClient.OnMultiCastRX += multicastClient_OnMultiCastRX;
        }

        #region IFAPHandler Members

        public bool Handle(RequestEventArgs e)
        {
            NetworkRequest req = Multiplexor.Decode(e.Request);
            logger.Trace("Server rx: {0} p: {1} source: {2} overlord: {3}", req.Verb, req.Param, req.SourceID,
                         req.OverlordID);
            switch (req.Verb)
            {
                case "INFO":
                    return HandleClient(req, e);
                case "CONNECT":
                    return HandleConnect(req, e);
                case "CHAT":
                    return HandleChat(req, e);
                case "COMPARE":
                    return HandleCompare(e, req);
                case "SEARCH":
                    return HandleSearch(e, req);
                case "UPDATE":
                    return HandleUpdate(e, req);
                case "NOOP":
                    return HandleNOOP(e, req);
            }
            return false;
        }

        #endregion

        #region Helper methods

        private void SendToStandardClients(NetworkRequest r)
        {
            foreach (ClientStream peer in connectedClientNodes.ToList().Where(c => c.Node.NodeType == ClientType.Client)
                )
                peer.AddMessage(r);
        }

        private void SendToOverlordClients(NetworkRequest r)
        {
            foreach (
                ClientStream peer in connectedClientNodes.ToList().Where(c => c.Node.NodeType == ClientType.Overlord))
                peer.AddMessage(r);
        }

        private void SendToOverlordServers(NetworkRequest r)
        {
            foreach (Uplink peer in extOverlordServers)
                peer.AddMessage(r);
        }

        #endregion

        private void multicastClient_OnMultiCastRX(string cmd)
        {
            if (cmd.StartsWith(WhoVerb.Message))
                announcerSync.Set();
        }

        public void Start(string networkId, string networkName)
        {
            logger.Info("Local overlord started.");
            logger.Debug("Local overlord started with ID: {0}", serverNode.ID);
            logger.Debug("Local overlord address: {0}", serverNode.Location);
            peerFinder.Start();
            network.NetworkID = networkId;
            network.NetworkName = networkName;
            ThreadPool.QueueUserWorkItem(ProcessLanConnections);
            ThreadPool.QueueUserWorkItem(processAnnounce);
        }

        private void processAnnounce(object o)
        {
            while (run)
            {
                var verb = new HelloVerb();

                int maxClients = 0;
                if (model.IsDedicated)
                {
                    maxClients = 100;
                }
                else
                {
                    switch (model.OverlordPriority)
                    {
                        case OverlordPriority.High:
                            maxClients = 100;
                            break;
                        case OverlordPriority.Normal:
                            maxClients = 50;
                            break;
                        case OverlordPriority.Low:
                            maxClients = 40;
                            break;
                    }
                }

                multicastServer.SendMessage(verb.CreateRequest(serverNode.Location, network.NetworkName, serverNode.ID,
                                                               network.NetworkID, serverNode.Strength,
                                                               connectedClientNodes.Count, maxClients));
                announcerSync.WaitOne(10000);
            }
        }


        private void ProcessLanConnections(object no)
        {
            while (run)
            {
                List<DetectedNode> localNodes = peerFinder.Peers.ToList();

                foreach (DetectedNode peer in localNodes)
                {
                    if (peer.Address == serverNode.Location)
                        continue;

                    //If not already connected to that peer then connect
                    if (extOverlordServers.ToList().Where(o => o.Destination.Location == peer.Address).Count() == 0)
                    {
                        logger.Debug("Server connecting as client to external overlord at {0}", peer.Address);
                        var verb = new ConnectVerb();
                        verb.Address = serverNode.Location;
                        verb.ClientType = ClientType.Overlord;
                        verb.Secret = IDService.CreateID();

                        var uplink = new Uplink(model.LocalNode,
                                                new Node
                                                    {
                                                        ID = peer.OverlordID,
                                                        Location = peer.Address,
                                                        NodeType = ClientType.Overlord,
                                                        Secret = verb.Secret
                                                    });
                        extOverlordServers.Add(uplink);


                        var client = new Client(serverNode);
                        if (client.Execute(verb, peer.Address, 5000))
                        {
                            //Connected as client on an external overlord

                            uplink.OnDisconnect += uplink_OnDisconnect;

                            uplink.Start();
                            logger.Debug("Server connected to client to external overlord at {0}", peer.Address);
                            break;
                        }
                        else
                        {
                            //Failed to connect ot the external overlord
                            logger.Debug("Server failed to connect to external overlord at {0}", peer.Address);
                            peerFinder.RemovePeer(peer);
                            extOverlordServers.Remove(uplink);
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
                logger.Debug("Server had uplink disconnect to {0}", s.Destination.ID);
                extOverlordServers.Remove(s);
                var verb = new UpdateVerb();
                foreach (Node node in externalNodes.ToList())
                {
                    if (node.OverlordID == s.Destination.ID || node.ID == s.Destination.ID)
                    {
                        //Check the node isnt now logged on locally
                        if (connectedClientNodes.Where(n => n.Node.ID == node.ID).Count() > 0)
                            continue;
                        verb.Nodes.Add(new Node {ID = node.ID, Online = false});
                    }
                    externalNodes.Remove(node);
                }
                verb.Nodes.Add(new Node {ID = s.Destination.ID, Online = false});
                NetworkRequest req = verb.CreateRequest();
                req.OverlordID = serverNode.ID;
                SendToStandardClients(req);

                //Remove the overlord itself
                if (extOverlordServers.Contains(s))
                {
                    extOverlordServers.Remove(s);
                }
            }
        }

        public void Stop()
        {
            run = false;
            //Kill client connections to external overlords
            foreach (Uplink client in extOverlordServers.ToList())
                client.Kill();

            var req = new NetworkRequest {Verb = "DISCONNECT", SourceID = serverNode.ID};
            SendToStandardClients(req);
            SendToOverlordClients(req);
        }

        private void m_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "OverlordPriority":
                    serverNode.GenerateStrength(model.OverlordPriority);
                    break;
            }
        }

        private bool HandleNOOP(RequestEventArgs e, NetworkRequest req)
        {
            if (!string.IsNullOrEmpty(req.SourceID) && !string.IsNullOrEmpty(req.AuthKey))
            {
                //check details are correct
                lock (sync)
                {
                    if (null !=
                        connectedClientNodes.Where(n => n.Node.ID == req.SourceID && req.AuthKey == n.Node.Secret).
                            FirstOrDefault())
                    {
                        SendResponse(e, null);
                        return true;
                    }
                }
            }
            SendError(e);
            return false;
        }


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
                var verb = new UpdateVerb();
                verb.ProcessRequest(req);

                //Ignore updates about ourself
                if (verb.Nodes != null && verb.Nodes.Count == 1 && verb.Nodes[0].ID == serverNode.ID)
                {
                    SendResponse(e, null);
                    return true;
                }

                //Is the call from a local client?
                ClientStream localClient =
                    connectedClientNodes.ToList().Where(
                        n =>
                        n.Node.ID == req.SourceID && n.Node.Secret == req.AuthKey &&
                        n.Node.NodeType != ClientType.Overlord).FirstOrDefault();
                if (null != localClient)
                {
                    //Only allow updates about itself
                    Node client = verb.Nodes.Where(n => n.ID == localClient.Node.ID).FirstOrDefault();
                    if (null != client && verb.Nodes.Count == 1)
                    {
                        logger.Trace("Server got update from local client {0}", client.ID);
                        lock (sync)
                        {
                            //Copy to local store
                            foreach (var value in verb.Nodes[0].Data)
                                localClient.Node.SetData(value.Key, value.Value);

                            req.OverlordID = serverNode.ID;
                            //Retransmit
                            SendToOverlordClients(req);
                            SendToStandardClients(req);

                            //Has the client disconnected?
                            if (!localClient.Node.Online)
                            {
                                localClient.Kill();
                                connectedClientNodes.Remove(localClient);
                            }
                        }
                        SendResponse(e, null);
                        return true;
                    }
                }
                else
                {
                    //Is the update from an external overlord?
                    Uplink overlord =
                        extOverlordServers.ToList().Where(
                            n =>
                            n.Destination.ID == req.OverlordID && n.Destination.Secret == req.AuthKey &&
                            n.Destination.NodeType == ClientType.Overlord).FirstOrDefault();
                    if (null != overlord)
                    {
                        logger.Trace("Server got update from external overlord {0}", overlord.Destination.ID);
                        //Check each update
                        var nverb = new UpdateVerb();
                        foreach (Node update in verb.Nodes)
                        {
                            if (!string.IsNullOrEmpty(update.ID))
                            {
                                //Ignore updates about ourself
                                if (update.ID == serverNode.ID)
                                    continue;

                                lock (sync)
                                {
                                    //Is the update about the overlord itself?
                                    Uplink osearch =
                                        extOverlordServers.Where(
                                            o => o.Destination.ID == update.ID && o.Destination.Secret == req.AuthKey).
                                            FirstOrDefault();
                                    if (null != osearch)
                                    {
                                        logger.Trace("Server got update from external about itself: {0}",
                                                     osearch.Destination.ID);
                                        //Copy to local store
                                        foreach (var value in update.Data)
                                            osearch.Destination.SetData(value.Key, value.Value);
                                        //Retransmit changes
                                        nverb.Nodes.Add(update);

                                        //Overlord going offline
                                        if (!osearch.Destination.Online)
                                        {
                                            osearch.OnDisconnect -= uplink_OnDisconnect;
                                            osearch.Kill();

                                            //Remove associated external nodes
                                            foreach (Node enode in externalNodes.ToList())
                                            {
                                                if (enode.OverlordID == osearch.Destination.OverlordID)
                                                {
                                                    externalNodes.Remove(enode);
                                                    //Only signal disconnect is the node isnt a local node
                                                    //I.e. they connected locally without disconnecting externally.
                                                    ClientStream search =
                                                        connectedClientNodes.Where(n => n.Node.ID == enode.ID).
                                                            FirstOrDefault();
                                                    if (null == search)
                                                    {
                                                        //The node isn't connected locally, is it connected elsewhere externally?
                                                        Node bestExternal =
                                                            externalNodes.ToList().Where(n => n.ID == enode.ID).
                                                                OrderByDescending(n => n.LastUpdate).FirstOrDefault();
                                                        if (null != bestExternal)
                                                        {
                                                            //User has logged on elsewhere, update local clients of new details
                                                            nverb.Nodes.Add(bestExternal);
                                                        }
                                                        else
                                                        {
                                                            nverb.Nodes.Add(new Node {ID = enode.ID, Online = false});
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        logger.Trace("Server got update from external server about : {0}", update.ID);

                                        //Check to see if the external node is connected locally, if so then dont retransmit changes but store changes under the relevant object
                                        ClientStream localNode =
                                            connectedClientNodes.Where(n => n.Node.ID == update.ID).FirstOrDefault();

                                        //Update about an external node from an external overlord
                                        Node search =
                                            externalNodes.Where(n => n.ID == update.ID && n.OverlordID == req.OverlordID)
                                                .FirstOrDefault();
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
                                                //Dont retransmit external node as it is local
                                                if (null == localNode)
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
                                            //Dont retransmit external node as it is local
                                            if (null == localNode)
                                            {
                                                //Retransmit changes
                                                nverb.Nodes.Add(update);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //Only transmit external node info to local clients
                        if (nverb.Nodes.Count > 0)
                        {
                            NetworkRequest nreq = nverb.CreateRequest();
                            nreq.OverlordID = req.OverlordID;
                            SendToStandardClients(nreq);
                        }
                        SendResponse(e, null);
                        return true;
                    }
                }
            }
            catch
            {
            }
            logger.Debug("Server received an invalid update");
            SendError(e);
            return false;
        }


        private List<Node> GetBestKnownClientList()
        {
            lock (sync)
            {
                var list = new List<Node>();

                list.Add(serverNode);

                //Priority ot local clients
                foreach (ClientStream client in connectedClientNodes)
                {
                    list.Add(client.Node);
                }
                //Add the most recent instance of each external node
                foreach (var clientGroup in externalNodes.GroupBy(n => n.ID))
                {
                    //Check the client isnt a local client
                    if (clientGroup.Count() > 0)
                    {
                        int exists = list.Where(n => n.ID == clientGroup.First().ID).Count();
                        if (exists > 0)
                            continue;

                        if (clientGroup.Count() == 1)
                        {
                            list.Add(clientGroup.First());
                        }
                        else
                        {
                            Node best = clientGroup.OrderByDescending(n => n.LastUpdate).FirstOrDefault();
                            if (null != best)
                                list.Add(best);
                        }
                    }
                }
                return list;
            }
        }

        private bool HandleSearch(RequestEventArgs e, NetworkRequest req)
        {
            //We dont do this on a server..
            var verb = new SearchVerb(null);
            NetworkRequest result = verb.ProcessRequest(req);
            byte[] data = Encoding.UTF8.GetBytes(result.Data);
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
            var verb = new CompareVerb(model);

            NetworkRequest result = verb.ProcessRequest(req);
            byte[] data = Encoding.UTF8.GetBytes(result.Data);
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
                SendToStandardClients(r);
                SendToOverlordClients(r);
            }
            else
            {
                SendToStandardClients(r);
            }
            SendResponse(e, null);
            return true;
        }

        private bool HandleConnect(NetworkRequest r, RequestEventArgs e)
        {
            string address = string.Empty;

            try
            {
                var iv = new ConnectVerb();
                iv.ProcessRequest(r);
                address = iv.Address;

                if (string.IsNullOrEmpty(iv.Secret))
                {
                    //Dont allow connections with no secret
                    return false;
                }

                //Dont allow connections to ourselves..
                if (iv.Address == serverNode.Location)
                    return false;

                //Only allow one connect attempt at once
                lock (sync)
                {
                    if (connectingIDs.Contains(address))
                        return false;
                    connectingIDs.Add(address);
                }

                //Connect to the remote client 
                var verb = new InfoVerb();
                var client = new Client(serverNode);

                if (!client.Execute(verb, address))
                    return false;
                //Connected ok
                var c = new ClientStream();
                c.OnDisconnect += c_OnDisconnect;
                Node n = verb.GetValidatedNode();
                if (null == n)
                    return false;
                n.Location = iv.Address;
                n.Online = true;
                n.NodeType = iv.ClientType;
                n.OverlordID = serverNode.ID;
                n.Secret = iv.Secret;

                lock (sync)
                {
                    //Notify other clients
                    var update = new UpdateVerb();

                    //Was this person already connected?
                    ClientStream search = connectedClientNodes.Where(xn => xn.Node.ID == n.ID).FirstOrDefault();
                    if (null != search)
                    {
                        connectedClientNodes.Remove(search);
                        search.Kill();
                    }
                    c.Start(n, serverNode);
                    connectedClientNodes.Add(c);
                    update.Nodes.Add(n);
                    NetworkRequest req = update.CreateRequest();
                    req.SourceID = serverNode.ID;
                    req.OverlordID = serverNode.ID;
                    req.AuthKey = iv.Secret;

                    SendToStandardClients(req);
                    //Dont send overlord logs to other overlords
                    if (n.NodeType != ClientType.Overlord)
                        SendToOverlordClients(req);
                }
                //Find client servers
                ThreadPool.QueueUserWorkItem(ScanClientAsync, n);
                //return ok

                //Add headers
                var headers = e.Response.Headers as HeaderCollection;
                if (null != headers)
                {
                    headers.Add("FAP-AUTH", iv.Secret);
                    headers.Add("FAP-SOURCE", serverNode.ID);
                    headers.Add("FAP-OVERLORD", serverNode.ID);
                }

                SendResponse(e, null);

                //Send network info
                if (n.NodeType == ClientType.Overlord)
                {
                    var update = new UpdateVerb();
                    //Only send local nodes
                    foreach (
                        ClientStream peer in
                            connectedClientNodes.ToList().Where(x => x.Node.NodeType == ClientType.Client))
                        update.Nodes.Add(peer.Node);
                    NetworkRequest req = update.CreateRequest();
                    req.SourceID = serverNode.ID;
                    req.OverlordID = serverNode.ID;
                    req.AuthKey = iv.Secret;
                    c.AddMessage(req);
                }
                else
                {
                    var update = new UpdateVerb();
                    //None overlord client.  Send local nodes and external ones.
                    update.Nodes = GetBestKnownClientList();

                    NetworkRequest req = update.CreateRequest();
                    req.SourceID = serverNode.ID;
                    req.OverlordID = serverNode.ID;
                    req.AuthKey = iv.Secret;
                    c.AddMessage(req);
                }
                return true;
            }
            catch
            {
            }
            finally
            {
                connectingIDs.Remove(address);
            }
            SendError(e);
            return false;
        }

        private void c_OnDisconnect(ClientStream s)
        {
            try
            {
                lock (sync)
                {
                    if (connectedClientNodes.Contains(s))
                    {
                        //Check it is actually the same node
                        //  var search = connectedClientNodes.Where(n => n.Node.ID == s.Node.ID && s.Node.Secret == s.Node.Secret).FirstOrDefault();
                        // if (null == search)
                        {
                            logger.Debug("Server dropped client {0}", s.Node.ID);
                            connectedClientNodes.Remove(s);
                            s.OnDisconnect -= c_OnDisconnect;
                            var info = new UpdateVerb();
                            info.Nodes.Add(new Node {ID = s.Node.ID, Online = false});
                            NetworkRequest req = info.CreateRequest();
                            req.OverlordID = serverNode.ID;
                            req.SourceID = serverNode.ID;

                            SendToOverlordClients(req);
                            SendToStandardClients(req);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private bool HandleClient(NetworkRequest r, RequestEventArgs e)
        {
            var verb = new InfoVerb();
            verb.Node = serverNode;
            SendResponse(e, Encoding.UTF8.GetBytes(verb.CreateRequest().Data));
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

        private void SendError(RequestEventArgs e)
        {
            e.Response.Status = HttpStatusCode.MethodNotAllowed;
            e.Response.ContentLength.Value = 0;
            var generator = new ResponseWriter();
            generator.SendHeaders(e.Context, e.Response);
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
                var wc = new WebClient();
                string html = wc.DownloadString("http://" + n.Host);

                if (!string.IsNullOrEmpty(html))
                {
                    webTitle = RegexEx.FindMatches("<title>.*</title>", html).FirstOrDefault();
                    if (null != webTitle && !string.IsNullOrEmpty(html) && webTitle.Length > 14)
                    {
                        webTitle = webTitle.Substring(7);
                        webTitle = webTitle.Substring(0, webTitle.Length - 8);
                    }
                }

                if (string.IsNullOrEmpty(webTitle))
                    webTitle = "Web";
            }
            catch
            {
            }

            //Check for FTP
            string ftp = string.Empty;
            try
            {
                var client = new TcpClient();
                client.Connect(n.Host, 21);
                ftp = "FTP";
                var sb = new StringBuilder();
                long start = Environment.TickCount + 3000;
                var data = new byte[20000];
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
            catch
            {
            }

            //Check for samba shares

            string samba = string.Empty;
            try
            {
                ShareCollection shares = ShareCollection.GetShares(n.Host);
                var sb = new StringBuilder();
                foreach (SambaShare share in shares)
                {
                    if (share.IsFileSystem && share.ShareType == ShareType.Disk)
                    {
                        try
                        {
                            //Make sure its readable
                            DirectoryInfo[] Flds = share.Root.GetDirectories();
                            if (sb.Length > 0)
                                sb.Append("|");
                            sb.Append(share.NetName);
                        }
                        catch
                        {
                        }
                    }
                }
                samba = sb.ToString();
            }
            catch
            {
            }

            lock (sync)
            {
                //update clients and overlords
                var r = new Node();
                r.SetData("HTTP", webTitle.Replace("\n", "").Replace("\r", ""));
                r.SetData("FTP", ftp.Replace("\n", "").Replace("\r", ""));
                r.SetData("Shares", samba.Replace("\n", "").Replace("\r", ""));
                r.ID = n.ID;
                r.OverlordID = serverNode.ID;
                lock (sync)
                {
                    //Check the client is still connected..
                    if (connectedClientNodes.Where(nx => nx.Node.ID == r.ID).Count() > 0)
                    {
                        var verb = new UpdateVerb();
                        verb.Nodes.Add(r);
                        NetworkRequest req = verb.CreateRequest();
                        req.OverlordID = serverNode.ID;
                        SendToStandardClients(req);
                        //Dont updates about overlords to other overlords
                        if (n.NodeType != ClientType.Overlord)
                            SendToOverlordClients(req);
                    }
                }
                //Store info
                n.SetData("HTTP", webTitle.Replace("\n", "").Replace("\r", ""));
                n.SetData("FTP", ftp.Replace("\n", "").Replace("\r", ""));
                n.SetData("Shares", samba.Replace("\n", "").Replace("\r", ""));
            }
        }

        #endregion
    }
}