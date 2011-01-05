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
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Fap.Application.Controllers;
using System.Reflection;
using System.Collections;
using System.Threading;
using Fap.Foundation.Logging;
using Fap.Domain.Services;
using System.Windows.Markup;
using System.Globalization;
using Autofac;
using Fap.Application.Views;
using Fap.Presentation.Panels;
using Fap.Domain.Entity;
using Fap.Network;
using Fap.Network.Services;

namespace Fap.Presentation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private IContainer container;

        private string GetImage()
        {
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            System.Globalization.CultureInfo culture = Thread.CurrentThread.CurrentCulture;
            string resourceName = asm.GetName().Name + ".g";
            System.Resources.ResourceManager rm = new System.Resources.ResourceManager(resourceName, asm);
            System.Resources.ResourceSet resourceSet = rm.GetResourceSet(culture, true, true);
            List<string> resources = new List<string>();
            foreach (DictionaryEntry resource in resourceSet)
            {
                if(((string)resource.Key).StartsWith("images/splash%20screens/"))
                resources.Add((string)resource.Key);
            }
            rm.ReleaseAllResources();
            Random r = new Random();
            int i = r.Next(0, resources.Count());
            return resources[i];
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            //NetTest test = new NetTest();
           // test.Test();
            Fap.Foundation.SafeObservableStatic.Dispatcher = System.Windows.Application.Current.Dispatcher;
            this.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);

            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));           

            string img = GetImage();
            SplashScreen appSplash = new SplashScreen(img);
            appSplash.Show(true);
            base.OnStartup(e);
            if (Compose())
            {
                //ThreadPool.SetMaxThreads(3, 3);
             //   BufferTest bt = new BufferTest();
              //  bt.Test(container);
               // return;

                ApplicationController controller = new ApplicationController(container);
                controller.Initalise();
                controller.Run();
            }
            else
            {
                Shutdown(1);
            }
            appSplash.Close(TimeSpan.FromSeconds(0));
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.Exception.Message);
            if (null != container)
            {
                var logger = container.Resolve<Logger>();
                logger.LogException(e.Exception);
            }
            e.Handled = true;
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.Exception.Message);
            if (null != container)
            {
                var logger = container.Resolve<Logger>();
                logger.AddError("THE FOLLOWING EXCEPTION WAS UNHANDLED!!!");
                logger.LogException(e.Exception);
            }
            e.Handled = true;
        }

        private bool Compose()
        {
            var builder = new ContainerBuilder();
            try
            {

                var dataAccess = Assembly.GetExecutingAssembly();
                builder.RegisterAssemblyTypes(dataAccess);
#if DEBUG
                // Add the Application assembly
                builder.RegisterAssemblyTypes((typeof(ApplicationController).Assembly));
                // Add domain assembly
                builder.RegisterAssemblyTypes((typeof(Fap.Domain.Entity.Model).Assembly));
                // Add network assembly
                builder.RegisterAssemblyTypes((typeof(Fap.Network.Mediator).Assembly));
#endif
                /*
                 * DownloadQueueController
                 * 
                 * 
                 * */
                builder.RegisterType<BufferService>().SingleInstance();
                builder.RegisterType<ApplicationController>().SingleInstance();
                builder.RegisterType<DownloadQueueController>().SingleInstance();
                builder.RegisterType<PeerController>().SingleInstance();
                builder.RegisterType<SharesController>().SingleInstance();
                builder.RegisterType<WatchdogController>().SingleInstance();

                //UI
                builder.RegisterType<MainWindow>().As<IMainWindow>();
                builder.RegisterType<MessageBox>().As<IMessageBoxView>();
                builder.RegisterType<Fap.Presentation.Panels.DownloadQueue>().As<IDownloadQueue>();
                builder.RegisterType<SettingsPanel>().As<ISettingsView>();
                builder.RegisterType<TabWindow>().As<IPopupWindow>();
                builder.RegisterType<Query>().As<IQuery>();
                builder.RegisterType<BrowsePanel>().As<IBrowserView>();
                builder.RegisterType<LogPanel>().As<ILogView>();
                builder.RegisterType<SharesPanel>().As<ISharesView>();
                builder.RegisterType<TrayIcon>().As<ITrayIconView>();
                //Services
              //  builder.RegisterType<BroadcastClient>().SingleInstance();
              //  builder.RegisterType<BroadcastServer>().SingleInstance();
                builder.RegisterType<ClientDownloadLimiterService>().SingleInstance();
                builder.RegisterType<ConnectionService>().SingleInstance();
                builder.RegisterType<Logger>().SingleInstance();
                builder.RegisterType<ServerUploadLimiterService>().SingleInstance();
               // builder.RegisterType<BroadcastServer>().SingleInstance();
                builder.RegisterType<BroadcastClient>().SingleInstance();
                builder.RegisterType<BroadcastServer>().SingleInstance();

                //Entities
                builder.RegisterType<Model>().SingleInstance();
                container = builder.Build();

                builder = new ContainerBuilder();
                builder.RegisterInstance<IContainer>(container).SingleInstance();
                builder.Update(container);
            }
            catch(Exception e)
            {
                 System.Windows.MessageBox.Show(e.Message);
                Shutdown(1);
                return false;
            }

            return true;

            /**
            AggregateCatalog catalog = new AggregateCatalog();
            // Add the Presentation assembly into the catalog
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
#if DEBUG
            // Add the Application assembly
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(ApplicationController).Assembly));
            // Add domain assembly
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(Fap.Domain.Entity.Model).Assembly));
#endif
            container = new CompositionContainer(catalog);
            CompositionBatch batch = new CompositionBatch();
            batch.AddExportedValue(container);
            try
            {
                container.Compose(batch);
            }
            catch (CompositionException compositionException)
            {
                System.Windows.MessageBox.Show(compositionException.ToString());
                Shutdown(1);
                return false;
            }
            return true;*/
        }

        protected override void OnExit(ExitEventArgs e)
        {
            
            container.Dispose();
            base.OnExit(e);
        }
    }
}
