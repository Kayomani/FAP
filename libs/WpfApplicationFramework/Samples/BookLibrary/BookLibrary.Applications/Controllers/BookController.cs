using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Waf.Applications;
using System.Waf.Applications.Services;
using BookLibrary.Applications.Properties;
using BookLibrary.Applications.Services;
using BookLibrary.Applications.ViewModels;
using BookLibrary.Applications.Views;
using BookLibrary.Domain;

namespace BookLibrary.Applications.Controllers
{
    [Export]
    public class BookController : Controller
    {
        private readonly CompositionContainer container;
        private readonly IEntityService entityService;
        private readonly ShellViewModel shellViewModel;
        private readonly BookViewModel bookViewModel;
        private readonly DelegateCommand addNewCommand;
        private readonly DelegateCommand removeCommand;
        private readonly DelegateCommand lendToCommand;
        private BookListViewModel bookListViewModel;
        

        [ImportingConstructor]
        public BookController(CompositionContainer container, IEntityService entityService, ShellViewModel shellViewModel, 
            BookViewModel bookViewModel)
        {
            this.container = container;
            this.entityService = entityService;
            this.shellViewModel = shellViewModel;
            this.bookViewModel = bookViewModel;
            this.addNewCommand = new DelegateCommand(AddNewBook, CanAddNewBook);
            this.removeCommand = new DelegateCommand(RemoveBook, CanRemoveBook);
            this.lendToCommand = new DelegateCommand(LendTo, CanLendTo);
        }


        public void Initialize()
        {
            bookViewModel.LendToCommand = lendToCommand;
            bookViewModel.PropertyChanged += BookViewModelPropertyChanged;

            IBookListView bookListView = container.GetExportedValue<IBookListView>();
            bookListViewModel = new BookListViewModel(bookListView, entityService.Books);
            bookListViewModel.AddNewCommand = addNewCommand;
            bookListViewModel.RemoveCommand = removeCommand;
            bookListViewModel.PropertyChanged += BookListViewModelPropertyChanged;

            shellViewModel.BookListView = bookListViewModel.View;
            shellViewModel.BookView = bookViewModel.View;

            bookListViewModel.SelectedBook = bookListViewModel.Books.FirstOrDefault();
        }

        private bool CanAddNewBook() { return bookViewModel.IsValid; }

        private void AddNewBook()
        {
            Book book = new Book();
            entityService.Books.Add(book);

            bookListViewModel.SelectedBook = book;
            bookViewModel.Focus();
        }

        private bool CanRemoveBook() { return bookListViewModel.SelectedBook != null; }

        private void RemoveBook()
        {
            foreach (Book book in bookListViewModel.SelectedBooks.ToArray())
            {
                entityService.Books.Remove(book);
            }
        }

        private bool CanLendTo() { return bookListViewModel.SelectedBook != null; }

        private void LendTo()
        {
            Book selectedBook = bookListViewModel.SelectedBook;

            LendToViewModel lendToViewModel = new LendToViewModel(container.GetExportedValue<ILendToView>(),
                selectedBook, entityService.Persons);
            if (lendToViewModel.ShowDialog(shellViewModel.View))
            {
                selectedBook.LendTo = lendToViewModel.SelectedPerson;
            }
        }

        private void UpdateCommands()
        {
            addNewCommand.RaiseCanExecuteChanged();
            removeCommand.RaiseCanExecuteChanged();
            lendToCommand.RaiseCanExecuteChanged();
        }

        private void BookListViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedBook")
            {
                bookViewModel.Book = bookListViewModel.SelectedBook;
                UpdateCommands();
            }
        }

        private void BookViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsValid")
            {
                UpdateCommands();
            }
        }
    }
}
