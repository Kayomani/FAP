using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Writer.Applications.ViewModels;
using Writer.Applications.Test.Views;
using Writer.Applications.Services;
using Writer.Applications.Test.Documents;
using Writer.Applications.Documents;

namespace Writer.Applications.Test.ViewModels
{
    [TestClass]
    public class SaveChangesViewModelTest
    {
        [TestMethod]
        public void SaveChangesViewModelCloseTest()
        {
            DocumentTypeMock documentType = new DocumentTypeMock("Mock Document", ".mock");
            IEnumerable<IDocument> documents = new IDocument[] 
            {
                documentType.New(),
                documentType.New(),
                documentType.New()
            };

            SaveChangesViewMock view = new SaveChangesViewMock();
            ZoomService zoomService = new ZoomService();

            SaveChangesViewModel viewModel = new SaveChangesViewModel(view, zoomService, documents);
            
            // In this case it tries to get the title of the unit test framework which is ""
            Assert.AreEqual("", SaveChangesViewModel.Title);
            Assert.AreEqual(zoomService, viewModel.ZoomService);
            Assert.AreEqual(documents, viewModel.Documents);
            Assert.AreEqual(ViewResult.Cancel, viewModel.ViewResult);

            object owner = new object();
            Assert.IsFalse(view.IsVisible);
            viewModel.ShowDialog(owner);
            Assert.IsTrue(view.IsVisible);
            Assert.AreEqual(owner, view.Owner);
            
            viewModel.YesCommand.Execute(null);
            Assert.IsFalse(view.IsVisible);
            Assert.AreEqual(ViewResult.Yes, viewModel.ViewResult);

            viewModel.ShowDialog(owner);
            viewModel.NoCommand.Execute(null);
            Assert.IsFalse(view.IsVisible);
            Assert.AreEqual(ViewResult.No, viewModel.ViewResult);
        }
    }
}
