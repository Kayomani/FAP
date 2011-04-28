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
            SendResponse(e, Encoding.ASCII.GetBytes(verb.CreateRequest().Data));
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
    }
}
