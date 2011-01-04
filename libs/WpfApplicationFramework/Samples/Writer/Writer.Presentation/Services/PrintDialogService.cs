using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Writer.Applications.Services;
using System.Windows.Documents;
using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Writer.Presentation.Services
{
    [Export(typeof(IPrintDialogService))]
    internal class PrintDialogService : IPrintDialogService
    {
        private readonly PrintDialog printDialog = new PrintDialog();


        public bool ShowDialog()
        {
            return printDialog.ShowDialog() == true;
        }

        public void PrintDocument(DocumentPaginator documentPaginator, string description)
        {
            printDialog.PrintDocument(documentPaginator, description);
        }
    }
}
