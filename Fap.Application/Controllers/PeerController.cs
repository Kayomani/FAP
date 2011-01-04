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
using Fap.Domain.Services;
using Fap.Domain.Entity;
using Fap.Application.ViewModels;
using Fap.Domain;
using Fap.Domain.Commands;
using System.Windows.Threading;
using System.Threading;
using Fap.Network.Services;
using Fap.Foundation.Logging;
using Fap.Network.Entity;
using Fap.Network;
using Autofac;
using Fap.Domain.Verbs;
using Fap.Domain.Controllers;
using System.Net;
using System.Net.Sockets;
using Fap.Foundation.Services;

namespace Fap.Application.Controllers
{
    /// <summary>
    /// Manages node to overlord communications,
    /// </summary>
    public class PeerController
    {
        private BroadcastClient client;
        private Timer timer;
        private IContainer container;

        List<DetectedOverlord> overlordList = new List<DetectedOverlord>();

        private object sync = new object();
        private object connectSync = new object();
        private bool running = false;
        private long startup = 0;

        private ConnectionService connectionService;
        private BufferService bufferService;
        private OverlordController overlord;
        private Logger logger;

        private Model model;
        private Node transmitted =new Node();


        public PeerController(IContainer c)
        {
            container = c;
            client = container.Resolve<BroadcastClient>();
            client.OnBroadcastCommandRx += new BroadcastClient.BroadcastCommandRx(client_OnBroadcastCommandRx);
            connectionService = container.Resolve<ConnectionService>();
            logger = container.Resolve<Logger>();
            model = container.Resolve<Model>();
            bufferService = container.Resolve<BufferService>();
        }

        private void client_OnBroadcastCommandRx(Request cmd)
        {
            switch (cmd.Command)
            {
                case "HELO":
                    HandleHelo(cmd);
                    break;
            }
        }

        private void HandleHelo(Request r)
        {
            HeloVerb hello = new HeloVerb(null);
            hello.ProcessRequest(r);
            lock (sync)
            {
                var search = overlordList.Where(o => o.Location == hello.ListenLocation).FirstOrDefault();
                if (null == search)
                {
                    overlordList.Add(new DetectedOverlord()
                    {
                        Clients = hello.Clients,
                        Index = hello.Index,
                        LastSeen = Environment.TickCount,
                        Location = hello.ListenLocation
                    });
                }
                else
                {
                    search.Clients = hello.Clients;
                    search.Index = hello.Index;
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

        public void Start()
        {
            logger.AddInfo("Attempting to connect to the local FAP network..");
            startup = Environment.TickCount;
            client.StartListener();
            timer = new Timer(new TimerCallback(Process), null, 100, 1000);
        }

        private void Process(object oj)
        {
            lock (connectSync)
            {
                if (running)
                {
                    return;
                }
                running = true;
            }


            var connected = model.Networks.Where(n => n.Connected && n.ID == "LOCAL").FirstOrDefault();
            if (null != connected)
            {
                CheckModelChanges();
                running = false;
                return;
            }


            //Copy list of servers
            List<DetectedOverlord> servers = null;
            lock (sync)
                servers = overlordList.ToList();

            servers = servers.Where(o => o.Clients < 25).OrderBy(o => o.Clients).ToList();

            //If no overlords after a time out then start our own.
            if (servers.Count == 0 && (Environment.TickCount - startup) > 5000 && null==overlord)
            {
                logger.AddInfo("No valid overlord detected after 5 seconds, starting a local overlord.");
                overlord = container.Resolve<OverlordController>();
                overlord.Start(GetLocalAddress(), 90, "LOCAL", "Local");
                running = false;
                return;
            }

            try
            {
                foreach (var server in servers)
                {
                    ConnectVerb connect = new ConnectVerb(overlord.Node);
                    connect.RemoteLocation = model.Node.Location;

                    Client c = container.Resolve<Client>();

                    Node serverNode = new Node();
                    serverNode.Location = server.Location;

                    string secret = IDService.CreateID();
                    //Add network info (Acts as permission to receive info)
                    Fap.Domain.Entity.Network nx = new Domain.Entity.Network();
                    nx.Secret = secret;
                    model.Networks.Add(nx);



                    if (c.Execute(connect, serverNode,secret))
                    {
                        if (connect.Status == 0)
                        {
                            if(string.IsNullOrEmpty(connect.OverlordID) || string.IsNullOrEmpty(connect.NetworkID))
                            {
                                //We didnt get back valid network info so try another server.
                                logger.AddWarning("Connect failed to return valid network info.");
                                continue;
                            }
                            var search = model.Networks.Where(n => n.ID == connect.NetworkID).FirstOrDefault();
                            if (null == search)
                            {
                                search = new Domain.Entity.Network();
                                search.ID = "LOCAL";// connect.NetworkID;
                                model.Networks.Add(search);
                            }
                            search.Name = connect.Name;
                            search.OverlordID = connect.OverlordID;
                            search.Secret = connect.Secret;
                            search.Connected = true;
                            logger.AddInfo("Connected to the local FAP network.");
                            model.Networks.Remove(nx);
                            break;
                        }
                    }
                    model.Networks.Remove(nx);
                }
            }
            finally
            {
                lock (connectSync)
                    running = false;
            }
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
                        if (!client.Execute(request, node, out response) || response.Status!=0)
                        {
                            network.Connected = false;
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




        private Session GetOverlordConnection(out string secret)
        {
            var connected = model.Networks.Where(n => n.Connected && n.ID == "LOCAL").FirstOrDefault();
            if (null != connected)
            {
                var overlord = model.Peers.Where(n => n.ID == connected.OverlordID).FirstOrDefault();
                if (null != overlord)
                {
                    secret = connected.Secret;
                    return connectionService.GetClientSession(overlord);
                }
            }
            secret = null;
            return null;
        }


        public class DetectedOverlord
        {
            public string Location { set; get; }
            public int Clients { set; get; }
            public long LastSeen { set; get; }
            public int Index { set; get; }
        }


    /*    private Timer timer;
        private BroadcastServer server;
        private BroadcastClient client;
        private Logger logger;
        private Model model;
        private readonly MainWindowViewModel mainWindowModel;
        private readonly BufferService bufferService;
        private readonly ConnectionService connectionService;

        public PeerController(BroadcastServer s, BroadcastClient c, Logger l, Model m, MainWindowViewModel mw, BufferService bufferService, ConnectionService connectionService)
        {
            server = s;
            client = c;
            logger = l;
            model = m;
            mainWindowModel = mw;
            this.bufferService = bufferService;
            this.connectionService = connectionService;
        }

        public void StartBroadcast()
        {
#if DEBUG
            timer = new Timer(new System.Threading.TimerCallback(processBroadcast), null, 0, 5000);
#else
            timer = new Timer(new System.Threading.TimerCallback(processBroadcast), null, 0, 15000);
#endif
        }

        public void StopBroadcast()
        {
            if (null != timer)
                timer.Dispose();
            timer = null;
        }

        public void StartBroadcastClient()
        {
            client.StartListener();
            client.OnBroadcastCommandRx += new BroadcastClient.BroadcastCommandRx(client_OnBroadcastCommandRx);
        }

        private void client_OnBroadcastCommandRx(ICommsCommand cmd)
        {
            if (cmd is Hello)
                ProcessHello(cmd as Hello);
            if (cmd is Chat)
                processChat(cmd as Chat);
            if (cmd is Quit)
                processQuit(cmd as Quit);
            if (cmd is Update)
                ProcessUpdate(cmd as Update);
        }

        private void processQuit(Quit cmd)
        {
            try
            {
                foreach (var client in model.Clients.ToList())
                {
                    if (client.Host == cmd.Address)
                    {
                        model.Clients.Remove(client);
                        client.Dispose();
                        break;
                    }
                }
            }
            catch(Exception e)
            {
                logger.LogException(e);
            }
        }

        private void processChat(Chat cmd)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now.ToShortTimeString());
            sb.Append(" ");
            sb.Append(cmd.Message);
            mainWindowModel.ChatMessages.Add(sb.ToString());
        }

        private void processBroadcast(object o)
        {
            Hello helloCMD = new Hello();
            helloCMD.Model = model;
            server.SendCommand(helloCMD);
        }

        public void AnnounceQuit()
        {
            Quit quitCMD = new Quit();
            quitCMD.Model = model;
            server.SendCommand(quitCMD);
        }

        public void AnnounceUpdate()
        {
            Update updateCMD = new Update();
            updateCMD.Model = model;
            server.SendCommand(updateCMD);
        }

        public void SendChatMessage(string msg)
        {
            Chat cmd = new Chat();
            StringBuilder sb = new StringBuilder();
            sb.Append(model.Nickname);
            sb.Append(": ");
            sb.Append(msg);
            cmd.Message =  sb.ToString();
            server.SendCommand(cmd);
        }



        private void ProcessHello(Hello h)
        {
            try
            {
                foreach (var client in model.Clients.ToList())
                {
                    if (client.Host == h.Address)
                    {
                        client.LastAccess = Environment.TickCount;
                        return;
                    }
                }

                RemoteClient r = new RemoteClient();

                r.Port = int.Parse(h.Port);
                r.Host = h.Address;
                Console.WriteLine("Testing");
                Client c = new Client(bufferService, connectionService);
                ClientInfoCMD cmd = new ClientInfoCMD(model);
                if (c.Execute(cmd, r))
                {
                    r.Nickname = cmd.Nickname;
                    r.Description = cmd.Description;
                    r.ShareSize = cmd.ShareSize;
                    r.AvatarBase64 = cmd.AvatarBase64;
                    r.LastAccess = Environment.TickCount;

                    lock (timer)
                    {
                        if (model.Clients.Where(x => x.Host == r.Host).Count() == 0)
                        {
                            model.Clients.Add(r);

                            //Scan open sessions and check info
                            foreach (var session in model.Sessions.ToList())
                            {
                                if (session.Host == r)
                                    session.User = r.Nickname;
                            }
                            logger.AddInfo("Found new client: " + r.Location);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogException(e);
            }
        }

        private void ProcessUpdate(Update h)
        {
            try
            {
                RemoteClient rc = null;

                foreach (var client in model.Clients.ToList())
                {
                    if (client.Host == h.Address)
                    {
                        client.LastAccess = Environment.TickCount;
                        rc = client;
                    }
                }
                if (null == rc)
                {
                    rc = new RemoteClient();
                    rc.Port = int.Parse(h.Port);
                    rc.Host = h.Address;
                    rc.LastAccess = Environment.TickCount;
                    Client c = new Client(bufferService,connectionService);
                    ClientInfoCMD cmd = new ClientInfoCMD(model);
                    c.Execute(cmd, rc);

                    rc.Nickname = cmd.Nickname;
                    rc.Description = cmd.Description;
                    rc.ShareSize = cmd.ShareSize;
                    rc.AvatarBase64 = cmd.AvatarBase64;
                    rc.LastAccess = Environment.TickCount;
                    model.Clients.Add(rc);
                }
                else
                {
                    Client c = new Client(bufferService,connectionService);
                    ClientInfoCMD cmd = new ClientInfoCMD(model);
                    c.Execute(cmd, rc);

                    rc.Nickname = cmd.Nickname;
                    rc.Description = cmd.Description;
                    rc.ShareSize = cmd.ShareSize;
                    rc.AvatarBase64 = cmd.AvatarBase64;
                    rc.LastAccess = Environment.TickCount;
                }

                //Scan open sessions and check info
                foreach (var session in model.Sessions.ToList())
                {
                    if (session.Host == rc)
                        session.User = rc.Nickname;
                }
                logger.AddInfo("Found new client: " + rc.Location);
            }
            catch (Exception e)
            {
                logger.LogException(e);
            }
        }*/
    }
}
        






