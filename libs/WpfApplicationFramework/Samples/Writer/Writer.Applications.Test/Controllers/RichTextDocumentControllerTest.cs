using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Waf.Applications;
using System.Waf.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Writer.Applications.Controllers;
using Writer.Applications.Documents;
using Writer.Applications.ViewModels;
using Writer.Applications.Views;

namespace Writer.Applications.Test.Controllers
{
    [TestClass]
    public class RichTextDocumentControllerTest
    {
        private TestController controller;
        private CompositionContainer container;
        

        [TestInitialize]
        public void TestInitialize()
        {
            controller = new TestController();
            container = controller.Container;
            controller.InitializeRichTextDocumentSupport();
        }


        [TestMethod]
        public void RichTextDocumentControllerConstructorTest()
        {
            AssertHelper.ExpectedException<ArgumentNullException>(() =>
                new RichTextDocumentController(container, null, container.GetExportedValue<MainViewModel>()));
        }

        [TestMethod]
        public void AddAndRemoveDocumentViewTest()
        {
            MainViewModel mainViewModel = container.GetExportedValue<MainViewModel>();
            IDocumentManager documentManager = container.GetExportedValue<IDocumentManager>();

            Assert.IsFalse(documentManager.Documents.Any());
            Assert.IsFalse(mainViewModel.DocumentViews.Any());

            // Create new documents

            IDocument document = documentManager.New(documentManager.DocumentTypes.First());

            IRichTextView richTextView = mainViewModel.DocumentViews.OfType<IRichTextView>().Single();
            RichTextViewModel richTextViewModel = richTextView.GetViewModel<RichTextViewModel>();
            Assert.AreEqual(document, richTextViewModel.Document);

            document = documentManager.New(documentManager.DocumentTypes.First());

            Assert.AreEqual(2, mainViewModel.DocumentViews.Count);
            richTextView = mainViewModel.DocumentViews.OfType<IRichTextView>().Last();
            richTextViewModel = richTextView.GetViewModel<RichTextViewModel>();
            Assert.AreEqual(document, richTextViewModel.Document);

            // Test ActiveDocument <-> ActiveDocumentView synchronisation

            Assert.AreEqual(documentManager.Documents.Last(), documentManager.ActiveDocument);

            documentManager.ActiveDocument = documentManager.Documents.First();
            Assert.AreEqual(mainViewModel.DocumentViews.First(), mainViewModel.ActiveDocumentView);

            mainViewModel.ActiveDocumentView = mainViewModel.DocumentViews.Last();
            Assert.AreEqual(documentManager.Documents.Last(), documentManager.ActiveDocument);

            // Close all documents

            documentManager.CloseAll();

            Assert.IsFalse(documentManager.Documents.Any());
            Assert.IsFalse(mainViewModel.DocumentViews.Any());
        }

        [TestMethod]
        public void IllegalDocumentCollectionChangeTest()
        {
            IDocumentManager documentManager = container.GetExportedValue<IDocumentManager>();
            
            documentManager.New(documentManager.DocumentTypes.First());

            // We have to use reflection to get the private documents collection field
            FieldInfo documentsInfo = typeof(DocumentManager).GetField("documents", BindingFlags.Instance | BindingFlags.NonPublic);
            ObservableCollection<IDocument> documents = (ObservableCollection<IDocument>)documentsInfo.GetValue(documentManager);

            // Now we call a method that is not supported by the DocumentController base class
            AssertHelper.ExpectedException<NotSupportedException>(() => documents.Clear());
        }
    }
}
