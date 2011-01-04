using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Writer.Applications.Documents;
using System.ComponentModel;

namespace Writer.Applications.Test.Documents
{
    public class DocumentTypeMock : DocumentType
    {
        public DocumentTypeMock(string description, string fileExtension)
            : base(description, fileExtension)
        {
            CanSaveResult = true;
        }


        public bool CanSaveResult { get; set; }
        public DocumentOperation DocumentOperation { get; private set; }
        public IDocument Document { get; private set; }
        public string FileName { get; private set; }


        public override bool CanNew() { return true; }

        protected override IDocument NewCore()
        {
            DocumentOperation = DocumentOperation.New;
            return new DocumentMock(this);
        }

        public override bool CanOpen() { return true; }

        protected override IDocument OpenCore(string fileName)
        {
            DocumentOperation = DocumentOperation.Open;
            FileName = fileName;
            return new DocumentMock(this);
        }

        public override bool CanSave(IDocument document) { return CanSaveResult && document is DocumentMock; }

        protected override void SaveCore(IDocument document, string fileName)
        {
            DocumentOperation = DocumentOperation.Save;
            Document = document;
            FileName = fileName;
        }
    }

    public enum DocumentOperation
    {
        New,
        Open,
        Save
    }

    public class DocumentMock : Document
    {
        public DocumentMock(DocumentTypeMock documentType) : base(documentType)
        {
        }
    }
}
