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

namespace FAP.Domain.Services
{
    public class Listener
    {
        private NodeServer listener;
        private IContainer container;

        private HTTPHandler http;
        private IFAPHandler fap;

        private Model model;

        private readonly bool isServer;

        public Listener(IContainer c, bool _isServer)
        {
            container = c;
            http = new HTTPHandler(c.Resolve<ShareInfoService>(), c.Resolve<Model>(), isServer); 
            this.isServer = _isServer;
            model = container.Resolve<Model>();
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
                    listener.Start(IPAddress.Any, port);
                    trybind = false;
                    if (isServer)
                    {
                        FAPServerHandler f = new FAPServerHandler(IPAddress.Parse("10.0.0.6"), port, model, container.Resolve<MulticastClientService>());
                        fap = f;
                        f.Start("Local", "Local");
                    }
                    else
                    {
                        FAPClientHandler f = new FAPClientHandler(model);
                        fap = f;
                        f.Start();
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
