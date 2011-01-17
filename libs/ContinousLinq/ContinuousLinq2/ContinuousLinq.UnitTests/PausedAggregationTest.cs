using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ContinuousLinq.Aggregates;
using System.Collections.ObjectModel;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class PausedAggregationTest
    {
        private ObservableCollection<Person> _source;

        [SetUp]
        public void Setup()
        {
            _source = ClinqTestFactory.CreateTwoPersonSource();
        }

        [Test]
        [Ignore("Need to remove after effects as they do not work")]
        public void PauseAggregation_OneItemAddedToCollection_ValueUpdatedAfterUsingBlockExits()
        {
            ContinuousValue<int> sum = _source.ContinuousSum(p => p.Age);

            Assert.AreEqual(30, sum.CurrentValue);

            int callCount = 0;
            sum.PropertyChanged += (sender, args) => callCount++;
            
            using (PausedAggregation pausedAggregation = new PausedAggregation())
            {
                _source.Add(new Person() { Age = 30 });
                Assert.AreEqual(0, callCount);
            }
            Assert.AreEqual(1, callCount);
        }

        [Test]
        [Ignore("Need to remove after effects as they do not work")]
        public void PauseAggregation_PropertyChangedInCollection_ValueUpdatedAfterUsingBlockExits()
        {
            ContinuousValue<int> sum = _source.ContinuousSum(p => p.Age);

            Assert.AreEqual(30, sum.CurrentValue);

            int callCount = 0;
            sum.PropertyChanged += (sender, args) => callCount++;

            using (PausedAggregation pausedAggregation = new PausedAggregation())
            {
                _source[0].Age = 1000;
                Assert.AreEqual(0, callCount);
            }

            Assert.AreEqual(1, callCount);
        }

        [Test]
        [Ignore("This is no longer pertinant after online calculations")]
        public void PauseAggregation_NestedPauses_ValueUpdatedAfterUsingBlockExits()
        {
            ContinuousValue<int> sum = _source.ContinuousSum(p => p.Age);

            Assert.AreEqual(30, sum.CurrentValue);

            int callCount = 0;
            sum.PropertyChanged += (sender, args) => callCount++;

            using (PausedAggregation pausedAggregation = new PausedAggregation())
            {
                using (PausedAggregation pausedAggregationTwo = new PausedAggregation())
                {
                    _source[0].Age = 1000;
                    Assert.AreEqual(0, callCount);
                }
            }

            Assert.AreEqual(1, callCount);
        }
    }
}
