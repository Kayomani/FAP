using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.IO.Packaging;
using System.Waf.Applications;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;
using Writer.Applications.Documents;
using Writer.Applications.Services;
using Writer.Applications.ViewModels;
using Writer.Applications.Views;

namespace Writer.Applications.Controllers
{
    [Export]
    public class PrintController : Controller
    {
        private const string PackagePath = "pack://temp.xps";
        
        private readonly CompositionContainer container;
        private readonly ShellViewModel shellViewModel;
        private readonly MainViewModel mainViewModel;
        private readonly IDocumentManager documentManager;
        private readonly IPrintDialogService printDialogService;
        private readonly DelegateCommand printPreviewCommand;
        private readonly DelegateCommand printCommand;
        private readonly DelegateCommand closePrintPreviewCommand;
        private PrintPreviewViewModel printPreviewViewModel;
        private Package package;
        private XpsDocument xpsDocument;

        
        [ImportingConstructor]
        public PrintController(CompositionContainer container, ShellViewModel shellViewModel, 
            MainViewModel mainViewModel, IDocumentManager documentManager, IPrintDialogService printDialogService)
        {
            this.container = container;
            this.shellViewModel = shellViewModel;
            this.mainViewModel = mainViewModel;
            this.documentManager = documentManager;
            this.printDialogService = printDialogService;
            this.printPreviewCommand = new DelegateCommand(ShowPrintPreview, CanPrintDocument);
            this.printCommand = new DelegateCommand(PrintDocument, CanPrintDocument);
            this.closePrintPreviewCommand = new DelegateCommand(ClosePrintPreview);

            AddWeakEventListener(documentManager, DocumentManagerPropertyChanged);
        }


        public void Initialize()
        {
            mainViewModel.PrintPreviewCommand = printPreviewCommand;
            mainViewModel.PrintCommand = printCommand;            
        }

        public void Shutdown()
        {
            if (xpsDocument != null)
            {
                xpsDocument.Close();
            }
        }

        private bool CanPrintDocument()
        {
            return documentManager.ActiveDocument != null;
        }

        private void ShowPrintPreview()
        {
            // We have to clone the FlowDocument before we use different pagination settings for the export.        
            RichTextDocument document = documentManager.ActiveDocument as RichTextDocument;
            FlowDocument clone = document.CloneContent();

            // Create a package for the XPS document
            MemoryStream packageStream = new MemoryStream();
            package = Package.Open(packageStream, FileMode.Create, FileAccess.ReadWrite);

            // Create a XPS document with the path "pack://temp.xps"
            string path = PackagePath;
            PackageStore.AddPackage(new Uri(path), package);
            xpsDocument = new XpsDocument(package, CompressionOption.SuperFast, path);
            
            // Serialize the XPS document
            XpsSerializationManager serializer = new XpsSerializationManager(new XpsPackagingPolicy(xpsDocument), false);
            DocumentPaginator paginator = ((IDocumentPaginatorSource)clone).DocumentPaginator;
            serializer.SaveAsXaml(paginator);

            // Get the fixed document sequence
            FixedDocumentSequence documentSequence = xpsDocument.GetFixedDocumentSequence();
            
            // Create and show the print preview view
            printPreviewViewModel = new PrintPreviewViewModel(
                container.GetExportedValue<IPrintPreviewView>(), documentSequence);
            printPreviewViewModel.CloseCommand = closePrintPreviewCommand;
            shellViewModel.ContentView = printPreviewViewModel.View;
        }

        private void PrintDocument()
        {
            if (printDialogService.ShowDialog())
            {
                // We have to clone the FlowDocument before we use different pagination settings for the export.        
                RichTextDocument document = (RichTextDocument)documentManager.ActiveDocument;
                FlowDocument clone = document.CloneContent();

                printDialogService.PrintDocument(((IDocumentPaginatorSource)clone).DocumentPaginator, 
                    documentManager.ActiveDocument.FileName);
            }
        }

        private void ClosePrintPreview()
        {
            // Remove the package from the store
            PackageStore.RemovePackage(new Uri(PackagePath));

            xpsDocument.Close();
            package.Close();

            shellViewModel.ContentView = mainViewModel.View;
        }

        private void DocumentManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveDocument")
            {
                printPreviewCommand.RaiseCanExecuteChanged();
                printCommand.RaiseCanExecuteChanged();
            }
        }
    }
}
