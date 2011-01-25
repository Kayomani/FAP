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
using Fap.Foundation.Logging;
using System.Xml.Linq;
using System.Xml;
using Fap.Domain.Entity;
using Fap.Foundation.Services;
using System.Text.RegularExpressions;

namespace Fap.Domain.Controllers
{
    /// <summary>
    /// TODO:
    /// Netsplit handling
    /// Node timeout
    /// </summary>
    public class OverlordController : AsyncControllerBase
    {
        private bool running = false;
        private FAPListener listener;
        private IContainer container;
        private BroadcastServer bserver;
        private BroadcastClient bclient;
        private ConnectionService connectionService;
        private BufferService bufferService;
        private string listenLocation;
        private Logger logService;

        //Announcer
        private Thread announcer;
        private long lastAnnounce;
        private long lastRequest;
        private readonly long minAnnounceFreq = 10000;
        private readonly long maxAnnounceFreq = 500;

        private string networkID;
        private string networkName;
        private Model model;


        private SafeObservable<Node> externalNodes = new SafeObservable<Node>();

        public OverlordController(IContainer c)
        {
            container = c;
            logService = c.Resolve<Logger>();
            model = c.Resolve<Model>();
            model.Overlord.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Overlord_PropertyChanged);
        }

        void Overlord_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "MaxPeers":
                    GenerateStrength();
                    break;
            }
        }

        public void Start(IPAddress ip, int port, string id, string name)
        {
            if (!running)
            {
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
                bclient.StartListener();
                ThreadPool.QueueUserWorkItem(new WaitCallback(Process_announce));
            }
            else
                throw new Exception("Super node alrady running.");
        }

        public void Stop()
        {
            announcer.Abort();
            listener.Stop();
            DisconnectVerb verb = new DisconnectVerb(model.Overlord);
            TransmitToAll(verb.CreateRequest());

            int startTime = Environment.TickCount;

            //Wait for outstanding streams to empty for up to 4 seconds.
            while (model.Overlord.Peers.Where(p => p.Running).Select(p => p.PendingRequests).Sum() > 0 && Environment.TickCount - startTime < 4000)
                Thread.Sleep(25);
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

        private void TransmitToAll(Request r)
        {
            foreach (var peer in model.Overlord.Peers.ToList())
                peer.AddMessage(r);

        }

        private void TransmitToAllNonOverlords(Request r)
        {
            foreach (var peer in model.Overlord.Peers.ToList().Where(p => p.Node.NodeType != ClientType.Overlord && p.Node.NodeType != ClientType.Unknown))
                peer.AddMessage(r);
        }

        /// <summary>
        /// Broadcast RX
        /// </summary>
        /// <param name="cmd"></param>
        private void bclient_OnBroadcastCommandRx(Request cmd)
        {
            //logService.AddInfo("Overlord rx: " + cmd.Command + " P: " + cmd.Param);
            switch (cmd.Command)
            {
                case "HELLO":
                    HandleHello(cmd);
                    break;
                case "WHO":
                    lock (announcer)
                        lastRequest = Environment.TickCount;
                    break;
            }
        }

        /// <summary>
        /// Unicast RX
        /// </summary>
        /// <param name="r"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool listener_OnReceiveRequest(Request r, Socket s)
        {
            logService.AddInfo("Overlord RX " + r.Command + " " + r.Param);
            switch (r.Command)
            {
                case "CONNECT":
                    return HandleConnect(r, s);
                case "CLIENT":
                    return HandleClient(r, s);
                case "CHAT":
                    return HandleChat(r, s);
                case "DISCONNECT":
                    return HandleDisconnect(r, s);
                case "INFO":
                    return HandleInfo(r, s);
                case "PING":
                    return HandlePing(r, s);
                case "COMPARE":
                     VerbFactory factory = new VerbFactory();
                     var verb = factory.GetVerb(r.Command, model);
                     s.Send(Mediator.Serialize(verb.ProcessRequest(r)));
                     break;
            }
            return false;
        }

        private bool HandlePing(Request r, Socket s)
        {
            PingVerb ping = new PingVerb(null);
            s.Send(Mediator.Serialize(ping.ProcessRequest(r)));
            var search = model.Overlord.Peers.Where(p => p.Node.ID == r.Param && r.RequestID == p.Node.Secret).FirstOrDefault();
            if (null != search)
                search.Node.LastUpdate = Environment.TickCount;
            return false;
        }

        private bool HandleInfo(Request r, Socket s)
        {
            InfoVerb info = new InfoVerb(model.Overlord);
            s.Send(Mediator.Serialize(info.ProcessRequest(r)));
            return false;
        }

        /// <summary>
        /// Incoming disconnect from a peer, remove and alert other peers
        /// </summary>
        /// <param name="r"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool HandleDisconnect(Request r, Socket s)
        {
            var search = model.Overlord.Peers.Where(p => p.Node.Secret == r.RequestID && p.Node.ID == r.Param).FirstOrDefault();
            Response response = new Response();
            response.RequestID = r.RequestID;

            if (null != search)
            {
                model.Overlord.Peers.Remove(search);
                search.OnDisconnect -= new PeerStream.Disconnect(peer_OnDisconnect);
                TransmitToAll(r);
                response.Status = 0;
            }
            else
            {
                //Is it a peer from another overlord?
                var osearch = externalNodes.Where(n => n.Secret == r.RequestID && r.Param == n.ID).FirstOrDefault();
                if (null != osearch)
                {
                    externalNodes.Remove(osearch);
                    TransmitToAllNonOverlords(r);
                    response.Status = 0;
                }
                else
                {
                    response.Status = 1;
                }
            }
            s.Send(Mediator.Serialize(response));
            return false;
        }

        /// <summary>
        /// Receive a chat message, if from a valid peer then forward onto all other peers
        /// </summary>
        /// <param name="r"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool HandleChat(Request r, Socket s)
        {
            var search = model.Overlord.Peers.Where(p => p.Node.Secret == r.RequestID && p.Node.ID == r.Param).FirstOrDefault();
            if (null != search)
            {
                TransmitToAll(r);
                search.Node.LastUpdate = Environment.TickCount;
                Response response = new Response();
                response.RequestID = r.RequestID;
                response.Status = 0;
                s.Send(Mediator.Serialize(response));
            }
            else
            {
                Response response = new Response();
                response.RequestID = r.RequestID;
                response.Status = 1;
                s.Send(Mediator.Serialize(response));
            }
            return false;
        }

        /// <summary>
        /// Receive a broadcast hello request.  These should only be received from other overlord peers.
        /// </summary>
        /// <param name="cmd"></param>
        private void HandleHello(Request cmd)
        {
            //Check we don't know about the peer already.
            HelloVerb verb = new HelloVerb(model.Overlord);
            verb.ProcessRequest(cmd);
            var search = model.Overlord.Peers.Where(p => p.Node.ID == verb.ID).FirstOrDefault();
            if (null != search)
                return;
            if (model.Overlord.ID == verb.ID)
                return;
            if (connectingIDs.Contains(verb.ID))
                return;
            connectingIDs.Add(verb.ID);

            //Connect remote client async as it may take time to fail.
            ThreadPool.QueueUserWorkItem(HandleHeloAsync, verb);
        }

        private SafeObservable<string> connectingIDs = new SafeObservable<string>();

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
                node.NodeType = ClientType.Unknown;
                PeerStream peer = null;
                node.Location = hello.ListenLocation;
                ConnectVerb connect = new ConnectVerb(model.Overlord);
                connect.RemoteLocation = model.Overlord.Location;
                var request = connect.CreateRequest();
                request.RequestID = IDService.CreateID();
                node.Secret = request.RequestID;

                var session = connectionService.GetClientSession(node);
                if (session != null)
                {
                    peer = new PeerStream(node, session);
                    peer.OnDisconnect += new PeerStream.Disconnect(peer_OnDisconnect);
                    model.Overlord.Peers.Add(peer);
                }
                Response response = new Response();
                if (c.Execute(request, node, out response) && response.Status == 0)
                {
                    //Registered ok, announce
                    ClientVerb info = new ClientVerb(node);
                    TransmitToAllNonOverlords(info.CreateRequest());

                    //Transmit all clients to the new overlord
                    foreach (var client in model.Overlord.Peers.ToList().Where(p => p.Node.ID != node.ID))
                    {
                        info = new ClientVerb(client.Node);
                        peer.AddMessage(info.CreateRequest());
                    }
                }
                else
                {
                    //Something went wrong
                    model.Overlord.Peers.Remove(peer);
                }
            }
            catch { }
            finally
            {
                connectingIDs.Remove(hello.ID);
            }
        }

        private void peer_OnDisconnect(PeerStream s)
        {
            if (model.Overlord.Peers.Contains(s))
                model.Overlord.Peers.Remove(s);

            //Notify other peers
            DisconnectVerb verb = new DisconnectVerb(s.Node);
            TransmitToAll(verb.CreateRequest());

            //Onos netsplit!
            if (s.Node.NodeType == ClientType.Overlord)
            {
                var items = externalNodes.Where(n => n.Secret == s.Node.Secret).ToList();
                foreach (var node in items)
                {

                }
            }

            //Try to send disconnect notification
            try
            {
                verb = new DisconnectVerb(model.Overlord);
                var session = connectionService.GetClientSession(s.Node);
                var req = verb.CreateRequest();
                req.RequestID = s.Node.Secret;
                session.Socket.Send(Mediator.Serialize(req));
            }
            catch
            {

            }
            s.OnDisconnect -= new PeerStream.Disconnect(peer_OnDisconnect);
        }

        /// <summary>
        /// Handles announcing presence via broadcast.
        /// </summary>
        private void Process_announce(object o)
        {
            announcer = Thread.CurrentThread;
            int sleepTime = 25;
            while (true)
            {
                bool doAnnounce = false;
                lock (announcer)
                {
                    bool reqAnnounce = lastRequest != 0;
                    bool timerExpired = lastAnnounce + minAnnounceFreq < Environment.TickCount;
                    bool allowAnnounce = lastAnnounce + maxAnnounceFreq < Environment.TickCount;

                    if ((reqAnnounce || timerExpired) && allowAnnounce)
                    {
                        doAnnounce = true;
                        lastRequest = 0;
                    }
                }
                if (doAnnounce)
                {
                    lastAnnounce = Environment.TickCount;
                    HelloVerb helo = new HelloVerb(model.Overlord);
                    helo.ListenLocation = listenLocation;
                    bserver.SendCommand(helo.CreateRequest());
                }
                Thread.Sleep(sleepTime);
            }
        }

        /// <summary>
        /// Receive a request containing updated client record information, if a valid client store and forward on.
        /// Alternatively this might a forwarded request from another overlord
        /// </summary>
        /// <param name="r"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool HandleClient(Request r, Socket s)
        {
            var client = model.Overlord.Peers.Where(p => p.Node.ID == r.Param && r.RequestID == p.Node.Secret).FirstOrDefault();
            if (null != client)
            {
                logService.AddInfo("Overlord RX d client " + r.Param);
                client.Node.LastUpdate = Environment.TickCount;
                //Client is ok, replicate new info.
                if (r.AdditionalHeaders.Count > 0)
                {
                    foreach (var info in r.AdditionalHeaders)
                        client.Node.SetData(info.Key, info.Value);
                    TransmitToAll(r);
                    Response response = new Response();
                    response.RequestID = r.RequestID;
                    response.Status = 0;
                    s.Send(Mediator.Serialize(response));
                }
            }
            else
            {
                var overlord = model.Overlord.Peers.Where(p => p.Node.NodeType == ClientType.Overlord && r.RequestID == p.Node.Secret).FirstOrDefault();
                if (null != overlord)
                {
                    //Received relayed information from a registered overlord, forward on again
                    overlord.Node.LastUpdate = Environment.TickCount;
                    logService.AddInfo("Overlord foward client " + r.Param + " from " + overlord.Node.ID);
                    var search = externalNodes.Where(n => n.ID == r.Param).FirstOrDefault();
                    if (search != null)
                    {
                        foreach (var kn in r.AdditionalHeaders)
                            search.SetData(kn.Key, kn.Value);
                    }
                    else
                    {
                        Node n = new Node();
                        n.ID = r.Param;
                        externalNodes.Add(n);
                        foreach (var kn in r.AdditionalHeaders)
                            n.SetData(kn.Key, kn.Value);
                    }

                    TransmitToAllNonOverlords(r);
                    Response response = new Response();
                    response.RequestID = r.RequestID;
                    response.Status = 0;
                    s.Send(Mediator.Serialize(response));
                }
                else
                {
                    logService.AddInfo("Overlord unreg client " + r.Param);
                    //Unregisted client or invalid info.
                    Response response = new Response();
                    response.RequestID = r.RequestID;
                    response.Status = 1;
                    s.Send(Mediator.Serialize(response));
                }
            }

            return false;
        }

        /// <summary>
        /// Receive a logon request from node 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool HandleConnect(Request r, Socket s)
        {
            Response response = new Response();
            try
            {
                Client c = new Client(bufferService, connectionService);
                Node clientNode = new Node();
                InfoVerb info = new InfoVerb(clientNode);

                clientNode.Location = r.Param;
                clientNode.Secret = r.RequestID;
                if (c.Execute(info, clientNode))
                {
                    if (info.Status == 0)
                    {
                        bool reconnect = false;
                        var search = model.Overlord.Peers.ToList().Where(p => p.Node.ID == clientNode.ID).FirstOrDefault();
                        //Remove old stream if there is one
                        if (search != null)
                        {
                            search.Kill();
                            model.Overlord.Peers.Remove(search);
                            search.OnDisconnect -= new PeerStream.Disconnect(peer_OnDisconnect);
                            reconnect = true;
                        }

                        clientNode.LastUpdate = Environment.TickCount;
                        search = new PeerStream(clientNode, connectionService.GetClientSession(clientNode));
                        search.OnDisconnect += new PeerStream.Disconnect(peer_OnDisconnect);


                        response.Status = 0;//ok

                        //Transmit client info to other clients

                        //If the client is reconnecting then clear out old info by sending a disconnect first.
                        if (reconnect)
                        {
                            DisconnectVerb disconnect = new DisconnectVerb(clientNode);
                            TransmitToAll(disconnect.CreateRequest());
                        }

                        model.Overlord.Peers.Add(search);

                        ClientVerb verb = new ClientVerb(clientNode);
                        TransmitToAll(verb.CreateRequest());


                        //Transmit all known clients to the connecting node
                        verb = new ClientVerb(model.Overlord);
                        search.AddMessage(verb.CreateRequest());

                        foreach (var client in model.Overlord.Peers.ToList().Where(p => p.Node.NodeType != ClientType.Unknown))
                        {
                            verb = new ClientVerb(client.Node);
                            search.AddMessage(verb.CreateRequest());
                        }

                        foreach (var client in externalNodes.ToList())
                        {
                            verb = new ClientVerb(client);
                            search.AddMessage(verb.CreateRequest());
                        }
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ScanClientAsync), search.Node);
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
            return false;
        }

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
                            if (sb.Length > 0)
                                sb.Append("|");
                            //Make sure its readable
                            System.IO.DirectoryInfo[] Flds = share.Root.GetDirectories();
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
    }
}
