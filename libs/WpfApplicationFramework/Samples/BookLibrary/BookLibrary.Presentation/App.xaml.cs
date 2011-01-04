using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Waf;
using System.Waf.Applications;
using System.Windows;
using System.Windows.Threading;
using BookLibrary.Applications.Controllers;

namespace BookLibrary.Presentation
{
    public partial class App : Application
    {
        private CompositionContainer container;
        private ApplicationController controller;


        static App()
        {
#if (DEBUG)
            WafConfiguration.Debug = true;
#endif
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

#if (DEBUG != true)
            // Don't handle the exceptions in Debug mode because otherwise the Debugger wouldn't
            // jump into the code when an exception occurs.
            DispatcherUnhandledException += AppDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;
#endif

            AggregateCatalog catalog = new AggregateCatalog();
            // Add the WpfApplicationFramework assembly to the catalog
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(Controller).Assembly));
            // Add the BookLibrary.Presentation assembly to the catalog
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            // Add the BookLibrary.Applications assembly to the catalog
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(ApplicationController).Assembly));

            container = new CompositionContainer(catalog);
            CompositionBatch batch = new CompositionBatch();
            batch.AddExportedValue(container);
            container.Compose(batch);

            controller = container.GetExportedValue<ApplicationController>();
            controller.Initialize();
            controller.Run();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            controller.Shutdown();
            container.Dispose();

            base.OnExit(e);
        }

        private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HandleException(e.Exception, false);
            e.Handled = true;
        }

        private static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception, e.IsTerminating);
        }

        private static void HandleException(Exception e, bool isTerminating)
        {
            if (e == null) { return; }

            Trace.TraceError(e.ToString());

            if (!isTerminating)
            {
                MessageBox.Show(string.Format(CultureInfo.CurrentCulture,
                        BookLibrary.Presentation.Properties.Resources.UnknownError, e.ToString())
                    , ApplicationInfo.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
