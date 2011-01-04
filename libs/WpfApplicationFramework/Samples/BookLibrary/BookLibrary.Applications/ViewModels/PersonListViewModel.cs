using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Waf.Applications;
using System.Windows.Input;
using BookLibrary.Applications.Views;
using BookLibrary.Domain;
using System;

namespace BookLibrary.Applications.ViewModels
{
    public class PersonListViewModel : ViewModel<IPersonListView>
    {
        private readonly IEnumerable<Person> persons;
        private readonly ObservableCollection<Person> selectedPersons;
        private Person selectedPerson;
        private ICommand addNewCommand;
        private ICommand removeCommand;
        

        public PersonListViewModel(IPersonListView view, IEnumerable<Person> persons)
            : base(view)
        {
            if (persons == null) { throw new ArgumentNullException("persons"); }
            
            this.persons = persons;
            this.selectedPersons = new ObservableCollection<Person>();
        }


        public IEnumerable<Person> Persons { get { return persons; } }

        public ObservableCollection<Person> SelectedPersons
        {
            get { return selectedPersons; }
        }

        public Person SelectedPerson
        {
            get { return selectedPerson; }
            set
            {
                if (selectedPerson != value)
                {
                    selectedPerson = value;
                    RaisePropertyChanged("SelectedPerson");
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
