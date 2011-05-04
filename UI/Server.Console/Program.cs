using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using FAP.Domain;
using FAP.Domain.Services;
using FAP.Application;
using FAP.Network;
using System.Net;
using System.Waf.Applications.Services;
using FAP.Application.Views;

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
                core.Load(true);
                //core.StartClientServer();
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
                 builder.RegisterAssemblyTypes((typeof(DomainModule).Assembly));
                 builder.RegisterAssemblyTypes((typeof(NetworkModule).Assembly));
                 builder.RegisterAssemblyTypes((typeof(ApplicationModule).Assembly));

                 builder.RegisterModule<DomainModule>();
                 builder.RegisterModule<NetworkModule>();
                 builder.RegisterModule<ApplicationModule>();

                 builder.RegisterType<MessageService>().As<IMessageService>();
                 builder.RegisterType<InterfaceSelectionView>().As<IInterfaceSelectionView>();
                 builder.RegisterType<SharesView>().As<ISharesView>();
                 builder.RegisterType<Query>().As<IQuery>();

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
