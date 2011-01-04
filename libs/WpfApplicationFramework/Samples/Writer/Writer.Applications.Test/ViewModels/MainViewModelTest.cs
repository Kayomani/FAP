using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Waf.Applications;
using System.Waf.Applications.Services;
using System.Waf.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Writer.Applications.Documents;
using Writer.Applications.Test.Controllers;
using Writer.Applications.Test.Services;
using Writer.Applications.ViewModels;

namespace Writer.Applications.Test.ViewModels
{
    [TestClass]
    public class MainViewModelTest
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

        
        [TestMethod]
        public void DocumentViewTest()
        {
            IDocumentManager documentManager = container.GetExportedValue<IDocumentManager>();
            MainViewModel mainViewModel = container.GetExportedValue<MainViewModel>();

            Assert.IsFalse(mainViewModel.DocumentViews.Any());
            Assert.IsNull(mainViewModel.ActiveDocumentView);

            mainViewModel.NewCommand.Execute(null);

            Assert.AreEqual(mainViewModel.DocumentViews.Single(), mainViewModel.ActiveDocumentView);

            mainViewModel.NewCommand.Execute(null);

            Assert.AreEqual(mainViewModel.DocumentViews.Last(), mainViewModel.ActiveDocumentView);
            Assert.AreEqual(2, mainViewModel.DocumentViews.Count);

            mainViewModel.NextDocumentCommand.Execute(null);

            Assert.AreEqual(mainViewModel.DocumentViews.First(), mainViewModel.ActiveDocumentView);

            mainViewModel.CloseCommand.Execute(null);

            Assert.AreEqual(1, mainViewModel.DocumentViews.Count);
            Assert.IsNull(mainViewModel.ActiveDocumentView);

            mainViewModel.ActiveDocumentView = mainViewModel.DocumentViews.Single();
            mainViewModel.CloseCommand.Execute(null);

            Assert.IsFalse(mainViewModel.DocumentViews.Any());
            Assert.IsNull(mainViewModel.ActiveDocumentView);
        }

        [TestMethod]
        public void OpenSaveDocumentTest()
        {
            FileDialogServiceMock fileDialogService = (FileDialogServiceMock)container.GetExportedValue<IFileDialogService>();
            MainViewModel mainViewModel = container.GetExportedValue<MainViewModel>();

            fileDialogService.Result = new FileDialogResult();
            mainViewModel.OpenCommand.Execute(null);
            Assert.AreEqual(FileDialogType.OpenFileDialog, fileDialogService.FileDialogType);

            Assert.IsFalse(mainViewModel.SaveCommand.CanExecute(null));
            Assert.IsFalse(mainViewModel.SaveAsCommand.CanExecute(null));

            mainViewModel.NewCommand.Execute(null);

            Assert.IsFalse(mainViewModel.SaveCommand.CanExecute(null));
            Assert.IsTrue(mainViewModel.SaveAsCommand.CanExecute(null));

            RichTextViewModel richTextViewModel = ((IView)mainViewModel.ActiveDocumentView).GetViewModel<RichTextViewModel>();
            
            AssertHelper.CanExecuteChangedEvent(mainViewModel.SaveCommand, () => 
                richTextViewModel.Document.Modified = true);

            Assert.IsTrue(mainViewModel.SaveCommand.CanExecute(null));
            Assert.IsTrue(mainViewModel.SaveAsCommand.CanExecute(null));

            fileDialogService.Result = new FileDialogResult();
            mainViewModel.SaveCommand.Execute(null);
            Assert.AreEqual(FileDialogType.SaveFileDialog, fileDialogService.FileDialogType);
            Assert.IsTrue(richTextViewModel.Document.Modified);

            fileDialogService.Result = new FileDialogResult();
            mainViewModel.SaveAsCommand.Execute(null);
            Assert.AreEqual(FileDialogType.SaveFileDialog, fileDialogService.FileDialogType);
        }

        [TestMethod]
        public void UpdateCommandsTest()
        {
            IDocumentManager documentManager = container.GetExportedValue<IDocumentManager>();
            MainViewModel mainViewModel = container.GetExportedValue<MainViewModel>();

            documentManager.New(documentManager.DocumentTypes.First());
            documentManager.New(documentManager.DocumentTypes.First());
            documentManager.ActiveDocument = null;

            AssertHelper.CanExecuteChangedEvent(mainViewModel.CloseCommand, () =>
                documentManager.ActiveDocument = documentManager.Documents.First());
            AssertHelper.CanExecuteChangedEvent(mainViewModel.SaveCommand, () =>
                documentManager.ActiveDocument = documentManager.Documents.Last());
            AssertHelper.CanExecuteChangedEvent(mainViewModel.SaveAsCommand, () =>
                documentManager.ActiveDocument = documentManager.Documents.First());
        }

        [TestMethod]
        public void SelectLanguageTest()
        {
            MainViewModel mainViewModel = container.GetExportedValue<MainViewModel>();
            Assert.IsNull(mainViewModel.NewLanguage);
            
            mainViewModel.GermanCommand.Execute(null);
            Assert.AreEqual("de-DE", mainViewModel.NewLanguage.Name);

            mainViewModel.EnglishCommand.Execute(null);
            Assert.AreEqual("en-US", mainViewModel.NewLanguage.Name);
        }
    }
}
