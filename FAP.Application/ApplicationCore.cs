using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using FAP.Domain.Services;
using FAP.Domain.Entities;
using FAP.Domain;
using FAP.Domain.Verbs;

namespace FAP.Application
{
    public class ApplicationCore
    {
        private IContainer container;

        private Listener client;
        private ShareInfoService shareInfo;
        private Listener server;
        private ConnectionController connectionController;

        private Model model;

        public ApplicationCore(IContainer c)
        {
            container = c;
            connectionController = c.Resolve<ConnectionController>();
            System.Net.ServicePointManager.Expect100Continue = false;
        }

        public void Load()
        {
            model = container.Resolve<Model>();
            model.Load();

            shareInfo = container.Resolve<ShareInfoService>();
            shareInfo.Load();
        }
        
        public void StartClientServer()
        {
            client = new Listener(container, false);
            client.Start(30);
            connectionController.Start();
        }

        public void StartOverlordServer()
        {
            server = new Listener(container, true);
            server.Start(40);
        }
    }
}
