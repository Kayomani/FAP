using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using FAP.Network.Services;

namespace FAP.Network
{
    public class NetworkModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MulticastServerService>().SingleInstance();
            builder.RegisterType<MulticastClientService>().SingleInstance();
        }
    }
}
