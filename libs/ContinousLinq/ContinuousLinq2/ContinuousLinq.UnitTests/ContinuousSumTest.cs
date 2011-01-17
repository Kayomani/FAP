using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using NUnit.Framework;
using ContinuousLinq.Aggregates;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ContinuousLinq.WeakEvents;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class ContinuousSumTest
    {
        private ObservableCollection<Person> _source;

        [SetUp]
        public void Setup()
        {
            _source = ClinqTestFactory.CreateTwoPersonSource();
        }

        [Test]
        public void SumIntegers_ImmediatelyAfterConstruction_SumCompleted()
        {
            ContinuousValue<int> sum = _source.ContinuousSum(p => p.Age);

            Assert.AreEqual(30, sum.CurrentValue);
        }

        [Test]
        public void SumIntegers_ChangeValueOfMonitoredPropertyCollection_SumUpdated()
        {
            ContinuousValue<int> sum = _source.ContinuousSum(p => p.Age);
            _source[0].Age++;

            Assert.AreEqual(31, sum.CurrentValue);
        }

        //[Test]
        //public void SumIntegers_ChangeValueOfMonitoredPropertyCollection_PostChangeLambda()
        //{
        //    int monitoredSum = 0;

        //    ContinuousValue<int> sum = _source.ContinuousSum(p => p.Age, newVal => monitoredSum = newVal);
        //    _source[0].Age += 12;

        //    Assert.AreEqual(sum.CurrentValue, 42);
        //    Assert.AreEqual(monitoredSum, sum.CurrentValue);
        //}

        [Test]
        public void SumIntegers_ChangeValueOfMonitoredPropertyCollection_PropertyChangedEventFires()
        {
            ContinuousValue<int> sum = _source.ContinuousSum(p => p.Age);
            int callCount = 0;
            sum.PropertyChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual("CurrentValue", args.PropertyName);
            };

            _source[0].Age++;

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void SumIntegers_AddItemToCollection_SumUpdated()
        {
            ContinuousValue<int> sum = _source.ContinuousSum(p => p.Age);
            _source.Add(new Person() { Age = 10 });
            Assert.AreEqual(40, sum.CurrentValue);
        }

        [Test]
        public void SumIntegers_RemoveItemFromCollection_SumUpdated()
        {
            ContinuousValue<int> sum = _source.ContinuousSum(p => p.Age);
            _source.Remove(_source[0]);
            Assert.AreEqual(20, sum.CurrentValue);
        }

        [Test]
        public void SumIntegers_ReplaceItemInCollection_SumUpdated()
        {
            ContinuousValue<int> sum = _source.ContinuousSum(p => p.Age);
            _source[0] = new Person() { Age = 50 };
            Assert.AreEqual(70, sum.CurrentValue);
        }

#if !SILVERLIGHT
        [Test]
        public void SumIntegers_MoveItemInCollection_SumTheSame()
        {
            ContinuousValue<int> sum = _source.ContinuousSum(p => p.Age);
            _source.Move(0, 1);

            Assert.AreEqual(30, sum.CurrentValue);
        }
#endif


    }
}
