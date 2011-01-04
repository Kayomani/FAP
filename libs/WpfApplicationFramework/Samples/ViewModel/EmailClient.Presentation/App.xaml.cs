using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Waf;
using System.Windows;
using EmailClient.Applications.Controllers;

namespace EmailClient.Presentation
{
    public partial class App : Application
    {
        private CompositionContainer container;

        
        static App()
        {
#if (DEBUG)
            WafConfiguration.Debug = true;
#endif
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AggregateCatalog catalog = new AggregateCatalog();
            // Add the EmailClient.Presentation assembly into the catalog
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            // Add the EmailClient.Applications assembly into the catalog
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(ApplicationController).Assembly));
            
            container = new CompositionContainer(catalog);
            CompositionBatch batch = new CompositionBatch();
            batch.AddExportedValue(container);
            container.Compose(batch);

            ApplicationController controller = container.GetExportedValue<ApplicationController>();
            controller.Initialize();
            controller.Run();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            container.Dispose();

            base.OnExit(e);
        }
    }
}
