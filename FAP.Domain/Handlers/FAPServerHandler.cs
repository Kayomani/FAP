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
        private Model model;
        private Overlord serverNode;
        private Entities.Network network;
        private BackgroundSafeObservable<ClientStream> connectedNodes = new BackgroundSafeObservable<ClientStream>();
        private BackgroundSafeObservable<string> connectingIDs = new BackgroundSafeObservable<string>();

        private MulticastServerService multicastServer = new MulticastServerService();
        private MulticastClientService multicastClient;

        private object sync = new object();

        public FAPServerHandler(IPAddress host, int port, Model m,MulticastClientService c)
        {
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
            network.NetworkID = networkId;
            network.NetworkName = networkName;
            HelloVerb verb = new HelloVerb();
            multicastServer.Start(verb.CreateRequest(serverNode.Location, network.NetworkName, network.NetworkID, serverNode.Strength));
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
            LogManager.GetLogger("faplog").Info("Server rx: {0} {1} ", req.Verb, req.Param);

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
            }
            return false;
        }

        #region Helper methods
        private void TransmitToAll(NetworkRequest r)
        {
            foreach (var peer in connectedNodes.ToList())
                peer.AddMessage(r);
        }

        private void TransmitToLocalClients(NetworkRequest r)
        {
            foreach (var peer in connectedNodes.ToList().Where(p => p.Node.NodeType == ClientType.Client))
                peer.AddMessage(r);
        }

        private void TransmitToAllOverlords(NetworkRequest r)
        {
            foreach (var peer in connectedNodes.ToList().Where(p => p.Node.NodeType == ClientType.Overlord))
                peer.AddMessage(r);
        }
        #endregion

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
            TransmitToAll(r);
            SendResponse(e, null);
            return true;
        }

        private bool HandleConnect(NetworkRequest r, RequestEventArgs e)
        {
           
            //Only allow one connect attempt at once
            lock (sync)
            {
                if (connectingIDs.Contains(r.Param))
                    return false;
                connectingIDs.Add(r.Param);
            }

            try
            {
                //Connect to the remote client 
                InfoVerb verb = new InfoVerb();
                Client client = new Client(serverNode);

                if (!client.Execute(verb, r.Param))
                    return false;
                //Connected ok
                ClientStream c = new ClientStream();
                c.OnDisconnect +=new ClientStream.Disconnect(c_OnDisconnect);
                Node n = verb.GetValidatedNode();
                if(null==n)
                    return false;
                n.Location = r.Param;
                n.Online = true;
                connectedNodes.Add(c);
                //Find client servers
                ThreadPool.QueueUserWorkItem(new WaitCallback(ScanClientAsync), n);
                //return ok
                SendResponse(e, null);
                //Send network info
                UpdateVerb update = new UpdateVerb();
                update.Nodes.Add(serverNode as Node);
                c.Start(n, serverNode);
                foreach (var peer in connectedNodes)
                    update.Nodes.Add(peer.Node);
                c.AddMessage(update.CreateRequest());
                
                return true;
            }
            catch
            {
            }
            finally
            {
                connectingIDs.Remove(r.Param);
            }

            return false;
        }

        private void c_OnDisconnect(ClientStream s)
        {
            try
            {
                connectedNodes.Remove(s);
                s.OnDisconnect -= new ClientStream.Disconnect(c_OnDisconnect);
                UpdateVerb info = new UpdateVerb();
                info.Nodes.Add(new Node() { ID = s.Node.ID, Online = false });
                TransmitToAll(info.CreateRequest());
            }
            catch
            {
            }
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
                        sb.Append(Encoding.Unicode.GetString(data, 0, length));
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
            UpdateVerb verb = new UpdateVerb();
            verb.Nodes.Add(r);
            TransmitToAll(verb.CreateRequest());
        }
        #endregion

    }
}
