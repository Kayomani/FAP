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
using Fap.Network.Services;
using Autofac;
using System.Net;
using System.Net.Sockets;
using Fap.Domain.Verbs;
using Fap.Network;
using Fap.Network.Entity;
using Fap.Foundation;
using System.Threading;
using Fap.Domain.Entity;
using Fap.Domain.Controllers;
using NLog;

namespace Fap.Domain.Services
{
    public class ClientListenerService
    {
        private FAPListener listener;
        private Model model;
        private LANPeerConnectionService peerController;
        private Logger logger;
        private BufferService bs;
        private ServerUploadLimiterService limiter;
        private UplinkConnectionPoolService ucps;
        private ShareInfoService shareInfo;

        public ClientListenerService(IContainer c)
        {
            model = c.Resolve<Model>();
            listener = c.Resolve<FAPListener>();
            peerController = c.Resolve<LANPeerConnectionService>();
            listener.OnReceiveRequest += new FAPListener.ReceiveRequest(listener_OnReceiveRequest);
            logger = LogManager.GetLogger("faplog");
            bs = c.Resolve<BufferService>();
            limiter = c.Resolve<ServerUploadLimiterService>();
            ucps = c.Resolve<UplinkConnectionPoolService>();
            shareInfo = c.Resolve<ShareInfoService>();
        }

        private FAPListenerRequestReturnStatus listener_OnReceiveRequest(Request r, Socket s)
        {
            logger.Trace("Client p2p RX  {0} {1} [{2}]", r.Command, r.Param,r.AdditionalHeaders.Count);
            switch (r.Command)
            {
                case "CLIENT":
                    //Ignore this - Should only get these on server connections.
                    logger.Error("Got Client command on p2p connection");
                    return FAPListenerRequestReturnStatus.None;
                case "CHAT":
                    //Ignore this - Should only get these on server connections.
                    logger.Error("Got Chat command on p2p connection");
                    break;
                case "DISCONNECT":
                    //Ignore this - Should only get these on server connections.
                    return FAPListenerRequestReturnStatus.None;
                case "UPLINK":
                    return HandleUplink(r, s);
                case "GET":
                    ServerUploaderService dl = new ServerUploaderService(model, bs, limiter);
                    return dl.HandleRequest(r, s); 
                case "BROWSE":
                    BrowseVerb b = new BrowseVerb(model, shareInfo);
                    s.Send(Mediator.Serialize(b.ProcessRequest(r)));
                    return FAPListenerRequestReturnStatus.None;
                default:
                    VerbFactory factory = new VerbFactory();
                    var verb = factory.GetVerb(r.Command, model);
                    s.Send(Mediator.Serialize(verb.ProcessRequest(r)));
                    return FAPListenerRequestReturnStatus.None;
            }
            return FAPListenerRequestReturnStatus.None;
        }

        private FAPListenerRequestReturnStatus HandleUplink(Request r, Socket s)
        {
            ucps.AddConnection(s, r.RequestID);
            UplinkVerb verb = new UplinkVerb(model.Node);
            s.Send(Mediator.Serialize(verb.ProcessRequest(r)));
            return FAPListenerRequestReturnStatus.ExternalHandler;
        }

        private bool IsOverlordKey(string key)
        {
            foreach (var network in model.Networks.ToList())
            {
                if (network.Secret == key)
                    return true;
            }
            return false;
        }

        private FAPListenerRequestReturnStatus HandleDisconnect(Request r, Socket s)
        {
            if (IsOverlordKey(r.RequestID))
            {
                SafeObservableStatic.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
      new Action(
       delegate()
       {
           var localNet = model.Networks.Where(n => n.ID == "LOCAL").FirstOrDefault();
           if (null != localNet)
           {
               if (localNet.State == ConnectionState.Connected)
               {
                   if (localNet.OverlordID == r.Param)
                   {
                       peerController.Disconnect();
                       var peers = model.Peers.Where(p => p.Network == localNet).ToList();
                       foreach (var p in peers)
                           model.Peers.Remove(p);
                   }
                   else
                   {
                       var search = model.Peers.Where(p => p.ID == r.Param).FirstOrDefault();
                       if (null != search)
                           model.Peers.Remove(search);
                   }
               }
           }
       }));
            }
            return FAPListenerRequestReturnStatus.None;
        }

        public void Start()
        {
            var address = GetLocalAddress();
            model.Node.Port =listener.Start(address, 95);
            model.Node.Host = address.ToString();
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
    }
}
