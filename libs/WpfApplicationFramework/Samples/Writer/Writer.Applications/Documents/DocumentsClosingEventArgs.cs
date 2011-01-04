using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Writer.Applications.Documents
{
    public class DocumentsClosingEventArgs : CancelEventArgs
    {
        private readonly IEnumerable<IDocument> documents;

        
        public DocumentsClosingEventArgs(IEnumerable<IDocument> documents)
        {
            this.documents = documents;
        }


        public IEnumerable<IDocument> Documents { get { return documents; } }
    }
}
