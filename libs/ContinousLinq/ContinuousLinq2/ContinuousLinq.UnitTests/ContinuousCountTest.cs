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
    public class ContinuousCountTest
    {
        private ObservableCollection<Person> _source;

        [SetUp]
        public void Setup()
        {
            _source = ClinqTestFactory.CreateTwoPersonSource();
        }

        [Test]
        public void Count_ImmediatelyAfterConstruction_SumCompleted()
        {
            ContinuousValue<int> count = _source.ContinuousCount();

            Assert.AreEqual(2, count.CurrentValue);
        }

        [Test]
        public void Count_AddingValueToCollection_CountUpdated()
        {
            ContinuousValue<int> count = _source.ContinuousCount();
            _source.Add(new Person());

            Assert.AreEqual(3, count.CurrentValue);
        }

        [Test]
        public void Count_AddingValueToCollection_AfterEffect()
        {
            int modCount = 0;
            ContinuousValue<int> count = _source.ContinuousCount(c => modCount = c);
            _source.Add(new Person());

            Assert.AreEqual(3, count.CurrentValue);
            Assert.AreEqual(modCount, count.CurrentValue);
        }
    }
}
