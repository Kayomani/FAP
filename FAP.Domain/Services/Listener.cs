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
using System.Net;
using Autofac;
using FAP.Domain.Entities;
using FAP.Domain.Handlers;
using FAP.Domain.Net;
using FAP.Domain.Verbs;
using FAP.Network.Server;
using FAP.Network.Services;
using HttpServer;

namespace FAP.Domain.Services
{
    public class ListenerService
    {
        private readonly IContainer container;

        private readonly HTTPHandler http;

        private readonly bool isServer;
        private readonly Model model;
        private IFAPHandler fap;
        private NodeServer listener;

        public ListenerService(IContainer c, bool _isServer)
        {
            container = c;
            http = c.Resolve<HTTPHandler>();
            isServer = _isServer;
            model = container.Resolve<Model>();
        }

        public bool IsRunning
        {
            get { return listener != null; }
        }

        public void Start(int inport)
        {
            listener = new NodeServer();
            listener.OnRequest += listener_OnRequest;

            bool trybind = true;
            int port = inport;
            do
            {
                try
                {
                    listener.Start(IPAddress.Parse(model.LocalNode.Host), port);
                    trybind = false;
                    if (isServer)
                    {
                        var f = new FAPServerHandler(IPAddress.Parse(model.LocalNode.Host),
                                                     port,
                                                     model,
                                                     container.Resolve<MulticastClientService>(),
                                                     container.Resolve<LANPeerFinderService>(),
                                                     container.Resolve<MulticastServerService>());
                        fap = f;
                        f.Start("Local", "Local");
                    }
                    else
                    {
                        var f = new FAPClientHandler(model, container.Resolve<ShareInfoService>(),
                                                     container.Resolve<IConversationController>(),
                                                     container.Resolve<BufferService>(),
                                                     container.Resolve<ServerUploadLimiterService>());
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
            } while (trybind);
        }

        public void Stop()
        {
            listener.Stop();
            listener.OnRequest -= listener_OnRequest;
            listener = null;
            var server = fap as FAPServerHandler;
            if (null != server)
            {
                server.Stop();
            }
            else
            {
                var client = fap as FAPClientHandler;
                if (null != client)
                {
                }
            }
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