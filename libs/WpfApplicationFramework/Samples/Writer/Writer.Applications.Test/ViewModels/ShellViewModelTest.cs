using System.ComponentModel;
using System.ComponentModel.Composition.Hosting;
using System.Waf.Applications;
using System.Waf.Applications.Services;
using System.Waf.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Writer.Applications.Services;
using Writer.Applications.Test.Controllers;
using Writer.Applications.Test.Services;
using Writer.Applications.Test.Views;
using Writer.Applications.ViewModels;
using Writer.Applications.Views;
using System;

namespace Writer.Applications.Test.ViewModels
{
    [TestClass]
    public class ShellViewModelTest
    {
        private TestController controller;
        private CompositionContainer container;


        [TestInitialize]
        public void TestInitialize()
        {
            controller = new TestController();
            container = controller.Container;
        }


        [TestMethod]
        public void ShowAndClose()
        {
            MessageServiceMock messageService = (MessageServiceMock)container.GetExportedValue<IMessageService>();
            IZoomService zoomService = container.GetExportedValue<IZoomService>();
            ShellViewMock shellView = (ShellViewMock)container.GetExportedValue<IShellView>();
            ShellViewModel shellViewModel = container.GetExportedValue<ShellViewModel>();
            MainViewModel mainViewModel = container.GetExportedValue<MainViewModel>();

            // Show the ShellView
            Assert.IsFalse(shellView.IsVisible);
            shellViewModel.Show();
            Assert.IsTrue(shellView.IsVisible);

            // In this case it tries to get the title of the unit test framework which is ""
            Assert.AreEqual("", ShellViewModel.Title);
            Assert.AreEqual(zoomService, shellViewModel.ZoomService);

            // Show the About Dialog
            Assert.IsNull(messageService.Message);
            mainViewModel.AboutCommand.Execute(null);
            Assert.AreEqual(MessageType.Message, messageService.MessageType);
            Assert.IsNotNull(messageService.Message);

            // Try to close the ShellView but cancel this operation through the closing event
            bool cancelClosing = true;
            shellViewModel.Closing += (sender, e) =>
            {
                e.Cancel = cancelClosing;
            };
            shellViewModel.Close();
            Assert.IsTrue(shellView.IsVisible);

            // Close the ShellView via the ExitCommand
            cancelClosing = false;
            AssertHelper.PropertyChangedEvent(mainViewModel, x => x.ExitCommand, () =>
                mainViewModel.ExitCommand = new DelegateCommand(() => shellViewModel.Close()));
            mainViewModel.ExitCommand.Execute(null);
            Assert.IsFalse(shellView.IsVisible);
        }
    }
}
