using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BookLibrary.Applications.ViewModels;
using BookLibrary.Applications.Test.Views;
using System.Waf.UnitTesting;
using BookLibrary.Domain;

namespace BookLibrary.Applications.Test.ViewModels
{
    [TestClass]
    public class PersonViewModelTest
    {
        [TestMethod]
        public void PersonViewModelPersonTest()
        {
            PersonViewMock personView = new PersonViewMock();
            PersonViewModel personViewModel = new PersonViewModel(personView);

            Assert.IsFalse(personViewModel.IsEnabled);

            Person person = new Person();
            AssertHelper.PropertyChangedEvent(personViewModel, x => x.Person, () => personViewModel.Person = person);
            Assert.AreEqual(person, personViewModel.Person);
            Assert.IsTrue(personViewModel.IsEnabled);

            AssertHelper.PropertyChangedEvent(personViewModel, x => x.IsEnabled, () => personViewModel.Person = null);
            Assert.IsNull(personViewModel.Person);
            Assert.IsFalse(personViewModel.IsEnabled);
        }

        [TestMethod]
        public void PersonViewModelIsValidTest()
        {
            PersonViewMock personView = new PersonViewMock();
            PersonViewModel personViewModel = new PersonViewModel(personView);
            
            Assert.IsTrue(personViewModel.IsValid);

            AssertHelper.PropertyChangedEvent(personViewModel, x => x.IsValid, () => personViewModel.IsValid = false);
            Assert.IsFalse(personViewModel.IsValid);

            Assert.IsFalse(personView.FirstControlHasFocus);
            personViewModel.Focus();
            Assert.IsTrue(personView.FirstControlHasFocus);
        }
    }
}
