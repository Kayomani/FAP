using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookLibrary.Applications.Views;
using System.Waf.Applications;
using System.Windows.Input;
using BookLibrary.Domain;
using BookLibrary.Applications.Properties;

namespace BookLibrary.Applications.ViewModels
{
    public class LendToViewModel : ViewModel<ILendToView>
    {
        private readonly Book book;
        private readonly IEnumerable<Person> persons;
        private readonly DelegateCommand okCommand;
        private bool isWasReturned;
        private bool isLendTo;
        private Person selectedPerson;
        private bool dialogResult;

        
        public LendToViewModel(ILendToView view, Book book, IEnumerable<Person> persons) : base(view)
        {
            if (book == null) { throw new ArgumentNullException("book"); }
            if (persons == null) { throw new ArgumentNullException("persons"); }
            
            this.book = book;
            this.persons = persons;
            this.okCommand = new DelegateCommand(OKHandler);

            if (book.LendTo == null) { isLendTo = true; }
            else { isWasReturned = true; }
            selectedPerson = persons.FirstOrDefault();
        }


        public static string Title { get { return ApplicationInfo.ProductName; } }

        public ICommand OkCommand { get { return okCommand; } }

        public Book Book { get { return book; } }

        public IEnumerable<Person> Persons { get { return persons; } }
        

        public bool IsWasReturned
        {
            get { return isWasReturned; }
            set
            {
                if (isWasReturned != value)
                {
                    isWasReturned = value;
                    IsLendTo = !value;
                    RaisePropertyChanged("IsWasReturned");
                }
            }
        }
        
        public bool IsLendTo
        {
            get { return isLendTo; }
            set
            {
                if (isLendTo != value)
                {
                    isLendTo = value;
                    IsWasReturned = !value;
                    RaisePropertyChanged("IsLendTo");
                }
            }
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


        public bool ShowDialog(object owner)
        {
            ViewCore.ShowDialog(owner);
            return dialogResult;
        }

        private void OKHandler() 
        {
            dialogResult = true;
            if (IsWasReturned) { SelectedPerson = null; }
            ViewCore.Close();
        }
    }
}
