using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections.ObjectModel;
using ContinuousLinq.Aggregates;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class FirstOrDefaultTest
    {
        private ObservableCollection<Person> _source;

        [SetUp]
        public void Setup()
        {
            _source = ClinqTestFactory.CreateTwoPersonSource();
        }

        [Test]
        public void FirstOrDefault_Test_GetFirstItem()
        {
            Person actualFirst = _source[0];
            ContinuousValue<Person> firstPerson = _source.ContinuousFirstOrDefault();

            Assert.AreEqual(actualFirst, firstPerson.CurrentValue);

        }

        [Test]
        public void FirstOrDefault_GetFirstItemAfterMove()
        {
            Person newFirst = new Person() { Age = 40, Name = "New" };

            ContinuousValue<Person> firstPerson = _source.ContinuousFirstOrDefault();

            _source.Insert(0, newFirst);

            Assert.AreEqual(newFirst, firstPerson.CurrentValue);

        }

        [Test]
        public void FirstOrDefault_AfterEffect_Test()
        {
            int firstPersonsAge = 0;
            ContinuousValue<Person> firstPerson = _source.ContinuousFirstOrDefault(p => firstPersonsAge = p.Age);

            Assert.AreEqual(_source[0].Age, firstPersonsAge);
            Assert.AreEqual(firstPerson.CurrentValue.Age, firstPersonsAge);
        }

        [Test]
        public void FirstOrDefault_WithPredicate()
        {
            ContinuousValue<Person> firstOverTen = _source.ContinuousFirstOrDefault(p => p.Age > 10);

            Assert.AreEqual(firstOverTen.CurrentValue.Name, "Jim");
            Assert.AreEqual(firstOverTen.CurrentValue.Age, 20);
        }

        [Test]
        public void FirstOrDefault_Predicate_WithChange()
        {
            ContinuousValue<Person> firstOverThirty = _source.ContinuousFirstOrDefault(p => p.Age > 30);

            Assert.IsNull(firstOverThirty.CurrentValue);
            _source[0].Age = 75;
            Assert.IsNotNull(firstOverThirty.CurrentValue);
            Assert.AreEqual(firstOverThirty.CurrentValue.Age, 75);
        }

        [Test]
        public void FirstOrDefault_Predicate_AfterEffect()
        {
            string overThirtyName = null;
            ContinuousValue<Person> firstOverThirty = _source.ContinuousFirstOrDefault(
                p => p.Age > 30,
                p => overThirtyName = (p == null) ? "Nobody" : p.Name);

            Assert.IsNull(firstOverThirty.CurrentValue);
            Assert.AreEqual(overThirtyName, "Nobody");
            _source[0].Age = 75;
            Assert.IsNotNull(firstOverThirty.CurrentValue);
            Assert.AreEqual(firstOverThirty.CurrentValue.Name, overThirtyName);
        }

    }
}
