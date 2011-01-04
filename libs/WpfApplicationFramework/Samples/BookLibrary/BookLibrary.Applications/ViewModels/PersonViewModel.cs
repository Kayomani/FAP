using System.ComponentModel.Composition;
using System.Waf.Applications;
using BookLibrary.Applications.Views;
using BookLibrary.Domain;

namespace BookLibrary.Applications.ViewModels
{
    [Export]
    public class PersonViewModel : ViewModel<IPersonView>
    {
        private bool isValid = true;
        private Person person;

        
        [ImportingConstructor]
        public PersonViewModel(IPersonView view)
            : base(view)
        {
        }


        public bool IsEnabled { get { return Person != null; } }

        public bool IsValid
        {
            get { return isValid; }
            set
            {
                if (isValid != value)
                {
                    isValid = value;
                    RaisePropertyChanged("IsValid");
                }
            }
        }

        public Person Person
        {
            get { return person; }
            set
            {
                if (person != value)
                {
                    person = value;
                    RaisePropertyChanged("Person");
                    RaisePropertyChanged("IsEnabled");
                }
            }
        }


        public void Focus()
        {
            ViewCore.FocusFirstControl();
        }
    }
}
