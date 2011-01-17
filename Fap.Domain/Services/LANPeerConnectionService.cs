using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fap.Domain.Verbs;
using System.Net;
using System.Net.Sockets;
using Fap.Network.Entity;
using Fap.Network;
using System.Threading;
using Autofac;
using Fap.Foundation;
using Fap.Domain.Entity;
using Fap.Network.Services;
using Fap.Domain.Controllers;
using Fap.Foundation.Logging;
using Fap.Foundation.Services;

namespace Fap.Domain.Services
{
    public class LANPeerConnectionService
    {
        private BroadcastClient client;
        private BroadcastServer server;

        private IContainer container;
        private object sync = new object();

        //Models
        private SafeObservable<DetectedOverlord> overlordList = new SafeObservable<DetectedOverlord>();
        private Model model;
        private Node transmitted = new Node();
        private Fap.Network.Entity.Network network;
        private long lastConnected = Environment.TickCount;

        //Services
        private ConnectionService connectionService;
        private BufferService bufferService;
        private OverlordController overlord;
        private Logger logger;

        private const int OVERLORD_DETECTED_TIMEOUT = 60000;
        private bool overlord_active = false;
        private long overlord_creation_holdoff_timer = 0;


        public bool IsOverlord
        {
            get { return null != overlord; }
        }

        public LANPeerConnectionService(IContainer c)
        {
            container = c;
            client = container.Resolve<BroadcastClient>();
            client.OnBroadcastCommandRx += new BroadcastClient.BroadcastCommandRx(client_OnBroadcastCommandRx);
            connectionService = container.Resolve<ConnectionService>();
            logger = container.Resolve<Logger>();
            model = container.Resolve<Model>();
            server = container.Resolve<BroadcastServer>();
            bufferService = container.Resolve<BufferService>();
        }

        public void Start(Fap.Network.Entity.Network local)
        {
            network = local;
            local.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(local_PropertyChanged);
            logger.AddInfo("Attempting to connect to the local FAP network..");
            client.StartListener();
            //Find current servers
            ThreadPool.QueueUserWorkItem(new WaitCallback(SendWhoAsync));

            ThreadPool.QueueUserWorkItem(new WaitCallback(ConnectionHandler));
            ThreadPool.QueueUserWorkItem(new WaitCallback(SyncWorker));
        }

        public void Stop()
        {
            if (network.State == ConnectionState.Connected)
            {
                //Notify network of disconnect
                DisconnectVerb disconnect = new DisconnectVerb(model.Node);
                string secret = string.Empty;
                Session session = GetOverlordConnection(out secret);
                if (null != session)
                {
                    Request request = disconnect.CreateRequest();
                    request.RequestID = secret;
                    Client c = new Client(bufferService, connectionService);
                    Response response = new Response();
                    if (!c.Execute(request, session, out response) || response.Status != 0)
                    {
                        logger.AddWarning("Failed to log off correctly.");
                    }
                }
            }
            if (IsOverlord)
                overlord.Stop();
        }

        private void local_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //Reset the start overlord timer each time the network status changes so on disconnect we dont just start a overlord straight away
            lastConnected = Environment.TickCount;
        }

        private void ConnectionHandler(object ox)
        {
            int sleep = 500;
            while (true)
            {
                try
                {
                    foreach (var s in overlordList.Where(o => o.LastSeen + OVERLORD_DETECTED_TIMEOUT < Environment.TickCount))
                        overlordList.Remove(s);

                    //Order servers by priority
                    List<DetectedOverlord> servers = overlordList.Where(s => !s.IsBanned).OrderByDescending(s => s.Strength).ToList();
                    List<DetectedOverlord> orderedList = new List<DetectedOverlord>();

                    foreach (var server in servers.Where(s => s.Strength > 666).ToList())
                    {
                        orderedList.Add(server);
                        servers.Remove(server);
                    }
                    foreach (var server in servers.Where(s => s.Strength > 333).ToList())
                    {
                        orderedList.Add(server);
                        servers.Remove(server);
                    }
                    foreach (var server in servers.ToList())
                    {
                        orderedList.Add(server);
                        servers.Remove(server);
                    }


                    int freeSlots = 0;
                    int serverCount = 0;
                    int peers = model.Peers.Count();

                    foreach (var server in orderedList)
                    {
                        int free = server.MaxClients - server.Clients;
                        if (free > 0)
                        {
                            freeSlots += free;
                            serverCount++;
                        }
                    }

                    //Does the network need an additional server?
                    bool requireNewServer = false;

                    if (peers < 10)
                    {
                        if (freeSlots < 2)
                            requireNewServer = true;
                    }
                    else if (peers < 100)
                    {
                        if (freeSlots < 10 || serverCount < 2)
                            requireNewServer = true;
                    }
                    else
                    {
                        if (freeSlots < (peers * 0.05) || serverCount<3)
                            requireNewServer = true;
                    }

                    if (!requireNewServer)
                    {
                        requireNewServer = orderedList.Count == 0;
                    }

                    if (requireNewServer)
                    {
                        if (!overlord_active)
                        {
                            if (overlord_creation_holdoff_timer == 0)
                            {
                                Random r = new Random(Environment.TickCount);
                                int holdoff = 0;
                                //No timer set, set it up
                                switch (model.MaxOverlordPeers)
                                {
                                    case OverlordLimits.HIGH_PRIORITY:
                                        holdoff = r.Next(0, 1500);
                                        break;
                                    case OverlordLimits.LOW_PRIORITY:
                                        holdoff = r.Next(3000, 5000);
                                        break;
                                    default:
                                        holdoff = r.Next(1500, 3000);
                                        break;
                                }
                                logger.AddInfo("Overlord hold off " + holdoff);
                                overlord_creation_holdoff_timer = holdoff + Environment.TickCount;
                            }
                            else if (overlord_creation_holdoff_timer < Environment.TickCount)
                            {
                                overlord_creation_holdoff_timer = 0;
                                overlord_active = true;
                                logger.AddInfo("Starting a local overlord..");
                                overlord = container.Resolve<OverlordController>();
                                overlord.Start(GetLocalAddress(), 90, "LOCAL", "Local");
                                Thread.Sleep(15);
                            }
                        }
                    }
                    else
                    {
                        overlord_creation_holdoff_timer = 0;
                        //Does the local server need removing because there are too many overlords on the local network?
                        if (overlord_active && model.Overlord.Peers.Count < 3)
                        {
                            bool requireRemoval = false;

                            if (peers < 10)
                            {
                                if (serverCount > 3 && freeSlots>5)
                                    requireRemoval = true;
                            }
                            else if (peers < 100)
                            {
                                if (serverCount > 5 && freeSlots > 10)
                                    requireRemoval = true;
                            }
                            else
                            {
                                if (freeSlots > (peers * 0.25))
                                    requireRemoval = true;
                            }

                            if (requireRemoval)
                            {
                                overlord_active = false;
                                overlord.Stop();
                            }
                        }
                    }
                    
                    //Handle connection to the network
                    if (network.State != ConnectionState.Connected)
                    {
                        if (network.State != ConnectionState.Connecting)
                            network.State = ConnectionState.Connecting;

                        foreach (var server in orderedList)
                        {
                            try
                            {
                                ConnectVerb connect = new ConnectVerb(model.Node);
                                connect.RemoteLocation = model.Node.Location;

                                Client c = container.Resolve<Client>();
                                Node serverNode = new Node();
                                serverNode.Location = server.Location;
                                network.Secret = IDService.CreateID();

                                if (c.Execute(connect, serverNode, network.Secret))
                                {
                                    if (connect.Status == 0)
                                    {
                                        if (string.IsNullOrEmpty(connect.OverlordID) || string.IsNullOrEmpty(connect.NetworkID))
                                        {
                                            //We didnt get back valid network info so try another server.
                                            logger.AddWarning("Connect failed to return valid network info.");
                                            continue;
                                        }
                                        var search = model.Networks.Where(n => n.ID == connect.NetworkID).FirstOrDefault();
                                        if (null == search)
                                        {
                                            search = new Fap.Network.Entity.Network();
                                            search.ID = "LOCAL";// connect.NetworkID;
                                            model.Networks.Add(search);
                                        }
                                        search.Name = connect.Name;
                                        search.OverlordID = connect.OverlordID;
                                        search.Secret = connect.Secret;
                                        search.State = ConnectionState.Connected;
                                        logger.AddInfo("Connected to local overlord on " + serverNode.Host + " (" + search.OverlordID + ")");
                                        break;
                                    }
                                }
                                else
                                {
                                    logger.AddInfo("Connection to  " + serverNode.Host + " timed out");
                                    server.IsBanned = true;
                                }
                            }
                            catch
                            {
                                server.IsBanned = true;
                            }
                        }
                    }
                }
                catch { }
                Thread.Sleep(sleep);
            }
        }

        private void SyncWorker(object ox)
        {
            int sleep = 500;
            while (true)
            {
                Thread.Sleep(sleep);
                //Send model changes if connected
                if (null != network && network.State == ConnectionState.Connected)
                    CheckModelChanges();
                //Remove overlords we haven't seen for a bit
                foreach (var overlord in overlordList.Where(o => o.LastSeen + OVERLORD_DETECTED_TIMEOUT < Environment.TickCount).ToList())
                    overlordList.Remove(overlord);
            }
        }


        private void client_OnBroadcastCommandRx(Request cmd)
        {
            switch (cmd.Command)
            {
                case "HELLO":
                    HandleHelo(cmd);
                    break;
                case "WHO":
                    //Do nothing
                    break;
            }
        }

        private void HandleHelo(Request r)
        {
            HelloVerb hello = new HelloVerb(null);
            hello.ProcessRequest(r);
            lock (sync)
            {
                var search = overlordList.Where(o => o.Location == hello.ListenLocation).FirstOrDefault();
                if (null == search)
                {
                    overlordList.Add(new DetectedOverlord()
                    {
                        Clients = hello.Clients,
                        ID = hello.ID,
                        MaxClients = hello.MaxClients,
                        Strength = hello.Strength,
                        LastSeen = Environment.TickCount,
                        Location = hello.ListenLocation
                    });
                }
                else
                {
                    search.Clients = hello.Clients;
                    search.ID = hello.ID;
                    search.MaxClients = hello.MaxClients;
                    search.Strength = hello.Strength;
                    search.LastSeen = Environment.TickCount;
                    search.Location = hello.ListenLocation;
                }
            }
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




        private void SendWhoAsync(object o)
        {
            WhoVerb verb = new WhoVerb();
            server.SendCommand(verb.CreateRequest());
        }

        /// <summary>
        /// Whilst connected to a network 
        /// </summary>
        private void CheckModelChanges()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            foreach (var entry in model.Node.Data)
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
                var network = model.Networks.Where(n => n.ID == "LOCAL").FirstOrDefault();
                if (null != network)
                {
                    var node = model.Peers.Where(p => p.ID == network.OverlordID).FirstOrDefault();
                    if (null != node)
                    {
                        Request request = new Request();
                        request.RequestID = network.Secret;
                        request.Command = "CLIENT";
                        request.Param = model.Node.ID;
                        foreach (var change in data)
                        {
                            request.AdditionalHeaders.Add(change.Key, change.Value);
                            transmitted.SetData(change.Key, change.Value);
                        }

                        Client client = new Client(bufferService, connectionService);
                        Response response = null;
                        if (!client.Execute(request, node, out response) || response.Status != 0)
                        {
                            network.State = ConnectionState.Disconnected;
                        }
                    }
                }
            }
        }



        public void SendChatMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
                ThreadPool.QueueUserWorkItem(new WaitCallback(SendChatMessageAsync), message);
        }


        public void SendPing()
        {
            if (network.State == ConnectionState.Connected)
                ThreadPool.QueueUserWorkItem(new WaitCallback(SendPingAsync));
        }


        private void SendPingAsync(object o)
        {
            model.Node.LastUpdate = Environment.TickCount;
            PingVerb ping = new PingVerb(model.Node);
            string secret = string.Empty;
            Session overlord = GetOverlordConnection(out secret);
            var request = ping.CreateRequest();
            request.RequestID = secret;
            Client c = new Client(bufferService, connectionService);
            Response response = new Response();
            if (!c.Execute(request, overlord, out response) || response.Status != 0)
            {
                logger.AddError("Failed to send chat message, try again shortly.");
            }
            else
            {
                ping.ReceiveResponse(response);
                logger.AddError("Ping time " + ping.Time);
            }
        }

        private void SendChatMessageAsync(object o)
        {
            string message = o as string;
            string secret = string.Empty;
            Session overlord = GetOverlordConnection(out secret);
            if (null != overlord)
            {
                Request r = new Request();
                r.RequestID = secret;
                r.Command = "CHAT";
                r.Param = model.Node.ID;
                r.AdditionalHeaders.Add("Text", message);
                r.AdditionalHeaders.Add("Name", model.Node.Nickname);

                Client c = new Client(bufferService, connectionService);
                Response response = new Response();
                if (!c.Execute(r, overlord, out response) || response.Status != 0)
                {
                    logger.AddError("Failed to send chat message, try again shortly.");
                }
            }
            else
            {
                logger.AddError("Failed to send chat message, you are currently not connected.");
            }
        }

        public void Disconnect()
        {
            var local = model.Networks.Where(n=>n.ID == "LOCAL").FirstOrDefault();
            if(null!=local)
            {
                if (local.State == ConnectionState.Connected)
                {
                    logger.AddInfo("Disconnected from local network");
                    local.Secret = IDService.CreateID();

                    var currentOverlord = overlordList.Where(o => o.ID == local.OverlordID).FirstOrDefault();
                    if (null != currentOverlord)
                    {
                        currentOverlord.Ban(4000);
                    }
                    local.State = ConnectionState.Disconnected;
                }
            }
        }


        public Session GetOverlordConnection(out string secret)
        {
            if (network.State == ConnectionState.Connected)
            {
                var overlord = model.Peers.Where(n => n.ID == network.OverlordID).FirstOrDefault();
                if (null != overlord)
                {
                    secret = network.Secret;
                    return connectionService.GetClientSession(overlord);
                }
            }
            secret = null;
            return null;
        }


        public class DetectedOverlord
        {
            private long banExpire;

            public bool IsBanned
            {
                get
                {
                    return banExpire > Environment.TickCount;
                }
                set
                {
                    if (value)
                        banExpire = Environment.TickCount + OVERLORD_DETECTED_TIMEOUT;
                    else
                        banExpire = 0;
                }
            }

            public void Ban(long time)
            {
                banExpire = Environment.TickCount  +time;
            }

            public string Location { set; get; }
            public string ID { set; get; }
            public int MaxClients { set; get; }
            public int Clients { set; get; }
            public int Strength { set; get; }
            public long LastSeen { set; get; }
        }
    }
}
