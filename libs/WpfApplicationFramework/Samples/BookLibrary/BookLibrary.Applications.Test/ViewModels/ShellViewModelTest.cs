using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BookLibrary.Applications.Test.Views;
using BookLibrary.Applications.ViewModels;
using BookLibrary.Applications.Test.Services;
using System.Waf.Applications;
using System.Waf.UnitTesting;

namespace BookLibrary.Applications.Test.ViewModels
{
    [TestClass]
    public class ShellViewModelTest
    {
        [TestMethod]
        public void ShellViewModelBasicTest()
        {
            ShellViewMock shellView = new ShellViewMock();
            MessageServiceMock messageService = new MessageServiceMock();
            ShellViewModel shellViewModel = new ShellViewModel(shellView, messageService);

            // The title isn't available in the unit test environment.
            Assert.AreEqual("", ShellViewModel.Title);

            // Show the ShellView
            shellViewModel.Show();
            Assert.IsTrue(shellView.IsVisible);

            // Show the about message box
            messageService.Reset();
            shellViewModel.AboutCommand.Execute(null);
            Assert.IsFalse(string.IsNullOrEmpty(messageService.Message));
            Assert.AreEqual(MessageType.Message, messageService.MessageType);

            // Close the ShellView
            bool closingEventRaised = false;
            shellViewModel.Closing += (sender, e) =>
            {
                closingEventRaised = true;
            };
            shellViewModel.Close();
            Assert.IsFalse(shellView.IsVisible);
            Assert.IsTrue(closingEventRaised);
        }

        [TestMethod]
        public void ShellViewModelPropertiesTest()
        {
            ShellViewMock shellView = new ShellViewMock();
            MessageServiceMock messageService = new MessageServiceMock();
            ShellViewModel shellViewModel = new ShellViewModel(shellView, messageService);

            DelegateCommand mockCommand = new DelegateCommand(() => {});
            AssertHelper.PropertyChangedEvent(shellViewModel, x => x.SaveCommand, () => 
                shellViewModel.SaveCommand = mockCommand);
            Assert.AreEqual(mockCommand, shellViewModel.SaveCommand);
            
            AssertHelper.PropertyChangedEvent(shellViewModel, x => x.ExitCommand, () =>
                shellViewModel.ExitCommand = mockCommand);
            Assert.AreEqual(mockCommand, shellViewModel.ExitCommand);

            Assert.IsTrue(shellViewModel.IsValid);
            AssertHelper.PropertyChangedEvent(shellViewModel, x => x.IsValid, () =>
                shellViewModel.IsValid = false);
            Assert.IsFalse(shellViewModel.IsValid);

            object mockView = new object();
            AssertHelper.PropertyChangedEvent(shellViewModel, x => x.BookListView, () =>
                shellViewModel.BookListView = mockView);
            Assert.AreEqual(mockView, shellViewModel.BookListView);

            AssertHelper.PropertyChangedEvent(shellViewModel, x => x.BookView, () =>
                shellViewModel.BookView = mockView);
            Assert.AreEqual(mockView, shellViewModel.BookView);

            AssertHelper.PropertyChangedEvent(shellViewModel, x => x.PersonListView, () =>
                shellViewModel.PersonListView = mockView);
            Assert.AreEqual(mockView, shellViewModel.PersonListView);

            AssertHelper.PropertyChangedEvent(shellViewModel, x => x.PersonView, () =>
                shellViewModel.PersonView = mockView);
            Assert.AreEqual(mockView, shellViewModel.PersonView);
        }
    }
}
