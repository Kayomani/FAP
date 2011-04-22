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
using Fap.Foundation;
using Fap.Network.Services;
using Autofac;
using System.Net;
using Fap.Network.Entity;
using Fap.Network;
using System.Threading;
using Fap.Domain.Verbs;
using System.Net.Sockets;
using System.Xml.Linq;
using System.Xml;
using Fap.Domain.Entity;
using Fap.Foundation.Services;
using System.Text.RegularExpressions;
using NLog;
using Fap.Domain.Services;

namespace Fap.Domain.Controllers
{
    /// <summary>
    /// TODO:
    /// Netsplit handling
    /// Node timeout
    /// </summary>
    public class ServerListenerService : AsyncControllerBase
    {
        private bool running = false;
        private FAPListener listener;
        private IContainer container;
        private BroadcastServer bserver;
        private BroadcastClient bclient;
        private ConnectionService connectionService;
        private BufferService bufferService;
        private ShareInfoService shareInfo;
        private string listenLocation;
        private Logger logService;
        private UplinkConnectionPoolService ucps;

        //Announcer
        private Thread announcer;
        private AutoResetEvent workerEvent = new AutoResetEvent(true);
        private readonly int minAnnounceFreq = 10000;

        private string networkID;
        private string networkName;
        private Model model;

        private SafeObservable<Node> externalNodes = new SafeObservable<Node>();
        private BackgroundSafeObservable<string> connectingIDs = new BackgroundSafeObservable<string>();
        private object sync = new object();

        public ServerListenerService(IContainer c)
        {
            container = c;
            logService = LogManager.GetLogger("faplog");
            model = c.Resolve<Model>();
            shareInfo = c.Resolve<ShareInfoService>();
            model.Overlord.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Overlord_PropertyChanged);
            ucps = c.Resolve<UplinkConnectionPoolService>();
        }

        private void Overlord_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "MaxPeers":
                    GenerateStrength();
                    break;
            }
        }

        #region Startup / shutdown
        public void Start(IPAddress ip, int port, string id, string name)
        {
            if (!running)
            {
                running = true;
                networkID = id;
                networkName = name;
                listener = container.Resolve<FAPListener>();
                listener.OnReceiveRequest += new FAPListener.ReceiveRequest(listener_OnReceiveRequest);
                port = listener.Start(ip, port);
                listenLocation = ip.ToString() + ":" + port;
                bserver = container.Resolve<BroadcastServer>();
                bclient = container.Resolve<BroadcastClient>();
                bufferService = container.Resolve<BufferService>();
                connectionService = container.Resolve<ConnectionService>();
                bclient.OnBroadcastCommandRx += new BroadcastClient.BroadcastCommandRx(bclient_OnBroadcastCommandRx);
                model.Overlord.Host = ip.ToString();
                model.Overlord.Port = port;
                model.Overlord.Nickname = "Overlord";
                model.Overlord.ID = IDService.CreateID();
                model.Overlord.NodeType = ClientType.Overlord;
                model.Overlord.Online = true;
                logService.Info("Overlord ID is {0}", model.Overlord.ID);
                bclient.StartListener();
                ThreadPool.QueueUserWorkItem(new WaitCallback(Process_announce));
            }
            else
                throw new Exception("Overlord already running.");
        }

        public void Stop()
        {
            announcer.Abort();
            listener.Stop();
            running = false;
            DisconnectVerb verb = new DisconnectVerb(model.Overlord);
            lock (sync)
            {
                TransmitToAll(verb.CreateRequest());
                int startTime = Environment.TickCount;

                //Wait for outstanding streams to empty for up to 3 seconds.
                while (model.Overlord.Peers.ToList().Where(p => p.Running).Select(p => p.PendingRequests).Sum() > 0 && Environment.TickCount - startTime < 3000)
                    Thread.Sleep(25);
                foreach (var p in model.Overlord.Peers)
                    p.Kill();
            }
        }

        private void GenerateStrength()
        {
            Random r = new Random(Environment.TickCount);

            switch (model.Overlord.MaxPeers)
            {
                case OverlordLimits.HIGH_PRIORITY:
                    model.Overlord.Strength = r.Next(666, 1000);
                    break;
                case OverlordLimits.LOW_PRIORITY:
                    model.Overlord.Strength = r.Next(0, 333);
                    break;
                default:
                    model.Overlord.Strength = r.Next(333, 666);
                    break;
            }
        }
        #endregion

        #region Announcer worker

        /// <summary>
        /// Handles announcing presence via broadcast.
        /// </summary>
        private void Process_announce(object o)
        {
            announcer = Thread.CurrentThread;
            while (true)
            {
                HelloVerb helo = new HelloVerb(model.Overlord);
                helo.ListenLocation = listenLocation;
                bserver.SendCommand(helo.CreateRequest());
                workerEvent.WaitOne(minAnnounceFreq);
            }
        }
        #endregion

        #region Command handlers
        /// <summary>
        /// Broadcast RX
        /// </summary>
        /// <param name="cmd"></param>
        private void bclient_OnBroadcastCommandRx(Request cmd)
        {
            switch (cmd.Command)
            {
                case "HELLO":
                    //Location announcements from other overlords
                    HandleHello(cmd);
                    break;
                case "WHO":
                    //Request from a local peer as to what severs exist - so announce.
                    workerEvent.Set();
                    break;
            }
        }

        /// <summary>
        /// Unicast RX (From connected clients)
        /// </summary>
        /// <param name="r"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        private FAPListenerRequestReturnStatus localClient_OnReceivedRequest(Request r, Socket s)
        {
            logService.Info("Overlord client RX {0} {1} ", r.Command, r.Param);
            switch (r.Command)
            {
                case "CLIENT":
                    return HandleClient(r, s);
                case "CHAT":
                    return HandleChat(r, s);
                case "PING":
                    return HandlePing(r, s);
                case "NOOP":
                    //Do nothing
                    break;
                case "DISCONNECT":
                    return HandleDisconnect(r, s);
            }
            return FAPListenerRequestReturnStatus.None;
        }

        /// <summary>
        /// Unicast RX (Non local client i.e p2p)
        /// </summary>
        /// <param name="r"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        private FAPListenerRequestReturnStatus listener_OnReceiveRequest(Request r, Socket s)
        {
            logService.Info("Overlord P2P RX {0} {1} ", r.Command, r.Param);
            switch (r.Command)
            {
                case "CONNECT":
                    //Connection request
                    return HandleConnect(r, s);
                case "INFO":
                    //Info on this node
                    return HandleInfo(r, s);
                case "PING":
                    return HandlePing(r, s);
                case "NOOP":
                    //Do nothing
                    break;
                case "UPLINK":
                    //Incoming uplink - local client on another overlord
                    return HandleUplink(r, s);
                case "BROWSE":
                    //Share request - We shouldnt get here..
                    BrowseVerb bverb = new BrowseVerb(model, shareInfo);
                    Response response = bverb.ProcessRequest(r);
                    response.AdditionalHeaders.Clear();
                    s.Send(Mediator.Serialize(response));
                    break;
                case "COMPARE":
                    //Stats request
                    VerbFactory factory = new VerbFactory();
                    var verb = factory.GetVerb(r.Command, model);
                    s.Send(Mediator.Serialize(verb.ProcessRequest(r)));
                    break;
            }
            return FAPListenerRequestReturnStatus.None;
        }
        #endregion

        #region Helper methods
        private void TransmitToAll(Request r)
        {
            foreach (var peer in model.Overlord.Peers.ToList().Where(p => p.Node.ID != model.Overlord.ID))
                peer.AddMessage(r);
        }

        private void TransmitToLocalClients(Request r)
        {
            foreach (var peer in model.Overlord.Peers.ToList().Where(p => p.Node.NodeType == ClientType.Client &&
                                                                          p.Node.ID != model.Overlord.ID))
                peer.AddMessage(r);
        }

        private void TransmitToAllOverlords(Request r)
        {
            foreach (var peer in model.Overlord.Peers.ToList().Where(p => p.Node.NodeType == ClientType.Overlord &&
                                                                          p.Node.ID != model.Overlord.ID))
                peer.AddMessage(r);
        }
        #endregion

        #region Command handlers
        private FAPListenerRequestReturnStatus HandleUplink(Request r, Socket s)
        {
            ucps.AddConnection(s, r.RequestID);
            UplinkVerb verb = new UplinkVerb(model.Node);
            s.Send(Mediator.Serialize(verb.ProcessRequest(r)));
            return FAPListenerRequestReturnStatus.ExternalHandler;
        }

        private FAPListenerRequestReturnStatus HandlePing(Request r, Socket s)
        {
            PingVerb ping = new PingVerb(null);
            //Return status based on if the node is local or not
            var search = model.Overlord.Peers.ToList().Where(p => p.Node.ID == r.Param && r.RequestID == p.Node.Secret).FirstOrDefault();
            if (null != search)
            {
                search.Node.LastUpdate = Environment.TickCount;
                ping.Status = 1;
            }
            else
                ping.Status = 0;
            s.Send(Mediator.Serialize(ping.ProcessRequest(r)));
            return FAPListenerRequestReturnStatus.None;
        }

        private FAPListenerRequestReturnStatus HandleInfo(Request r, Socket s)
        {
            InfoVerb info = new InfoVerb(model.Overlord);
            s.Send(Mediator.Serialize(info.ProcessRequest(r)));
            return FAPListenerRequestReturnStatus.None;
        }

        /// <summary>
        /// Incoming disconnect from a peer, remove and alert other peers
        /// </summary>
        /// <param name="r"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        private FAPListenerRequestReturnStatus HandleDisconnect(Request r, Socket s)
        {
           // Response response = new Response();
           //  response.RequestID = r.RequestID;
           // bool transmitResponse = false;
            lock (sync)
            {
                var search = model.Overlord.Peers.ToList().Where(p => p.Node.Secret == r.RequestID && p.Node.ID == r.Param).FirstOrDefault();

                if (null != search)
                {
                    //Local peer - Note this is not usually used, it is normal for a peer to just disconnect and this message gets raised.
                    search.Kill();
                    model.Overlord.Peers.Remove(search);
                    search.OnDisconnect -= new Uplink.Disconnect(localClient_OnDisconnect);
                    search.OnTxTimingout -= new Uplink.TxTimingout(localClient_OnTxTimingout);
                    search.OnReceivedRequest -= new FapConnectionHandler.ReceiveRequest(localClient_OnReceivedRequest);
                    TransmitToAll(r);
                 //   response.Status = 0;
                }
                else
                {
                    //Is it a peer from another overlord?
                    var osearch = externalNodes.Where(n => n.Secret == r.RequestID && r.Param == n.ID).FirstOrDefault();
                    if (null != osearch)
                    {
                        externalNodes.Remove(osearch);
                        TransmitToLocalClients(r);
                      //  response.Status = 0;
                    }
                    else
                    {
                      //  response.Status = 1;
                    }
                }
            }
          //      s.Send(Mediator.Serialize(response));
            return FAPListenerRequestReturnStatus.None;
        }

        /// <summary>
        /// Receive a chat message, if from a valid peer then forward onto all other peers
        /// </summary>
        /// <param name="r"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        private FAPListenerRequestReturnStatus HandleChat(Request r, Socket s)
        {
            Response response = new Response();
            response.RequestID = r.RequestID;
            lock (sync)
            {
                var search = model.Overlord.Peers.Where(p => p.Node.Secret == r.RequestID && p.Node.ID == r.Param).FirstOrDefault();
                if (null != search)
                {
                    //Received from a local client so transmit to the whole network
                    TransmitToAll(r);
                    search.Node.LastUpdate = Environment.TickCount;
                    // response.Status = 0;
                }
                else
                {
                    //Received from remote overlord so only transmit to local clients
                    TransmitToLocalClients(r);
                    // response.Status = 1;
                }
            }
            //s.Send(Mediator.Serialize(response));
            return FAPListenerRequestReturnStatus.None;
        }

        /// <summary>
        /// Handle a broadcast hello request.  
        /// </summary>
        /// <param name="cmd"></param>
        private void HandleHello(Request cmd)
        {
            HelloVerb verb = new HelloVerb(model.Overlord);
            lock (connectingIDs)
            {
                //Check we don't know about the peer already.
                verb.ProcessRequest(cmd);
                var search = model.Overlord.Peers.Where(p => p.Node.ID == verb.ID).FirstOrDefault();
                //Do we know the node already?
                if (null != search)
                    return;
                //Dont connect to ourselves..
                if (model.Overlord.ID == verb.ID)
                    return;
                //Are we trying to connect to this node already?
                if (connectingIDs.Contains(verb.ID))
                    return;
                connectingIDs.Add(verb.ID);
            }
            //Connect remote client async as it may take time to fail.
            ThreadPool.QueueUserWorkItem(HandleHeloAsync, verb);
        }

        /// <summary>
        /// Handle connecting to a remote peer which was announced via broadcast.
        /// </summary>
        /// <param name="o"></param>
        private void HandleHeloAsync(object o)
        {
            HelloVerb hello = o as HelloVerb;
            if (null == hello)
                return;
            try
            {
                Client c = new Client(bufferService, connectionService);
                Node node = new Node();
                node.ID = hello.ID;
                //Unknown clients are not transmitted
                node.NodeType = ClientType.Overlord;
                Uplink peer = null;
                node.Location = hello.ListenLocation;
                node.Online = true;
                ConnectVerb connect = new ConnectVerb(model.Overlord);
                connect.RemoteLocation = model.Overlord.Location;
                var request = connect.CreateRequest();
                request.RequestID = IDService.CreateID();
                node.Secret = request.RequestID;

                var session = connectionService.GetClientSession(node);
                if (session != null)
                {
                    Response response = new Response();
                    if (c.Execute(request, session, out response) && response.Status == 0)
                    {
                        peer = new Uplink(node, session, bufferService, connectionService);
                        peer.OnDisconnect += new Uplink.Disconnect(localClient_OnDisconnect);
                        peer.OnTxTimingout += new Uplink.TxTimingout(localClient_OnTxTimingout);
                        peer.OnReceivedRequest += new FapConnectionHandler.ReceiveRequest(localClient_OnReceivedRequest);

                        var serverDownlink = ucps.FindUplink(node.Secret);
                        if (null == serverDownlink)
                        {
                            //The remote server couldnt connect to us yet still returned ok??
                            serverDownlink.Shutdown(SocketShutdown.Both);
                            serverDownlink.Close();
                            return;
                        }
                        peer.Start(serverDownlink);
                        //Conencted ok, announce
                        ClientVerb info = new ClientVerb(node);
                        TransmitToLocalClients(info.CreateRequest());
                        model.Overlord.Peers.Add(peer);
                    }
                }
            }
            catch { }
            finally
            {
                connectingIDs.Remove(hello.ID);
            }
        }

        /// <summary>
        /// Receive a request containing updated client record information, if a valid client store and forward on.
        /// Alternatively this might a forwarded request from another overlord
        /// </summary>
        /// <param name="r"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        private FAPListenerRequestReturnStatus HandleClient(Request r, Socket s)
        {
            var client = model.Overlord.Peers.ToList().Where(p => p.Node.ID == r.Param && r.RequestID == p.Node.Secret).FirstOrDefault();
            if (null != client)
            {
                //Local client - Broadcast new info
                client.Node.LastUpdate = Environment.TickCount;
                if (r.AdditionalHeaders.Count > 0)
                {
                    foreach (var info in r.AdditionalHeaders)
                        client.Node.SetData(info.Key, info.Value);
                    TransmitToAll(r);
                    //Response response = new Response();
                    // response.RequestID = r.RequestID;
                    // response.Status = 0;
                    //s.Send(Mediator.Serialize(response));
                }
            }
            else
            {
                var overlord = model.Overlord.Peers.Where(p => p.Node.NodeType == ClientType.Overlord && r.RequestID == p.Node.Secret).FirstOrDefault();
                if (null != overlord)
                {
                    //Received relayed information from a registered overlord, forward on 
                    overlord.Node.LastUpdate = Environment.TickCount;
                    logService.Trace("Overlord foward client {0} from {1}", r.Param, overlord.Node.ID);
                    var search = externalNodes.Where(n => n.ID == r.Param).FirstOrDefault();
                    if (search != null)
                    {
                        search.OverlordID = overlord.Node.ID;
                        foreach (var kn in r.AdditionalHeaders)
                            search.SetData(kn.Key, kn.Value);
                    }
                    else
                    {
                        Node n = new Node();
                        n.OverlordID = overlord.Node.ID;
                        n.ID = r.Param;
                        externalNodes.Add(n);
                        foreach (var kn in r.AdditionalHeaders)
                            n.SetData(kn.Key, kn.Value);
                    }
                    TransmitToLocalClients(r);
                    // Response response = new Response();
                    // response.RequestID = r.RequestID;
                    //response.Status = 0;
                    //s.Send(Mediator.Serialize(response));
                }
                else
                {
                    logService.Warn("Overlord unreg client info for {0}", r.Param);
                    //Unregisted client or invalid info.
                    //Response response = new Response();
                    // response.RequestID = r.RequestID;
                    // response.Status = 1;
                    //s.Send(Mediator.Serialize(response));
                }
            }
            return FAPListenerRequestReturnStatus.None;
        }

        /// <summary>
        /// Receive a logon request from node 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        private FAPListenerRequestReturnStatus HandleConnect(Request r, Socket s)
        {
            Response response = new Response();
            try
            {
                Client c = new Client(bufferService, connectionService);
                Node clientNode = new Node();
                UplinkVerb info = new UplinkVerb(clientNode);

                clientNode.Location = r.Param;
                clientNode.Secret = r.RequestID;
                clientNode.NodeType = ClientType.Client;

                if (c.Execute(info, clientNode, r.RequestID))
                {
                    if (info.Status == 0)
                    {
                        Session session = connectionService.GetClientSession(clientNode);
                        lock (sync)
                        {
                            bool reconnect = false;
                            var search = model.Overlord.Peers.ToList().Where(p => p.Node.ID == clientNode.ID).FirstOrDefault();
                            //Remove old stream if there is one
                            if (search != null)
                            {
                                search.Kill();
                                model.Overlord.Peers.Remove(search);
                                search.OnDisconnect -= new Uplink.Disconnect(localClient_OnDisconnect);
                                search.OnTxTimingout -= new Uplink.TxTimingout(localClient_OnTxTimingout);
                                search.OnReceivedRequest -= new FapConnectionHandler.ReceiveRequest(localClient_OnReceivedRequest);
                                reconnect = true;
                            }

                            clientNode.LastUpdate = Environment.TickCount;
                            clientNode.OverlordID = model.Overlord.ID;

                            clientNode.Online = true;
                            Uplink newu = new Uplink(clientNode, session, bufferService, connectionService);
                            newu.OnDisconnect += new Uplink.Disconnect(localClient_OnDisconnect);
                            newu.OnTxTimingout += new Uplink.TxTimingout(localClient_OnTxTimingout);
                            newu.OnReceivedRequest += new FapConnectionHandler.ReceiveRequest(localClient_OnReceivedRequest);
                            response.Status = 0;

                            //Transmit client info to other clients
                            //If the client is reconnecting then clear out old info by sending a disconnect first.
                            if (reconnect)
                            {
                                DisconnectVerb disconnect = new DisconnectVerb(clientNode);
                                TransmitToLocalClients(disconnect.CreateRequest());
                            }
                            model.Overlord.Peers.Add(newu);

                            ClientVerb verb = new ClientVerb(clientNode);
                            TransmitToAll(verb.CreateRequest());

                            //Transmit overlord info to the connecting client
                            verb = new ClientVerb(model.Overlord);
                            newu.AddMessage(verb.CreateRequest());
                            //Transmit all known clients to the connecting node
                            foreach (var client in model.Overlord.Peers.ToList().Where(p => p.Node.NodeType != ClientType.Unknown))
                            {
                                verb = new ClientVerb(client.Node);
                                newu.AddMessage(verb.CreateRequest());
                            }
                            ThreadPool.QueueUserWorkItem(new WaitCallback(ScanClientAsync), newu.Node);
                            newu.Start(s);
                        }
                    }
                    else
                    {
                        response.Status = 3;//Other error
                    }

                }
                else
                {
                    response.Status = 2;//Could not connect
                }
            }
            catch
            {
                response.Status = 4;
            }
            response.AdditionalHeaders.Add("Host", model.Overlord.ID);
            response.AdditionalHeaders.Add("ID", networkID);
            response.AdditionalHeaders.Add("Name", networkName);
            response.RequestID = r.RequestID;
            s.Send(Mediator.Serialize(response));

            if (response.Status == 0)
                return FAPListenerRequestReturnStatus.ExternalHandler;
            return FAPListenerRequestReturnStatus.None;
        }
        #endregion

        #region Action handlers
        private Request localClient_OnTxTimingout()
        {
            //return new PingVerb(model.Overlord).CreateRequest();
            NoopVerb verb = new NoopVerb();
            return verb.CreateRequest();
        }

        private void localClient_OnDisconnect(Uplink s)
        {
            lock (sync)
            {
                if (model.Overlord.Peers.Contains(s))
                {
                    model.Overlord.Peers.Remove(s);

                    //Notify other peers
                    DisconnectVerb verb = new DisconnectVerb(s.Node);
                    TransmitToAll(verb.CreateRequest());

                    //Onos netsplit!
                    if (s.Node.NodeType == ClientType.Overlord)
                    {
                        foreach (var node in externalNodes.Where(n => n.OverlordID == s.Node.ID).ToList())
                            externalNodes.Remove(node);
                        //This is figured out client side.
                        // DisconnectVerb disc = new DisconnectVerb(node);
                        //TransmitToLocalClients(disc.CreateRequest());
                    }
                }
            }
            s.OnDisconnect -= new Uplink.Disconnect(localClient_OnDisconnect);
            s.OnTxTimingout -= new Uplink.TxTimingout(localClient_OnTxTimingout);
            s.OnReceivedRequest -= new FapConnectionHandler.ReceiveRequest(localClient_OnReceivedRequest);
        }

        #endregion

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


            //Update client
            if (n.GetData("HTTP") != webTitle ||
               n.GetData("FTP") != ftp ||
               n.GetData("Shares") != samba)
            {
                n.SetData("HTTP", webTitle);
                n.SetData("FTP", ftp);
                n.SetData("Shares", samba);
                //Fake a update request
                Request r = new Request();
                r.Command = "CLIENT";
                r.Param = n.ID;
                r.AdditionalHeaders.Add("HTTP", webTitle);
                r.AdditionalHeaders.Add("FTP", ftp);
                r.AdditionalHeaders.Add("Shares", samba);
                TransmitToAll(r);
            }
        }
        #endregion
    }
}
