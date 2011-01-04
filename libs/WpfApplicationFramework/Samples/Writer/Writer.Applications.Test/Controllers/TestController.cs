using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Writer.Applications.Test.Services;
using Writer.Applications.Test.Views;
using Writer.Applications.Documents;
using Writer.Applications.Controllers;
using Writer.Applications.Test.Controllers;

namespace Writer.Applications.Test.Controllers
{
    public class TestController
    {
        private readonly CompositionContainer container;
        private PrintController printController;

        
        public TestController()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(ApplicationController).Assembly));
            catalog.Catalogs.Add(new TypeCatalog(
                typeof(PresentationControllerMock),
                typeof(MessageServiceMock), typeof(FileDialogServiceMock), typeof(PrintDialogServiceMock),
                typeof(ShellViewMock), typeof(MainViewMock), typeof(RichTextViewMock), typeof(SaveChangesViewMock),
                typeof(PrintPreviewViewMock)
            ));
            container = new CompositionContainer(catalog);
            CompositionBatch batch = new CompositionBatch();
            batch.AddExportedValue(container);
            container.Compose(batch);
        }


        public CompositionContainer Container { get { return container; } }


        public void InitializePrintController()
        {
            printController = container.GetExportedValue<PrintController>();
            printController.Initialize();
        }

        public void InitializeRichTextDocumentSupport()
        {
            IDocumentManager documentManager = container.GetExportedValue<IDocumentManager>();
            RichTextDocumentController documentController = container.GetExportedValue<RichTextDocumentController>();

            documentManager.Register(new RichTextDocumentType());
        }

        public void ShutdownPrintController()
        {
            printController.Shutdown();
        }
    }
}
