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
    public class ContinuousContainsTest
    {
        private ObservableCollection<Person> _source;

        [SetUp]
        public void Setup()
        {
            _source = ClinqTestFactory.CreateTwoPersonSource();
        }

        [Test]
        public void Contains_Test()
        {
            Person p = _source[0];
            ContinuousValue<bool> hasPerson = _source.ContinuousContains(p);

            Assert.IsTrue(hasPerson.CurrentValue);
            _source.Remove(p);
            Assert.IsFalse(hasPerson.CurrentValue);
        } 

        [Test]
        public void Contains_Test_AfterEffect()
        {
            Person p = _source[0];
            bool personAE = false;
            ContinuousValue<bool> hasPerson = _source.ContinuousContains(p, val => personAE = val);

            Assert.IsTrue(hasPerson.CurrentValue);
            Assert.IsTrue(personAE);
            _source.Remove(p);
            Assert.IsFalse(hasPerson.CurrentValue);
            Assert.IsFalse(personAE);
        }
    }
}
