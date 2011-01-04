using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BookLibrary.Applications.Controllers;
using System.ComponentModel.Composition.Hosting;
using BookLibrary.Applications.Services;
using BookLibrary.Domain;
using BookLibrary.Applications.Test.Views;
using BookLibrary.Applications.ViewModels;
using BookLibrary.Applications.Views;
using System.Waf.Applications;
using System.Waf.UnitTesting;

namespace BookLibrary.Applications.Test.Controllers
{
    [TestClass]
    public class PersonControllerTest
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
        public void PersonControllerSelectionTest()
        {
            IEntityService entityService = container.GetExportedValue<IEntityService>();
            entityService.Persons.Add(new Person() { Firstname = "Harry"});
            entityService.Persons.Add(new Person() { Firstname = "Ron" });
            
            PersonController personController = container.GetExportedValue<PersonController>();
            personController.Initialize();

            // Check that Initialize shows the PersonListView and PersonView
            ShellViewModel shellViewModel = container.GetExportedValue<ShellViewModel>();
            Assert.IsInstanceOfType(shellViewModel.PersonListView, typeof(IPersonListView));
            Assert.IsInstanceOfType(shellViewModel.PersonView, typeof(IPersonView));

            // Check that the first Person is selected
            IPersonListView personListView = container.GetExportedValue<IPersonListView>();
            PersonListViewModel personListViewModel = personListView.GetViewModel<PersonListViewModel>();
            Assert.AreEqual(entityService.Persons.First(), personListViewModel.SelectedPerson);
            
            // Change the selection
            PersonViewModel personViewModel = container.GetExportedValue<PersonViewModel>();
            personListViewModel.SelectedPerson = entityService.Persons.Last();
            Assert.AreEqual(entityService.Persons.Last(), personViewModel.Person);
        }

        [TestMethod]
        public void PersonControllerAddAndRemoveTest()
        {
            Person harry = new Person() { Firstname = "Harry" };
            Person ron = new Person() { Firstname = "Ron" };
            
            IEntityService entityService = container.GetExportedValue<IEntityService>();
            entityService.Persons.Add(harry);
            entityService.Persons.Add(ron);

            PersonController personController = container.GetExportedValue<PersonController>();
            personController.Initialize();

            PersonListViewMock personListView = container.GetExportedValue<PersonListViewMock>();
            PersonListViewModel personListViewModel = personListView.GetViewModel<PersonListViewModel>();
            PersonViewMock personView = container.GetExportedValue<PersonViewMock>();
            PersonViewModel personViewModel = personView.GetViewModel<PersonViewModel>();

            // Add a new Person
            Assert.AreEqual(2, entityService.Persons.Count);
            Assert.IsTrue(personListViewModel.AddNewCommand.CanExecute(null));
            personListViewModel.AddNewCommand.Execute(null);
            Assert.AreEqual(3, entityService.Persons.Count);

            // Check that the new Person is selected and the first control gets the focus
            Assert.AreEqual(entityService.Persons.Last(), personViewModel.Person);
            Assert.IsTrue(personView.FirstControlHasFocus);

            // Simulate an invalid UI input state => the user can't add more persons
            AssertHelper.CanExecuteChangedEvent(personListViewModel.AddNewCommand, () => 
                personViewModel.IsValid = false);
            Assert.IsFalse(personListViewModel.AddNewCommand.CanExecute(null));

            // Remove the last two Persons at once
            personListViewModel.SelectedPersons.Add(ron);
            personListViewModel.SelectedPersons.Add(entityService.Persons.Last());
            Assert.IsTrue(personListViewModel.RemoveCommand.CanExecute(null));
            personListViewModel.RemoveCommand.Execute(null);
            Assert.IsTrue(entityService.Persons.SequenceEqual(new Person[] { harry }));

            // Deselect all Persons => the Remove command must be deactivated
            AssertHelper.CanExecuteChangedEvent(personListViewModel.RemoveCommand, () =>
            {
                personListViewModel.SelectedPersons.Clear();
                personListViewModel.SelectedPerson = null;
            });
            Assert.IsFalse(personListViewModel.RemoveCommand.CanExecute(null));
        }
    }
}
