using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Waf.Applications;
using System.Windows.Input;
using BookLibrary.Applications.Views;
using BookLibrary.Domain;
using System;

namespace BookLibrary.Applications.ViewModels
{
    public class BookListViewModel : ViewModel<IBookListView>
    {
        private readonly IEnumerable<Book> books;
        private readonly ObservableCollection<Book> selectedBooks;
        private Book selectedBook;
        private ICommand addNewCommand;
        private ICommand removeCommand;

        
        public BookListViewModel(IBookListView view, IEnumerable<Book> books)
            : base(view)
        {
            if (books == null) { throw new ArgumentNullException("books"); }
            
            this.books = books;
            this.selectedBooks = new ObservableCollection<Book>();
        }


        public IEnumerable<Book> Books { get { return books; } }

        public ObservableCollection<Book> SelectedBooks
        {
            get { return selectedBooks; }
        }

        public Book SelectedBook
        {
            get { return selectedBook; }
            set
            {
                if (selectedBook != value)
                {
                    selectedBook = value;
                    RaisePropertyChanged("SelectedBook");
                }
            }
        }

        public ICommand AddNewCommand
        {
            get { return addNewCommand; }
            set
            {
                if (addNewCommand != value)
                {
                    addNewCommand = value;
                    RaisePropertyChanged("AddNewCommand");
                }
            }
        }

        public ICommand RemoveCommand
        {
            get { return removeCommand; }
            set
            {
                if (removeCommand != value)
                {
                    removeCommand = value;
                    RaisePropertyChanged("RemoveCommand");
                }
            }
        }
    }
}
