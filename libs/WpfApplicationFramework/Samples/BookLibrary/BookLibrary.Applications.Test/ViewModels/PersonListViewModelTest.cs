using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BookLibrary.Applications.Test.Views;
using BookLibrary.Applications.ViewModels;
using System.Waf.UnitTesting;
using BookLibrary.Domain;
using System.Waf.Applications;

namespace BookLibrary.Applications.Test.ViewModels
{
    [TestClass]
    public class PersonListViewModelTest
    {
        [TestMethod]
        public void PersonListViewModelPersonsTest()
        {
            List<Person> persons = new List<Person>()
            {
                new Person() { Firstname = "Harry" },
                new Person() { Firstname = "Ron" }
            };
            
            PersonListViewMock personListView = new PersonListViewMock();
            
            AssertHelper.ExpectedException<ArgumentNullException>(() => new PersonListViewModel(personListView, null));
            PersonListViewModel personListViewModel = new PersonListViewModel(personListView, persons);

            Assert.AreEqual(persons, personListViewModel.Persons);
            Assert.IsNull(personListViewModel.SelectedPerson);
            Assert.IsFalse(personListViewModel.SelectedPersons.Any());

            // Select the first person
            AssertHelper.PropertyChangedEvent(personListViewModel, x => x.SelectedPerson,
                () => personListViewModel.SelectedPerson = persons.First());
            Assert.AreEqual(persons.First(), personListViewModel.SelectedPerson);
            
            personListViewModel.SelectedPersons.Add(persons.First());
            Assert.IsTrue(personListViewModel.SelectedPersons.SequenceEqual(new Person[] { persons.First() }));

            // Select both persons
            personListViewModel.SelectedPersons.Add(persons.Last());
            Assert.IsTrue(personListViewModel.SelectedPersons.SequenceEqual(persons));
        }

        [TestMethod]
        public void PersonListViewModelCommandsTest()
        {
            List<Person> persons = new List<Person>()
            {
                new Person() { Firstname = "Harry" },
                new Person() { Firstname = "Ron" }
            };
            
            PersonListViewMock personListView = new PersonListViewMock();
            PersonListViewModel personListViewModel = new PersonListViewModel(personListView, persons);

            DelegateCommand mockCommand = new DelegateCommand(() => {});
            AssertHelper.PropertyChangedEvent(personListViewModel, x => x.AddNewCommand, () => 
                personListViewModel.AddNewCommand = mockCommand);
            Assert.AreEqual(mockCommand, personListViewModel.AddNewCommand);

            AssertHelper.PropertyChangedEvent(personListViewModel, x => x.RemoveCommand, () =>
                personListViewModel.RemoveCommand = mockCommand);
            Assert.AreEqual(mockCommand, personListViewModel.RemoveCommand);
        }
    }
}
