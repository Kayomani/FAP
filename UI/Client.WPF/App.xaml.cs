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
using System.Reflection;
using System.Collections;
using System.Threading;
using System.Windows.Markup;
using System.Globalization;
using Autofac;
using Fap.Presentation.Panels;
using NLog;
using NLog.Config;
using System.Waf.Applications.Services;
using System.Waf.Presentation.Services;
using FAP.Application.Views;
using FAP.Domain;
using FAP.Application;
using FAP.Network;
using Fap.Foundation;

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

            Fap.Foundation.SafeObservableStatic.Dispatcher = System.Windows.Application.Current.Dispatcher;
            SafeObservingCollectionManager.Start();
            this.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);

            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));           

            string img = GetImage();
            SplashScreen appSplash = new SplashScreen(img);
            appSplash.Show(true);
            base.OnStartup(e);
            if (Compose())
            {
                if (e.Args.Length == 1 && e.Args[0] == "WAIT")
                {
                    //Delay the application starting up, used when restarting.
                    Thread.Sleep(3000);
                }

                ApplicationCore core = new ApplicationCore(container);
                if (core.Load())
                {
                    core.StartClientServer();
                    core.StartGUI();
#if DEBUG2
                    core.StartOverlordServer();
#endif
                }
                else
                {
                    Shutdown(1);
                }
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
                var logger = LogManager.GetLogger("faplog");
                logger.FatalException("Unhandled dispatcher exception",e.Exception);
            }
            e.Handled = true;
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.Exception.Message);
            if (null != container)
            {
                var logger = LogManager.GetLogger("faplog");
                logger.FatalException("Unhandled exception", e.Exception);
            }
            e.Handled = true;
        }

         private bool Compose()
        {
             var builder = new ContainerBuilder();
             try
             {
                 builder.RegisterAssemblyTypes((typeof(DomainModule).Assembly));
                 builder.RegisterAssemblyTypes((typeof(NetworkModule).Assembly));
                 builder.RegisterAssemblyTypes((typeof(ApplicationModule).Assembly));
                 builder.RegisterAssemblyTypes((typeof(GUIModule).Assembly));

                 builder.RegisterModule<DomainModule>();
                 builder.RegisterModule<NetworkModule>();
                 builder.RegisterModule<ApplicationModule>();
                 builder.RegisterModule<GUIModule>();

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

        protected override void OnExit(ExitEventArgs e)
        {
            
            container.Dispose();
            base.OnExit(e);
        }
    }
}
