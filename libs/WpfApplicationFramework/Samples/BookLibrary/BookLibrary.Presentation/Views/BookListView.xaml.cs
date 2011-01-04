using System.ComponentModel.Composition;
using System.Waf.Applications;
using System.Windows.Controls;
using BookLibrary.Applications.ViewModels;
using BookLibrary.Applications.Views;
using BookLibrary.Domain;

namespace BookLibrary.Presentation.Views
{
    [Export(typeof(IBookListView))]
    public partial class BookListView : UserControl, IBookListView
    {
        public BookListView()
        {
            InitializeComponent();
        }


        private BookListViewModel ViewModel { get { return DataContext as BookListViewModel; } }


        private void ListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (Book book in e.RemovedItems)
            {
                ViewModel.SelectedBooks.Remove(book);
            }
            foreach (Book book in e.AddedItems)
            {
                ViewModel.SelectedBooks.Add(book);
            }
        }
    }
}
