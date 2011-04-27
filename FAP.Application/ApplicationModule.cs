using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;

namespace FAP.Application
{
    public class ApplicationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConnectionController>().SingleInstance();
        }
    }
}
