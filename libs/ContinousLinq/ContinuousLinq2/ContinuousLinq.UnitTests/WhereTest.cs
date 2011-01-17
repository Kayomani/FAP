using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ContinuousLinq;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class WhereTest
    {
        private ObservableCollection<Person> _source;

        [SetUp]
        public void Setup()
        {
            _source = ClinqTestFactory.CreateTwoPersonSource();
        }

        [Test]
        public void Where_SimpleOnePropertyFilter_ItemsFiltered()
        {
            ReadOnlyContinuousCollection<Person> output = from person in _source
                                                          where person.Age > 10
                                                          select person;

            Assert.AreEqual(1, output.Count);
        }

        [Test]
        public void Where_TwoLevelPropertyFilter_ItemsFiltered()
        {
            Person brother = new Person();
            brother.Age = 0;

            _source[0].Brother = brother;

            ReadOnlyContinuousCollection<Person> output = from person in _source
                                                          where person.Brother != null && person.Brother.Age > 10
                                                          select person;

            Assert.AreEqual(0, output.Count);
            
            brother.Age = 100;

            Assert.AreEqual(1, output.Count);
        }

        [Test]
        public void Where_FilterContainsAConstant_CorrectlyFiltered()
        {
            Person person = new Person("Ninja", 20);

            ReadOnlyContinuousCollection<Person> peopleMatchingAge = person.GetPeopleWithSameAge(_source);

            Assert.AreEqual(1, peopleMatchingAge.Count);
        }

        [Test]
        public void Where_FilterContainsAConstantWithPropertyChangingOnConstant_CorrectResultsReturned()
        {
            Person person = new Person("Ninja", 20);

            ReadOnlyContinuousCollection<Person> peopleMatchingAge = person.GetPeopleWithSameAge(_source);

            int callCount = 0;
            peopleMatchingAge.CollectionChanged += (sender, args) => callCount++;

            person.Age = 10;

            Assert.AreEqual(2, callCount); // one remove and one add
            Assert.AreEqual(1, peopleMatchingAge.Count);
        }

        [Test]
        public void Where_FilterContainsTwoLevelConstantWithConstantNull_CorrectResultsReturned()
        {
            Person person = new Person("Ninja", 20);

            ReadOnlyContinuousCollection<Person> peopleMatchingAge = person.GetPeopleWithSameAgeAsBrother(_source);

            Assert.AreEqual(0, peopleMatchingAge.Count);
        }


        [Test]
        public void Where_FilterContainsTwoLevelConstantWithPropertyChangingOnConstant_CorrectResultsReturned()
        {
            Person person = new Person("Ninja", 100);

            ReadOnlyContinuousCollection<Person> peopleMatchingAge = person.GetPeopleWithSameAgeAsBrother(_source);

            person.Brother = new Person("Brother", 20);

            Assert.AreEqual(1, peopleMatchingAge.Count);
        }

        [Test]
        public void DropReference_Always_GarbageCollectsResultCollection()
        {
            var ageCollection = from person in _source
                                where person.Age > 10
                                select person;

            var weakReference = new WeakReference(ageCollection);
            Assert.IsTrue(weakReference.IsAlive);

            ageCollection = null;
            GC.Collect();
            Assert.IsFalse(weakReference.IsAlive);
        }
    }
}
