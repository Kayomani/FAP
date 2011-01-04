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
    public class PersonController : Controller
    {
        private readonly CompositionContainer container;
        private readonly IEntityService entityService;
        private readonly ShellViewModel shellViewModel;
        private readonly PersonViewModel personViewModel;
        private readonly DelegateCommand addNewCommand;
        private readonly DelegateCommand removeCommand;
        private PersonListViewModel personListViewModel;
        

        [ImportingConstructor]
        public PersonController(CompositionContainer container, IEntityService entityService, ShellViewModel shellViewModel, 
            PersonViewModel personViewModel)
        {
            this.container = container;
            this.entityService = entityService;
            this.shellViewModel = shellViewModel;
            this.personViewModel = personViewModel;
            this.addNewCommand = new DelegateCommand(AddNewPerson, CanAddPerson);
            this.removeCommand = new DelegateCommand(RemovePerson, CanRemovePerson);
        }


        public void Initialize()
        {
            personViewModel.PropertyChanged += PersonViewModelPropertyChanged;
            
            IPersonListView personListView = container.GetExportedValue<IPersonListView>();
            personListViewModel = new PersonListViewModel(personListView, entityService.Persons);
            personListViewModel.AddNewCommand = addNewCommand;
            personListViewModel.RemoveCommand = removeCommand;
            personListViewModel.PropertyChanged += PersonListViewModelPropertyChanged;

            shellViewModel.PersonListView = personListViewModel.View;
            shellViewModel.PersonView = personViewModel.View;

            personListViewModel.SelectedPerson = personListViewModel.Persons.FirstOrDefault();
        }

        private bool CanAddPerson() { return personViewModel.IsValid; }

        private void AddNewPerson()
        {
            Person person = new Person();
            entityService.Persons.Add(person);
            
            personListViewModel.SelectedPerson = person;
            personViewModel.Focus();
        }

        private bool CanRemovePerson() { return personListViewModel.SelectedPerson != null; }

        private void RemovePerson()
        {
            foreach (Person person in personListViewModel.SelectedPersons.ToArray())
            {
                entityService.Persons.Remove(person);
            }
        }

        private void UpdateCommands()
        {
            addNewCommand.RaiseCanExecuteChanged();
            removeCommand.RaiseCanExecuteChanged();
        }

        private void PersonListViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedPerson")
            {
                personViewModel.Person = personListViewModel.SelectedPerson;
                UpdateCommands();
            }
        }

        private void PersonViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsValid")
            {
                UpdateCommands();
            }
        }
    }
}
