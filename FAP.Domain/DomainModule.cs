using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using FAP.Domain.Services;
using FAP.Domain.Entities;
using FAP.Domain.Handlers;

namespace FAP.Domain
{
    public class DomainModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ShareInfoService>().SingleInstance();
            builder.RegisterType<Listener>();
            builder.RegisterType<Model>().SingleInstance();
            builder.RegisterType<HTTPHandler>();
        }
    }
}
