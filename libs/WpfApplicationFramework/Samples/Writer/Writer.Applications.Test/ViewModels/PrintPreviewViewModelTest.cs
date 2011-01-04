using System;
using System.Windows.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Writer.Applications.Test.Views;
using Writer.Applications.ViewModels;

namespace Writer.Applications.Test.ViewModels
{
    [TestClass]
    public class PrintPreviewViewModelTest
    {
        [TestMethod]
        public void ExecutePrintCommand()
        {
            PrintPreviewViewMock printPreviewView = new PrintPreviewViewMock();
            DocumentPaginatorSourceMock document = new DocumentPaginatorSourceMock();

            PrintPreviewViewModel printPreviewViewModel = new PrintPreviewViewModel(printPreviewView, document);
            Assert.AreEqual(document, printPreviewViewModel.Document);

            printPreviewViewModel.PrintCommand.Execute(null);
            Assert.IsTrue(printPreviewView.PrintCalled);
        }


        private class DocumentPaginatorSourceMock : IDocumentPaginatorSource
        {
            public DocumentPaginator DocumentPaginator
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}
