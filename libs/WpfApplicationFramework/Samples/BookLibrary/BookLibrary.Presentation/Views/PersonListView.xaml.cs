using System.ComponentModel.Composition;
using System.Waf.Applications;
using System.Windows.Controls;
using BookLibrary.Applications.ViewModels;
using BookLibrary.Applications.Views;
using BookLibrary.Domain;

namespace BookLibrary.Presentation.Views
{
    [Export(typeof(IPersonListView))]
    public partial class PersonListView : UserControl, IPersonListView
    {
        public PersonListView()
        {
            InitializeComponent();
        }


        private PersonListViewModel ViewModel { get { return this.GetViewModel<PersonListViewModel>(); } }


        private void ListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (Person person in e.RemovedItems)
            {
                ViewModel.SelectedPersons.Remove(person);
            }
            foreach (Person person in e.AddedItems)
            {
                ViewModel.SelectedPersons.Add(person);
            }
        }

    }
}
