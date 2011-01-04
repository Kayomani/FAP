using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BookLibrary.Applications.Test.Views;
using BookLibrary.Applications.ViewModels;
using BookLibrary.Domain;
using System.Waf.UnitTesting;
using System.Waf.Applications;

namespace BookLibrary.Applications.Test.ViewModels
{
    [TestClass]
    public class BookViewModelTest
    {
        [TestMethod]
        public void BookViewModelBookTest()
        {
            BookViewMock bookView = new BookViewMock();
            BookViewModel bookViewModel = new BookViewModel(bookView);

            Assert.IsFalse(bookViewModel.IsEnabled);

            Book book = new Book();
            AssertHelper.PropertyChangedEvent(bookViewModel, x => x.Book, () => bookViewModel.Book = book);
            Assert.AreEqual(book, bookViewModel.Book);
            Assert.IsTrue(bookViewModel.IsEnabled);

            AssertHelper.PropertyChangedEvent(bookViewModel, x => x.IsEnabled, () => bookViewModel.Book = null);
            Assert.IsNull(bookViewModel.Book);
            Assert.IsFalse(bookViewModel.IsEnabled);
        }

        [TestMethod]
        public void BookViewModelIsValidTest()
        {
            BookViewMock bookView = new BookViewMock();
            BookViewModel bookViewModel = new BookViewModel(bookView);

            Assert.IsTrue(bookViewModel.IsValid);

            AssertHelper.PropertyChangedEvent(bookViewModel, x => x.IsValid, () => bookViewModel.IsValid = false);
            Assert.IsFalse(bookViewModel.IsValid);

            Assert.IsFalse(bookView.FirstControlHasFocus);
            bookViewModel.Focus();
            Assert.IsTrue(bookView.FirstControlHasFocus);
        }

        [TestMethod]
        public void BookViewModelCommandsTest()
        {
            BookViewMock bookView = new BookViewMock();
            BookViewModel bookViewModel = new BookViewModel(bookView);

            DelegateCommand mockCommand = new DelegateCommand(() => { });
            AssertHelper.PropertyChangedEvent(bookViewModel, x => x.LendToCommand, () =>
                bookViewModel.LendToCommand = mockCommand);
            Assert.AreEqual(mockCommand, bookViewModel.LendToCommand);
        }
    }
}
