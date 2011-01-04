using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Writer.Applications.Services;
using System.ComponentModel.Composition;
using System.Windows.Documents;

namespace Writer.Applications.Test.Services
{
    [Export(typeof(IPrintDialogService))]
    public class PrintDialogServiceMock : IPrintDialogService
    {
        public bool ShowDialogResult { get; set; }
        public DocumentPaginator DocumentPaginator { get; private set; }
        public string Description { get; private set; }

        
        public bool ShowDialog()
        {
            DocumentPaginator = null;
            Description = null;
            return ShowDialogResult;
        }

        public void PrintDocument(DocumentPaginator documentPaginator, string description)
        {
            DocumentPaginator = documentPaginator;
            Description = description;
        }
    }
}
