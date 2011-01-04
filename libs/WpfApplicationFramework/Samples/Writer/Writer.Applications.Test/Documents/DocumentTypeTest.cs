using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Writer.Applications.Documents;
using System.Waf.UnitTesting;
using System.ComponentModel;

namespace Writer.Applications.Test.Documents
{
    [TestClass]
    public class DocumentTypeTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            AssertHelper.ExpectedException<ArgumentException>(() => new DocumentTypeBaseMock("", ".rtf"));
            AssertHelper.ExpectedException<ArgumentException>(() => new DocumentTypeBaseMock("RichText Documents", null));
            AssertHelper.ExpectedException<ArgumentException>(() => new DocumentTypeBaseMock("RichText Documents", "rtf"));

            AssertHelper.ExpectedException<ArgumentNullException>(() => new DocumentBaseMock(null));
        }

        [TestMethod]
        public void CheckBaseImplementation()
        {
            DocumentTypeBaseMock documentType = new DocumentTypeBaseMock("RichText Documents", ".rtf");
            Assert.IsFalse(documentType.CanNew());
            Assert.IsFalse(documentType.CanOpen());
            Assert.IsFalse(documentType.CanSave(null));

            DocumentTypeBaseMock documentType2 = new DocumentTypeBaseMock("XPS Documents", ".xps");
            AssertHelper.ExpectedException<NotSupportedException>(() => documentType.New());
            AssertHelper.ExpectedException<NotSupportedException>(() => documentType.Open("TestDocument1.rtf"));
            AssertHelper.ExpectedException<NotSupportedException>(() => 
                documentType.Save(new DocumentBaseMock(documentType2), "TestDocument1.rtf"));

            AssertHelper.ExpectedException<ArgumentException>(() => documentType.Open(""));
            AssertHelper.ExpectedException<ArgumentException>(() => documentType.Save(new DocumentBaseMock(documentType2), ""));
            AssertHelper.ExpectedException<ArgumentNullException>(() => documentType.Save(null, "TestDocument1.rtf"));

            AssertHelper.ExpectedException<NotImplementedException>(() => documentType.CallNewCore());
            AssertHelper.ExpectedException<NotImplementedException>(() => documentType.CallOpenCore(null));
            AssertHelper.ExpectedException<NotImplementedException>(() => documentType.CallSaveCore(null, null));
        }


        private class DocumentTypeBaseMock : DocumentType
        {
            public DocumentTypeBaseMock(string description, string fileExtension)
                : base(description, fileExtension)
            {
            }


            public IDocument CallNewCore() { return NewCore(); }

            public IDocument CallOpenCore(string fileName) { return OpenCore(fileName); }

            public void CallSaveCore(IDocument document, string fileName)
            {
                SaveCore(document, fileName);
            }
        }

        private class DocumentBaseMock : Document
        {
            public DocumentBaseMock(DocumentTypeBaseMock documentType)
                : base(documentType)
            {
            }
        }
    }
}
