using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Writer.Applications.Documents;
using Writer.Applications.Test.Services;
using System.Waf.UnitTesting;
using System.Waf.Applications.Services;
using System.IO;
using System.ComponentModel;

namespace Writer.Applications.Test.Documents
{
    [TestClass]
    public class DocumentManagerTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            AssertHelper.ExpectedException<ArgumentNullException>(() => new DocumentManager(null));
        }

        [TestMethod]
        public void RegisterDocumentTypesTest()
        {
            FileDialogServiceMock fileDialogService = new FileDialogServiceMock();
            DocumentManager documentManager = new DocumentManager(fileDialogService);
            
            DocumentTypeMock documentType = new DocumentTypeMock("Mock Document", ".mock");
            documentManager.Register(documentType);
            Assert.IsTrue(documentManager.DocumentTypes.SequenceEqual(new IDocumentType[] { documentType }));

            documentManager.New(documentType);
            AssertHelper.ExpectedException<InvalidOperationException>(() => documentManager.Deregister(documentType));

            documentManager.CloseAll();
            documentManager.Deregister(documentType);
            Assert.IsFalse(documentManager.DocumentTypes.Any());

            AssertHelper.ExpectedException<ArgumentNullException>(() => documentManager.Register(null));
            AssertHelper.ExpectedException<ArgumentNullException>(() => documentManager.Deregister(null));
        }

        [TestMethod]
        public void NewAndActiveDocumentTest()
        {
            FileDialogServiceMock fileDialogService = new FileDialogServiceMock();
            DocumentManager documentManager = new DocumentManager(fileDialogService);

            DocumentTypeMock documentType = new DocumentTypeMock("Mock Document", ".mock");
            documentManager.Register(documentType);

            Assert.IsFalse(documentManager.Documents.Any());
            Assert.IsNull(documentManager.ActiveDocument);

            IDocument document = documentManager.New(documentType);
            Assert.IsTrue(documentManager.Documents.SequenceEqual(new IDocument[] { document }));
            Assert.AreEqual(document, documentManager.ActiveDocument);

            AssertHelper.ExpectedException<ArgumentNullException>(() => documentManager.New(null));
            AssertHelper.ExpectedException<ArgumentException>(() => documentManager.New(new DocumentTypeMock("Dummy", ".dmy"))); 

            AssertHelper.PropertyChangedEvent(documentManager, x => x.ActiveDocument, () => documentManager.ActiveDocument = null);
            Assert.AreEqual(null, documentManager.ActiveDocument);

            AssertHelper.ExpectedException<ArgumentException>(() => documentManager.ActiveDocument = documentType.New());
        }

        [TestMethod]
        public void OpenDocumentTest()
        {
            FileDialogServiceMock fileDialogService = new FileDialogServiceMock();
            DocumentManager documentManager = new DocumentManager(fileDialogService);

            AssertHelper.ExpectedException<InvalidOperationException>(() => documentManager.Open());

            DocumentTypeMock documentType = new DocumentTypeMock("Mock Document", ".mock");
            documentManager.Register(documentType);

            Assert.IsFalse(documentManager.Documents.Any());
            Assert.IsNull(documentManager.ActiveDocument);

            fileDialogService.Result = new FileDialogResult("Document1.mock", new FileType("Mock Document", ".mock"));
            IDocument document = documentManager.Open();
            Assert.AreEqual(FileDialogType.OpenFileDialog, fileDialogService.FileDialogType);
            Assert.AreEqual("Mock Document", fileDialogService.FileTypes.Single().Description);
            Assert.AreEqual(".mock", fileDialogService.FileTypes.Single().FileExtension);
            Assert.AreEqual("Document1.mock", document.FileName);
            
            Assert.IsTrue(documentManager.Documents.SequenceEqual(new IDocument[] { document }));
            Assert.AreEqual(document, documentManager.ActiveDocument);

            // Open the same file again -> It's not opened again, just activated.

            document = documentManager.Open();
            Assert.IsTrue(documentManager.Documents.SequenceEqual(new IDocument[] { document }));
            Assert.AreEqual(document, documentManager.ActiveDocument);

            // Now the user cancels the OpenFileDialog box

            fileDialogService.Result = new FileDialogResult();
            IDocument document2 = documentManager.Open();
            Assert.IsNull(document2);

            Assert.IsTrue(documentManager.Documents.SequenceEqual(new IDocument[] { document }));
            Assert.AreEqual(document, documentManager.ActiveDocument);
        }

        [TestMethod]
        public void SaveDocumentTest()
        {
            FileDialogServiceMock fileDialogService = new FileDialogServiceMock();
            DocumentManager documentManager = new DocumentManager(fileDialogService);
            DocumentTypeMock documentType = new DocumentTypeMock("Mock Document", ".mock");

            AssertHelper.ExpectedException<ArgumentNullException>(() => documentManager.Save(null));
            AssertHelper.ExpectedException<ArgumentNullException>(() => documentManager.SaveAs(null));
            AssertHelper.ExpectedException<ArgumentException>(() => documentManager.Save(documentType.New()));
            AssertHelper.ExpectedException<ArgumentException>(() => documentManager.SaveAs(documentType.New()));
                
            documentManager.Register(documentType);
            IDocument document = documentManager.New(documentType);
            document.FileName = "Document.mock";

            fileDialogService.Result = new FileDialogResult("Document1.mock", new FileType("Mock Document", ".mock"));
            documentManager.Save(documentManager.ActiveDocument);
            Assert.AreEqual(FileDialogType.SaveFileDialog, fileDialogService.FileDialogType);
            Assert.AreEqual("Mock Document", fileDialogService.FileTypes.Single().Description);
            Assert.AreEqual(".mock", fileDialogService.FileTypes.Single().FileExtension);
            Assert.AreEqual("Mock Document", fileDialogService.DefaultFileType.Description);
            Assert.AreEqual(".mock", fileDialogService.DefaultFileType.FileExtension);
            Assert.AreEqual("Document", fileDialogService.DefaultFileName);

            // Change the CanSave to return false so that no documentType is able to save the document anymore

            documentType.CanSaveResult = false;
            AssertHelper.ExpectedException<InvalidOperationException>(() => documentManager.Save(documentManager.ActiveDocument));
        }

        [TestMethod]
        public void SaveDocumentWhenFileExistsTest()
        {
            // Get the absolute file path
            string fileName = Path.GetFullPath("SaveWhenFileExistsTest.mock");
            
            FileDialogServiceMock fileDialogService = new FileDialogServiceMock();
            DocumentManager documentManager = new DocumentManager(fileDialogService);
            DocumentTypeMock documentType = new DocumentTypeMock("Mock Document", ".mock");
            documentManager.Register(documentType);

            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.WriteLine("Hello World");
            }

            IDocument document = documentManager.New(documentType);
            // We set the absoulte file path to simulate that we already saved the document
            document.FileName = fileName;
            
            documentManager.Save(document);
            Assert.AreEqual(DocumentOperation.Save, documentType.DocumentOperation);
            Assert.AreEqual(document, documentType.Document);
            Assert.AreEqual(fileName, documentType.FileName);

            fileDialogService.Result = new FileDialogResult(fileName, new FileType("Mock Document", ".mock"));
            documentManager.SaveAs(document);
            Assert.AreEqual("Mock Document", fileDialogService.DefaultFileType.Description);
            Assert.AreEqual(".mock", fileDialogService.DefaultFileType.FileExtension);
        }

        [TestMethod]
        public void CloseDocumentTest()
        {
            FileDialogServiceMock fileDialogService = new FileDialogServiceMock();
            DocumentManager documentManager = new DocumentManager(fileDialogService);
            DocumentTypeMock documentType = new DocumentTypeMock("Mock Document", ".mock");
            documentManager.Register(documentType);

            documentManager.New(documentType);
            documentManager.Close(documentManager.ActiveDocument);
            Assert.IsFalse(documentManager.Documents.Any());

            IDocument document = documentManager.New(documentType);
            
            bool cancelResult = true;
            IEnumerable<IDocument> documentsToClose = null;
            documentManager.DocumentsClosing += (sender, e) =>
            {
                documentsToClose = e.Documents;
                e.Cancel = cancelResult;
            };

            Assert.AreEqual(document, documentManager.ActiveDocument);
            documentManager.Close(documentManager.ActiveDocument);
            Assert.AreEqual(document, documentManager.ActiveDocument);
            Assert.IsTrue(documentsToClose.SequenceEqual(new IDocument[] { document }));

            Assert.AreEqual(document, documentManager.ActiveDocument);
            documentManager.CloseAll();
            Assert.AreEqual(document, documentManager.ActiveDocument);
            Assert.IsTrue(documentsToClose.SequenceEqual(new IDocument[] { document }));

            cancelResult = false;
            documentManager.CloseAll();
            Assert.IsNull(documentManager.ActiveDocument);
            Assert.IsFalse(documentManager.Documents.Any());

            AssertHelper.ExpectedException<ArgumentNullException>(() => documentManager.Close(null));
            DocumentTypeMock documentType2 = new DocumentTypeMock("Unmanaged Document", ".udoc");
            AssertHelper.ExpectedException<ArgumentException>(() => documentManager.Close(documentType2.New()));
        }
    }
}
