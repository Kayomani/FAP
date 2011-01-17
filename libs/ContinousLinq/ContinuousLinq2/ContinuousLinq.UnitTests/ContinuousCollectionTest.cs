using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using ContinuousLinq.Aggregates;
using NUnit.Framework;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class ContinuousCollectionTest
    {
        private ContinuousCollection<Person> _target;

        [SetUp]
        public void SetUp()
        {
            _target = new ContinuousCollection<Person>();
        }


#if !SILVERLIGHT
        [Test]
        public void AddRange_Always_CallsNotifyCollectionChangedOnlyOnce()
        {
            int timesCalled = 0;

            _target.CollectionChanged += (sender, e) => timesCalled++;

            IEnumerable<Person> people = GetPeople();
            _target.AddRange(people);

            Assert.AreEqual(1, timesCalled);
        }


        [Test]
        public void AddRange_Always_CallsNotifyCollectionChangedWithCorrectValues()
        {
            NotifyCollectionChangedEventArgs args = null;

            _target.CollectionChanged += (sender, e) => args = e;

            IEnumerable<Person> people = GetPeople();
            _target.AddRange(people);

            Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
            Assert.AreEqual(0, args.NewStartingIndex);

            var newItems = args.NewItems.Cast<Person>();
            Assert.IsTrue(people.SequenceEqual(newItems));
        }


        [Test]
        public void AddRange_Always_AddsItemsToEndOfList()
        {
            _target.Add(new Person { Age = 20 });
            _target.Add(new Person { Age = 20 });

            IEnumerable<Person> people = GetPeople();
            _target.AddRange(people);

            Assert.AreEqual(12, _target.Count);

            var lastTenItemsInList = _target.Skip(2);
            Assert.IsTrue(people.SequenceEqual(lastTenItemsInList));
        }

        [Test]
        public void AddRange_Always_UpdatesQueriesCorrectly()
        {
            var peopleOverAgeFive = from person in _target
                                    where person.Age > 5
                                    select person;

            IEnumerable<Person> people = GetPeople();
            _target.AddRange(people);

            Assert.AreEqual(5, peopleOverAgeFive.Count);
            Assert.IsTrue(peopleOverAgeFive.SequenceEqual(people.Where(person => person.Age > 5)));
        }

        [Test]
        public void AddRange_Always_UpdatesContinuousValueCorrectly()
        {
            IEnumerable<Person> people = GetPeople();

            ContinuousValue<int> sum = _target.ContinuousSum(p => p.Age);

            _target.AddRange(people);

            Assert.AreEqual(55, sum.CurrentValue);
        }

        [Test]
        public void InsertRange_Always_CallsNotifyCollectionChangedOnlyOnce()
        {
            int timesCalled = 0;

            _target.CollectionChanged += (sender, e) => timesCalled++;

            IEnumerable<Person> people = GetPeople();
            _target.InsertRange(0, people);

            Assert.AreEqual(1, timesCalled);
        }

        [Test]
        public void InsertRange_Always_CallsNotifyCollectionChangedWithCorrectValues()
        {
            NotifyCollectionChangedEventArgs args = null;

            _target.CollectionChanged += (sender, e) => args = e;

            IEnumerable<Person> people = GetPeople();
            _target.InsertRange(0, people);

            Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
            Assert.AreEqual(0, args.NewStartingIndex);

            var newItems = args.NewItems.Cast<Person>();
            Assert.IsTrue(people.SequenceEqual(newItems));
        }

        [Test]
        public void InsertRange_Always_InsertsItemsAtCorrectIndexLocation()
        {
            IEnumerable<Person> peopleWithIncreasingAges = GetPeople();

            _target.Add(new Person { Age = 20 });
            _target.Add(new Person { Age = 20 });

            _target.InsertRange(1, peopleWithIncreasingAges);

            Assert.AreEqual(20, _target[0].Age);

            for (int i = 1; i <= 10; i++)
            {
                Assert.AreEqual(i, _target[i].Age);
            }

            Assert.AreEqual(20, _target[11].Age);
        }

        [Test]
        public void InsertRange_Always_UpdatesQueriesCorrectly()
        {
            _target.Add(new Person { Age = 1 });
            _target.Add(new Person { Age = 1 });

            var peopleOverAgeFive = from person in _target
                                    where person.Age > 5
                                    select person;

            IEnumerable<Person> people = GetPeople();
            _target.InsertRange(1, people);

            Assert.AreEqual(5, peopleOverAgeFive.Count);
            Assert.IsTrue(peopleOverAgeFive.SequenceEqual(people.Where(person => person.Age > 5)));
        }

        [Test]
        public void InsertRange_Always_UpdatesContinuousValueCorrectly()
        {
            _target.Add(new Person { Age = 0 });
            _target.Add(new Person { Age = 0 });

            IEnumerable<Person> people = GetPeople();

            ContinuousValue<int> sum = _target.ContinuousSum(p => p.Age);

            _target.InsertRange(1, people);

            Assert.AreEqual(55, sum.CurrentValue);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void RemoveRange_GoesBeyondIndices_ThrowsException()
        {
            _target.RemoveRange(0, 1);
        }

        [Test]
        public void RemoveRange_Always_CallsNotifyCollectionChangedOnlyOnce()
        {
            int timesCalled = 0;

            IEnumerable<Person> people = GetPeople();
            _target.AddRange(people);

            _target.CollectionChanged += (sender, e) => timesCalled++;

            _target.RemoveRange(0, 10);
            Assert.AreEqual(1, timesCalled);
        }

        [Test]
        public void RemoveRange_Always_CallsNotifyCollectionChangedWithCorrectValues()
        {
            NotifyCollectionChangedEventArgs args = null;

            IEnumerable<Person> people = GetPeople();

            _target.Add(new Person { Age = 20 });
            _target.Add(new Person { Age = 20 });

            _target.InsertRange(1, people);



            _target.CollectionChanged += (sender, e) => args = e;

            _target.RemoveRange(1, 10);

            Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.AreEqual(1, args.OldStartingIndex);

            var oldItems = args.OldItems.Cast<Person>();
            Assert.IsTrue(people.SequenceEqual(oldItems));
        }

        [Test]
        public void RemoveRange_Always_RemovesItemsAtCorrectIndex()
        {
            IEnumerable<Person> people = GetPeople();

            _target.Add(new Person { Age = 20 });
            _target.Add(new Person { Age = 20 });

            _target.InsertRange(1, people);

            _target.RemoveRange(1, 10);

            Assert.AreEqual(2, _target.Count);
            Assert.AreEqual(20, _target[0].Age);
            Assert.AreEqual(20, _target[1].Age);
        }

        [Test]
        public void RemoveRange_Always_UpdatesQueriesCorrectly()
        {
            var peopleOverAgeFive = from person in _target
                                    where person.Age > 5
                                    select person;

            IEnumerable<Person> people = GetPeople();
            _target.AddRange(people);

            _target.RemoveRange(0, 9);

            Assert.AreEqual(1, peopleOverAgeFive.Count);
            Assert.AreEqual(10, peopleOverAgeFive[0].Age);
        }

        [Test]
        public void RemoveRange_Always_UpdatesContinuousValueCorrectly()
        {
            IEnumerable<Person> people = GetPeople();
            _target.AddRange(people);

            ContinuousValue<int> sum = _target.ContinuousSum(p => p.Age);

            _target.RemoveRange(0, 9);

            Assert.AreEqual(10, sum.CurrentValue);
        }

        [Test]
        public void ReplaceRange_Always_CallsNotifyCollectionChangedOnlyOnce()
        {
            int timesCalled = 0;

            IEnumerable<Person> people = GetPeople();
            _target.AddRange(people);

            _target.CollectionChanged += (sender, e) => timesCalled++;

            var peopleAgeFive = GetPeopleWithAge(5);
            _target.ReplaceRange(0, peopleAgeFive);

            Assert.AreEqual(1, timesCalled);
        }

        [Test]
        public void ReplaceRange_Always_CallsNotifyCollectionChangedWithCorrectValues()
        {
            NotifyCollectionChangedEventArgs args = null;

            _target.CollectionChanged += (sender, e) => args = e;

            IEnumerable<Person> people = GetPeople();
            _target.AddRange(people);

            var peopleAgeFive = GetPeopleWithAge(5);
            _target.ReplaceRange(0, peopleAgeFive);

            Assert.AreEqual(NotifyCollectionChangedAction.Replace, args.Action);
            Assert.AreEqual(0, args.OldStartingIndex);

            var oldItems = args.OldItems.Cast<Person>();
            Assert.IsTrue(people.SequenceEqual(oldItems));

            var newItems = args.NewItems.Cast<Person>();
            Assert.IsTrue(peopleAgeFive.SequenceEqual(newItems));
        }

        [Test]
        public void ReplaceRange_Always_ReplacesItemsAtCorrectIndex()
        {
            IEnumerable<Person> people = GetPeople();
            _target.AddRange(people);

            var newItems = GetPeopleWithAge(20, 9);

            _target.ReplaceRange(0, newItems);

            Assert.AreEqual(10, _target.Count);

            for (int i = 0; i < 9; i++)
            {
                Assert.AreEqual(20, _target[i].Age);
            }

            Assert.AreEqual(10, _target[9].Age);
        }

        [Test]
        public void ReplaceRange_Always_UpdatesQueriesCorrectly()
        {
            var peopleOverAgeFive = from person in _target
                                    where person.Age > 5
                                    select person;

            IEnumerable<Person> people = GetPeople();
            _target.AddRange(people);

            var peopleAgeOne = GetPeopleWithAge(1, 5);

            _target.ReplaceRange(5, peopleAgeOne);

            Assert.AreEqual(0, peopleOverAgeFive.Count);
        }

        [Test]
        public void ReplaceRange_Always_UpdatesContinuousValueCorrectly()
        {
            IEnumerable<Person> people = GetPeople();
            _target.AddRange(people);

            ContinuousValue<int> sum = _target.ContinuousSum(p => p.Age);

            var peopleAgeOne = GetPeopleWithAge(1);
            _target.ReplaceRange(0, peopleAgeOne);

            Assert.AreEqual(10, sum.CurrentValue);
        }
#endif

        private static IEnumerable<Person> GetPeople()
        {
            return Enumerable.Range(0, 10).Select(i => new Person { Age = ++i }).ToList();
        }

        private static IEnumerable<Person> GetPeopleWithAge(int age)
        {
            return GetPeopleWithAge(age, 10);
        }

        private static IEnumerable<Person> GetPeopleWithAge(int age, int count)
        {
            return Enumerable.Range(0, count).Select(i => new Person { Age = age }).ToList();
        }
    }
}