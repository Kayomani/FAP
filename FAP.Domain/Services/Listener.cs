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
using FAP.Network.Server;
using Autofac;
using System.Net;
using HttpServer;
using FAP.Domain.Handlers;
using FAP.Domain.Entities;
using FAP.Network.Services;
using FAP.Domain.Verbs;
using FAP.Domain.Net;

namespace FAP.Domain.Services
{
    public class ListenerService
    {
        private NodeServer listener;
        private IContainer container;

        private HTTPHandler http;
        private IFAPHandler fap;

        private Model model;

        private readonly bool isServer;

        public ListenerService(IContainer c,bool _isServer)
        {
            container = c;
            http = c.Resolve<HTTPHandler>(); 
            this.isServer = _isServer;
            model = container.Resolve<Model>();
        }

        public bool IsRunning
        {
            get { return listener != null; }
        }

        public void Start(int inport)
        {
            listener = new NodeServer();
            listener.OnRequest += new NodeServer.Request(listener_OnRequest);

            bool trybind = true;
            int port = inport;
            do
            {
                try
                {
                    listener.Start(IPAddress.Parse(model.IPAddress), port);
                    trybind = false;
                    if (isServer) 
                    {
                        FAPServerHandler f = new FAPServerHandler(IPAddress.Parse(model.IPAddress), port, model, container.Resolve<MulticastClientService>(), container.Resolve<LANPeerFinderService>());
                        fap = f;
                        f.Start("Local", "Local");
                    }
                    else
                    {
                        FAPClientHandler f = new FAPClientHandler(model, container.Resolve<ShareInfoService>(), container.Resolve<IConversationController>());
                        fap = f;
                        f.Start();
                        model.ClientPort = port;
                    }
                }
                catch
                {
                    //Try again
                    port++;
                    if (inport + 100 < port)
                    {
                        throw new Exception("Could to bind listener");
                    }
                }
            }
            while (trybind);
        }

        public void Stop()
        {
            listener.Stop();
            listener.OnRequest -= new NodeServer.Request(listener_OnRequest);
            listener = null;
        }

        private bool listener_OnRequest(RequestType type, RequestEventArgs arg)
        {
            if (type == RequestType.HTTP)
            {
                if (arg.Request.Method == "GET")
                  return http.Handle(arg.Request.Uri.LocalPath, arg);
            }
            else
            {
               return fap.Handle(arg);
            }
            return false;
        }
    }
}
