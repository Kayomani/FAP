using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BookLibrary.Applications.ViewModels;
using BookLibrary.Applications.Test.Views;
using System.Waf.UnitTesting;
using BookLibrary.Domain;
using System.Waf.Applications;

namespace BookLibrary.Applications.Test.ViewModels
{
    [TestClass]
    public class BookListViewModelTest
    {
        [TestMethod]
        public void BookListViewModelBooksTest()
        {
            List<Book> books = new List<Book>()
            {
                new Book() { Title = "The Fellowship of the Ring" },
                new Book() { Title = "The Two Towers" }
            };
            
            BookListViewMock bookListView = new BookListViewMock();
            
            AssertHelper.ExpectedException<ArgumentNullException>(() => new BookListViewModel(bookListView, null));
            BookListViewModel bookListViewModel = new BookListViewModel(bookListView, books);

            Assert.AreEqual(books, bookListViewModel.Books);
            Assert.IsNull(bookListViewModel.SelectedBook);
            Assert.IsFalse(bookListViewModel.SelectedBooks.Any());

            // Select the first book
            AssertHelper.PropertyChangedEvent(bookListViewModel, x => x.SelectedBook,
                () => bookListViewModel.SelectedBook = books.First());
            Assert.AreEqual(books.First(), bookListViewModel.SelectedBook);

            bookListViewModel.SelectedBooks.Add(books.First());
            Assert.IsTrue(bookListViewModel.SelectedBooks.SequenceEqual(new Book[] { books.First() }));

            // Select both books
            bookListViewModel.SelectedBooks.Add(books.Last());
            Assert.IsTrue(bookListViewModel.SelectedBooks.SequenceEqual(books));
        }

        [TestMethod]
        public void BookListViewModelCommandsTest()
        {
            List<Book> books = new List<Book>()
            {
                new Book() { Title = "The Fellowship of the Ring" },
                new Book() { Title = "The Two Towers" }
            };

            BookListViewMock bookListView = new BookListViewMock();
            BookListViewModel bookListViewModel = new BookListViewModel(bookListView, books);

            DelegateCommand mockCommand = new DelegateCommand(() => { });
            AssertHelper.PropertyChangedEvent(bookListViewModel, x => x.AddNewCommand, () =>
                bookListViewModel.AddNewCommand = mockCommand);
            Assert.AreEqual(mockCommand, bookListViewModel.AddNewCommand);

            AssertHelper.PropertyChangedEvent(bookListViewModel, x => x.RemoveCommand, () =>
                bookListViewModel.RemoveCommand = mockCommand);
            Assert.AreEqual(mockCommand, bookListViewModel.RemoveCommand);
        }
    }
}
