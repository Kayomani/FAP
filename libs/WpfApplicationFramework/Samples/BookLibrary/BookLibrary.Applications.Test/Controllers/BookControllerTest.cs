using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BookLibrary.Applications.Services;
using BookLibrary.Applications.Controllers;
using BookLibrary.Applications.ViewModels;
using BookLibrary.Applications.Views;
using BookLibrary.Domain;
using System.ComponentModel.Composition.Hosting;
using BookLibrary.Applications.Test.Views;
using System.Waf.UnitTesting;

namespace BookLibrary.Applications.Test.Controllers
{
    [TestClass]
    public class BookControllerTest
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
        public void BookControllerSelectionTest()
        {
            IEntityService entityService = container.GetExportedValue<IEntityService>();
            entityService.Books.Add(new Book() { Title = "The Fellowship of the Ring" });
            entityService.Books.Add(new Book() { Title = "The Two Towers" });
            
            BookController bookController = container.GetExportedValue<BookController>();
            bookController.Initialize();

            // Check that Initialize shows the BookListView and BookView
            ShellViewModel shellViewModel = container.GetExportedValue<ShellViewModel>();
            Assert.IsInstanceOfType(shellViewModel.BookListView, typeof(IBookListView));
            Assert.IsInstanceOfType(shellViewModel.BookView, typeof(IBookView));

            // Check that the first Book is selected
            IBookListView bookListView = container.GetExportedValue<IBookListView>();
            BookListViewModel bookListViewModel = bookListView.GetViewModel<BookListViewModel>();
            Assert.AreEqual(entityService.Books.First(), bookListViewModel.SelectedBook);

            // Change the selection
            BookViewModel bookViewModel = container.GetExportedValue<BookViewModel>();
            bookListViewModel.SelectedBook = entityService.Books.Last();
            Assert.AreEqual(entityService.Books.Last(), bookViewModel.Book);
        }

        [TestMethod]
        public void BookControllerAddAndRemoveTest()
        {
            Book fellowship = new Book() { Title = "The Fellowship of the Ring" };
            Book twoTowers = new Book() { Title = "The Two Towers" };

            IEntityService entityService = container.GetExportedValue<IEntityService>();
            entityService.Books.Add(fellowship);
            entityService.Books.Add(twoTowers);

            BookController bookController = container.GetExportedValue<BookController>();
            bookController.Initialize();

            BookListViewMock bookListView = container.GetExportedValue<BookListViewMock>();
            BookListViewModel bookListViewModel = bookListView.GetViewModel<BookListViewModel>();
            BookViewMock bookView = container.GetExportedValue<BookViewMock>();
            BookViewModel bookViewModel = bookView.GetViewModel<BookViewModel>();

            // Add a new Book
            Assert.AreEqual(2, entityService.Books.Count);
            Assert.IsTrue(bookListViewModel.AddNewCommand.CanExecute(null));
            bookListViewModel.AddNewCommand.Execute(null);
            Assert.AreEqual(3, entityService.Books.Count);

            // Check that the new Book is selected and the first control gets the focus
            Assert.AreEqual(entityService.Books.Last(), bookViewModel.Book);
            Assert.IsTrue(bookView.FirstControlHasFocus);

            // Simulate an invalid UI input state => the user can't add more books
            AssertHelper.CanExecuteChangedEvent(bookListViewModel.AddNewCommand, () =>
                bookViewModel.IsValid = false);
            Assert.IsFalse(bookListViewModel.AddNewCommand.CanExecute(null));

            // Remove the last two Books at once
            bookListViewModel.SelectedBooks.Add(twoTowers);
            bookListViewModel.SelectedBooks.Add(entityService.Books.Last());
            Assert.IsTrue(bookListViewModel.RemoveCommand.CanExecute(null));
            bookListViewModel.RemoveCommand.Execute(null);
            Assert.IsTrue(entityService.Books.SequenceEqual(new Book[] { fellowship }));

            // Deselect all Books => the Remove command must be deactivated
            AssertHelper.CanExecuteChangedEvent(bookListViewModel.RemoveCommand, () =>
            {
                bookListViewModel.SelectedBooks.Clear();
                bookListViewModel.SelectedBook = null;
            });
            Assert.IsFalse(bookListViewModel.RemoveCommand.CanExecute(null));
        }

        [TestMethod]
        public void BookControllerLendToTest()
        {
            Book fellowship = new Book() { Title = "The Fellowship of the Ring" };
            Book twoTowers = new Book() { Title = "The Two Towers" };
            Person harry = new Person() { Firstname = "Harry" };
            Person ron = new Person() { Firstname = "Ron" };

            IEntityService entityService = container.GetExportedValue<IEntityService>();
            entityService.Books.Add(fellowship);
            entityService.Books.Add(twoTowers);
            entityService.Persons.Add(harry);
            entityService.Persons.Add(ron);

            BookController bookController = container.GetExportedValue<BookController>();
            bookController.Initialize();

            BookListViewMock bookListView = container.GetExportedValue<BookListViewMock>();
            BookListViewModel bookListViewModel = bookListView.GetViewModel<BookListViewModel>();
            BookViewMock bookView = container.GetExportedValue<BookViewMock>();
            BookViewModel bookViewModel = bookView.GetViewModel<BookViewModel>();

            // Check that the LendTo Button is enabled
            Assert.IsNull(fellowship.LendTo);
            Assert.AreEqual(fellowship, bookViewModel.Book);
            Assert.IsTrue(bookViewModel.LendToCommand.CanExecute(null));
            
            // Open the LendTo dialog
            LendToViewMock.ShowDialogAction = (view) =>
            {
                Assert.AreEqual(container.GetExportedValue<IShellView>(), view.Owner);
                Assert.IsTrue(view.IsVisible);
                LendToViewModel viewModel = (LendToViewModel)view.DataContext;
                Assert.AreEqual(fellowship, viewModel.Book);
                Assert.AreEqual(entityService.Persons, viewModel.Persons);

                // Lend the book to Ron
                viewModel.SelectedPerson = ron;
                viewModel.OkCommand.Execute(null);
            };
            bookViewModel.LendToCommand.Execute(null);
            Assert.AreEqual(ron, fellowship.LendTo);

            // Check that the LendTo Button is disabled when no book is selected anymore.
            AssertHelper.CanExecuteChangedEvent(bookViewModel.LendToCommand, () =>
                bookListViewModel.SelectedBook = null);
            Assert.IsNull(bookViewModel.Book);
            Assert.IsFalse(bookViewModel.LendToCommand.CanExecute(null));
        }
    }
}
