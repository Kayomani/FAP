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
    public class ThenByTest
    {
        private ObservableCollection<Person> _source;

        [SetUp]
        public void SetUp()
        {
            _source = ClinqTestFactory.CreateSixPersonSource();
            _source[1].Age = 20; // same as _source[2].age
            _source[2].Age = 20;
            _source[1].Name = "Zoolander";
            _source[2].Name = "Alfonse";
        }

        [Test]
        public void Sort_ThenBy_CountRemainsTheSame()
        {
            ReadOnlyContinuousCollection<Person> output =
                from person in _source
                orderby person.Age, person.Name
                select person;

            Assert.AreEqual(_source.Count, output.Count);
        }

        [Test]
        public void Sort_ThenBy_SortedItemsAreInProperIndexes()
        {
            ReadOnlyContinuousCollection<Person> output =
                from person in _source
                orderby person.Age, person.Name
                select person;

            Assert.AreEqual(output[0].Age, 0);
            Assert.AreEqual(output[1].Age, 20);
            Assert.AreEqual(output[2].Age, 20);
            // output IDX 1 should be alfonse
            // output IDX 2 should be zoolander

            Assert.AreEqual(output[1].Name, "Alfonse");
            Assert.AreEqual(output[2].Name, "Zoolander"); 
        }

        [Test]
        public void StandardClinqBehavior()
        {
            IEnumerable<Person> list = _source;

            var output = from person in list
                         orderby person.Age, person.Name
                         select person;
            
            Assert.AreEqual(_source.Count, output.Count());
        }
    }
}
