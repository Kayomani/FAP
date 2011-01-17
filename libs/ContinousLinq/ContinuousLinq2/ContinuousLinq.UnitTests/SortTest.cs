using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Collections;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class SortTest
    {
        private ObservableCollection<Person> _source;

        [SetUp]
        public void SetUp()
        {
            _source = ClinqTestFactory.CreateSixPersonSource();
        }

        [Test]
        public void RemoveFromSource_LastItemInCollection_CountIsZeroAndNoExceptionThrown()
        {
            var sourceWithTwoItems = new ObservableCollection<Person>();
            var personOne = new Person("Bob", 10);
            var personTwo = new Person("Jim", 20);

            ReadOnlyContinuousCollection<string> output =
                from person in sourceWithTwoItems
                where person.Age <= 20
                orderby person.Name
                select person.Name;

            sourceWithTwoItems.Add(personOne);
            sourceWithTwoItems.Add(personTwo);
            sourceWithTwoItems.Remove(personOne);
            
            //Assert.AreEqual(_source.Count, output.Count);
        }


        [Test]
        public void Sort_CountRemainsTheSame()
        {
            ReadOnlyContinuousCollection<Person> output =
                from person in _source
                orderby person.Age
                select person;

            Assert.AreEqual(_source.Count, output.Count);
        }

        [Test]
        public void Sort_SortedListRemainsSorted()
        {
            ReadOnlyContinuousCollection<Person> output =
                from person in _source
                orderby person.Age
                select person;

            // source list is already pre-sorted by age, verify that it
            // stayed that way.
            for (int x = 0; x < _source.Count; x++)
                Assert.AreEqual(_source[x].Age, output[x].Age);
        }

        [Test]
        public void Sort_HeadBecomesTail()
        {
            ReadOnlyContinuousCollection<Person> output =
                from person in _source
                orderby person.Age
                select person;

            _source[0].Age = 99; //move the first to the last.

            Assert.AreEqual(_source[0].Age, output[output.Count - 1].Age);
        }

        [Test]
        public void Sort_TailBecomesHead()
        {
            ReadOnlyContinuousCollection<Person> output =
                from person in _source
                orderby person.Age
                select person;

            _source[_source.Count - 1].Age = -1;

            Assert.AreEqual(_source[_source.Count - 1].Age, output[0].Age);

        }

        [Test]
        public void Sort_Descending_CountRemainsTheSame()
        {
            ReadOnlyContinuousCollection<Person> output =
                from person in _source
                orderby person.Age descending
                select person;

            Assert.AreEqual(_source.Count, output.Count);
        }

        [Test]
        public void Sort_Descending_HeadBecomesTail()
        {
            ReadOnlyContinuousCollection<Person> output =
                from person in _source
                orderby person.Age descending
                select person;

            _source[0].Age = -1; // reversed direction, set to -1 should put it last.

            Assert.AreEqual(_source[0].Age, output[output.Count - 1].Age);
        }

        [Test]
        public void Sort_Descending_TailBecomesHead()
        {
            ReadOnlyContinuousCollection<Person> output =
                from person in _source
                orderby person.Age descending
                select person;

            _source[_source.Count - 1].Age = 99; // reversed direction, set to 99 should put it first.
            Assert.AreEqual(_source[_source.Count - 1].Age, output[0].Age);
        }

        [Test]
        public void Sort_Descending_SortedItemsAreInProperIndexes()
        {
            ReadOnlyContinuousCollection<Person> output =
                from person in _source
                orderby person.Age descending
                select person;
            
            int outputIdx = output.Count - 1;

            for (int sourceIdx = 0; sourceIdx < _source.Count; sourceIdx++)
            {
                Assert.AreEqual(_source[sourceIdx].Age, output[outputIdx].Age);
                outputIdx--;
            }
        }

        [Test]
        public void Sort_ThenBy_HeadBecomesTail()
        {
        }

        [Test]
        public void Sort_ThenBy_TailBecomesHead()
        {
        }

        [Test]
        public void Sort_ThenBy_Descending_CountRemainsTheSame()
        {
        }

        [Test]
        public void Sort_ThenBy_Descending_SortedItemsAreInProperIndexes()
        {
        }

        [Test]
        public void Sort_ThenBy_Descending_HeadBecomesTail()
        {
        }

        [Test]
        public void Sort_ThenBy_Descending_TailBecomesHead()
        {
        }
    }
}
