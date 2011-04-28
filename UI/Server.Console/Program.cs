using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using FAP.Domain;
using FAP.Domain.Services;
using FAP.Application;
using FAP.Network;

namespace Server.Console
{
    class Program
    {
        static void Main(string[] args)
        {

            
            Program p = new Program();
            p.Run();
        }

        private IContainer container;

        private void Run()
        {
            if(Compose())
            {

                ApplicationCore core = new ApplicationCore(container);
                core.Load();
                core.StartClientServer();
                core.StartOverlordServer();
               
                System.Console.WriteLine("Server started");
                System.Console.ReadKey();
            }
            else
            {
                System.Console.WriteLine("Program composition failed");
                System.Console.ReadKey();
            }
        }
        private bool Compose()
        {
             var builder = new ContainerBuilder();
             try
             {
                 builder.RegisterModule<DomainModule>();
                 builder.RegisterModule<NetworkModule>();
                 builder.RegisterModule<ApplicationModule>();
                 container = builder.Build();
                 builder = new ContainerBuilder();
                 builder.RegisterInstance<IContainer>(container).SingleInstance();
                 builder.Update(container);
                 return true;
             }
             catch
             {
                 return false;
             }
        }
    }
}
