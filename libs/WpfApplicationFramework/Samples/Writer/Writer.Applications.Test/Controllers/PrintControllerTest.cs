using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition.Hosting;
using Writer.Applications.ViewModels;
using System.Waf.UnitTesting;
using Writer.Applications.Documents;
using Writer.Applications.Test.Services;
using Writer.Applications.Services;
using System.Waf.Applications;

namespace Writer.Applications.Test.Controllers
{
    [TestClass]
    public class PrintControllerTest
    {
        private TestController controller;
        private CompositionContainer container;


        [TestInitialize]
        public void TestInitialize()
        {
            controller = new TestController();
            container = controller.Container;
            controller.InitializePrintController();
            controller.InitializeRichTextDocumentSupport();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            controller.ShutdownPrintController();
        }


        [TestMethod]
        public void PrintPreviewTest()
        {
            ShellViewModel shellViewModel = container.GetExportedValue<ShellViewModel>();
            MainViewModel mainViewModel = container.GetExportedValue<MainViewModel>();

            // When no document is available then the command cannot be executed
            Assert.IsFalse(mainViewModel.PrintPreviewCommand.CanExecute(null));

            // Create a new document and check that we can execute the PrintPreview command
            mainViewModel.NewCommand.Execute(null);
            Assert.IsTrue(mainViewModel.PrintPreviewCommand.CanExecute(null));
            
            // Execute the PrintPreview command and check the the PrintPreviewView is visible inside the ShellView
            mainViewModel.PrintPreviewCommand.Execute(null);
            PrintPreviewViewModel printPreviewViewModel = (PrintPreviewViewModel)
                ((IView)shellViewModel.ContentView).GetViewModel();
            
            // Execute the Close command and check that the MainView is visible again
            printPreviewViewModel.CloseCommand.Execute(null);
            Assert.AreEqual(((IView)shellViewModel.ContentView).GetViewModel(), mainViewModel);
        }

        [TestMethod]
        public void PrintTest()
        {
            MainViewModel mainViewModel = container.GetExportedValue<MainViewModel>();
            Assert.IsFalse(mainViewModel.PrintCommand.CanExecute(null));

            mainViewModel.NewCommand.Execute(null);
            Assert.IsTrue(mainViewModel.PrintCommand.CanExecute(null));

            IDocumentManager documentManager = container.GetExportedValue<IDocumentManager>();
            PrintDialogServiceMock printDialogService = 
                (PrintDialogServiceMock)container.GetExportedValue<IPrintDialogService>();

            printDialogService.ShowDialogResult = true;
            mainViewModel.PrintCommand.Execute(null);
            Assert.IsNotNull(printDialogService.DocumentPaginator);
            Assert.AreEqual(documentManager.ActiveDocument.FileName, printDialogService.Description);
            
            printDialogService.ShowDialogResult = false;
            mainViewModel.PrintCommand.Execute(null);
            Assert.IsNull(printDialogService.DocumentPaginator);
            Assert.IsNull(printDialogService.Description);
        }

        [TestMethod]
        public void UpdateCommandsTest()
        {
            IDocumentManager documentManager = container.GetExportedValue<IDocumentManager>();
            MainViewModel mainViewModel = container.GetExportedValue<MainViewModel>();

            documentManager.New(documentManager.DocumentTypes.First());
            documentManager.New(documentManager.DocumentTypes.First());
            documentManager.ActiveDocument = null;

            AssertHelper.CanExecuteChangedEvent(mainViewModel.PrintPreviewCommand, () =>
                documentManager.ActiveDocument = documentManager.Documents.First());
            AssertHelper.CanExecuteChangedEvent(mainViewModel.PrintCommand, () =>
                documentManager.ActiveDocument = documentManager.Documents.Last());
        }
    }
}
