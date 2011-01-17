using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using ContinuousLinq;
using System.Collections.Specialized;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class GroupJoinTest
    {
        private ReadOnlyContinuousCollection<Pair<Person, ReadOnlyContinuousCollection<Person>>> _target;
        private ObservableCollection<Person> _outer;
        private ObservableCollection<Person> _inner;
        private IEnumerable<Pair<Person, IEnumerable<Person>>> _standardLinqResults;

        public class Pair<TFirst, TSecond>
        {
            public TFirst First { get; set; }
            public TSecond Second { get; set; }

            public Pair(TFirst first, TSecond second)
            {
                First = first;
                Second = second;
            }
        }

        [SetUp]
        public void Setup()
        {
            _outer = ClinqTestFactory.CreateTwoPersonSource();
            _inner = ClinqTestFactory.CreateTwoPersonSource();

            _target = from outerPerson in _outer
                      join innerPerson in _inner on outerPerson.Age equals innerPerson.Age into innersMatchingOuterAge
                      select new Pair<Person, ReadOnlyContinuousCollection<Person>>(outerPerson, innersMatchingOuterAge);

            _standardLinqResults = from outerPerson in _outer.AsEnumerable()
                                   join innerPerson in _inner on outerPerson.Age equals innerPerson.Age into innersMatchingOuterAge
                                   select new Pair<Person, IEnumerable<Person>>(outerPerson, innersMatchingOuterAge);
        }

        private static void AssertAreEquivalent(IEnumerable<Pair<Person, IEnumerable<Person>>> standardLinq, ReadOnlyContinuousCollection<Pair<Person, ReadOnlyContinuousCollection<Person>>> clinqResults)
        {
            Assert.AreEqual(standardLinq.Count(), clinqResults.Count);

            int i = 0;
            foreach (var standardLinqGroup in standardLinq)
            {
                Assert.AreEqual(standardLinqGroup.First, clinqResults[i].First);
                CollectionAssert.AreEquivalent(standardLinqGroup.Second, clinqResults[i].Second);
                i++;
            }
        }

        [Test]
        public void Initialize_Always_MatchesStandardLinqResults()
        {
            AssertAreEquivalent(_standardLinqResults, _target);
        }

        [Test]
        public void AddToOuter_Always_NotifiesCollectionChanged()
        {
            Person newPerson = new Person("New", _inner[1].Age);

            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;

                Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
                Assert.AreEqual(1, args.NewStartingIndex);
                Assert.AreEqual(1, args.NewItems.Count);

                var newResult0 = (Pair<Person, ReadOnlyContinuousCollection<Person>>)args.NewItems[0];
                Assert.AreEqual(newPerson, newResult0.First);
                Assert.AreEqual(1, newResult0.Second.Count);
                CollectionAssert.Contains(newResult0.Second, _inner[1]);
            };

            _outer.Insert(1, newPerson);

            Assert.AreEqual(1, callCount);
            AssertAreEquivalent(_standardLinqResults, _target);
        }

        [Test]
        public void RemoveFromOuter_Always_NotifiesCollectionChanged()
        {
            Person oldPerson = _outer[1];

            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;

                Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
                Assert.AreEqual(1, args.OldStartingIndex);
                Assert.AreEqual(1, args.OldItems.Count);

                var oldResult = (Pair<Person, ReadOnlyContinuousCollection<Person>>)args.OldItems[0];
                Assert.AreEqual(oldPerson, oldResult.First);
                Assert.AreEqual(1, oldResult.Second.Count);
                CollectionAssert.Contains(oldResult.Second, _inner[1]);
            };

            _outer.RemoveAt(1);

            Assert.AreEqual(1, callCount);
            AssertAreEquivalent(_standardLinqResults, _target);
        }

        [Test]
        public void ResetOuter_Always_NotifiesCollectionChanged()
        {
            Person oldPerson = _outer[1];

            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;

                Assert.AreEqual(NotifyCollectionChangedAction.Reset, args.Action);
            };

            _outer.Clear();

            Assert.AreEqual(1, callCount);
            AssertAreEquivalent(_standardLinqResults, _target);
        }

        [Test]
        public void ReplaceInOuter_Always_NotifiesCollectionChanged()
        {
            Person oldPerson = _outer[1];
            Person newPerson = new Person("New", _inner[1].Age);

            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;

                Assert.AreEqual(NotifyCollectionChangedAction.Replace, args.Action);
                Assert.AreEqual(1, args.OldItems.Count);

                Assert.AreEqual(1, args.NewStartingIndex);
                Assert.AreEqual(1, args.NewItems.Count);

                var oldResult = (Pair<Person, ReadOnlyContinuousCollection<Person>>)args.OldItems[0];
                Assert.AreEqual(oldPerson, oldResult.First);
                Assert.AreEqual(1, oldResult.Second.Count);
                CollectionAssert.Contains(oldResult.Second, _inner[1]);

                var newResult = (Pair<Person, ReadOnlyContinuousCollection<Person>>)args.NewItems[0];
                Assert.AreEqual(newPerson, newResult.First);
                Assert.AreEqual(1, newResult.Second.Count);
                CollectionAssert.Contains(newResult.Second, _inner[1]);
            };

            _outer[1] = newPerson;

            Assert.AreEqual(1, callCount);
            AssertAreEquivalent(_standardLinqResults, _target);
        }

#if !SILVERLIGHT
        [Test]
        public void MoveInOuter_Always_NotifiesCollectionChanged()
        {
            Person person = _outer[1];

            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;

                Assert.AreEqual(NotifyCollectionChangedAction.Move, args.Action);
                Assert.AreEqual(1, args.OldStartingIndex);
                Assert.AreEqual(1, args.OldItems.Count);

                Assert.AreEqual(0, args.NewStartingIndex);
                Assert.AreEqual(1, args.NewItems.Count);

                var oldResult = (Pair<Person, ReadOnlyContinuousCollection<Person>>)args.OldItems[0];
                Assert.AreEqual(person, oldResult.First);
                Assert.AreEqual(1, oldResult.Second.Count);
                CollectionAssert.Contains(oldResult.Second, _inner[1]);

                var newResult = (Pair<Person, ReadOnlyContinuousCollection<Person>>)args.NewItems[0];
                Assert.AreEqual(person, newResult.First);
                Assert.AreEqual(1, newResult.Second.Count);
                CollectionAssert.Contains(newResult.Second, _inner[1]);
            };

            _outer.Move(1, 0);

            Assert.AreEqual(1, callCount);
            AssertAreEquivalent(_standardLinqResults, _target);
        }
#endif

        [Test]
        public void OnOuterKeyChanged_Always_UpdatesInnerResults()
        {
            _outer[1].Age = _inner[0].Age;

            AssertAreEquivalent(_standardLinqResults, _target);
        }

        [Test]
        public void AddToInner_Always_UpdatesCollection()
        {
            Person newInner = new Person("New", _outer[0].Age);
            _inner.Insert(1, newInner);

            AssertAreEquivalent(_standardLinqResults, _target);
        }

        [Test]
        public void RemoveFromInner_Always_UpdatesCollection()
        {
            _inner.RemoveAt(0);

            AssertAreEquivalent(_standardLinqResults, _target);
        }

        [Test]
        public void ReplaceInInner_Always_UpdatesCollection()
        {
            Person newInner = new Person("New", _outer[0].Age);

            _inner[1] = newInner;

            AssertAreEquivalent(_standardLinqResults, _target);
        }

        [Test]
        public void ResetInInner_Always_UpdatesCollection()
        {
            _inner.Clear();

            AssertAreEquivalent(_standardLinqResults, _target);
        }

#if !SILVERLIGHT
        [Test]
        public void MoveInInner_Always_UpdatesCollection()
        {
            _inner.Move(1, 0);

            AssertAreEquivalent(_standardLinqResults, _target);
        }
#endif

        [Test]
        public void OnInnerKeyChanged_Always_UpdatesInnerResults()
        {
            _inner[0].Age = _inner[1].Age;

            AssertAreEquivalent(_standardLinqResults, _target);
        }

        [Test]
        public void Test()
        {
            Random rand = new Random();

            _outer.Clear();
            _inner.Clear();

            int outerItems = 500;
            int innerItems = 300000;


            for (int i = 0; i < innerItems; i++)
            {
                _inner.Add(new Person(i.ToString(), i % outerItems));
            }

            for (int i = 0; i < outerItems; i++)
            {
                _outer.Add(new Person(i.ToString(), i));
            }

            //for (int i = 0; i < outerItems; i++)
            //{
            //    _outer.Move(rand.Next(outerItems), rand.Next(outerItems));
            //}

            //for (int i = outerItems - 1; i >= 0; i--)
            //{
            //    if ((rand.Next() & 1) == 0)
            //    {
            //        _outer.RemoveAt(i);
            //    }
            //}


            //for (int i = 0; i < innerItems; i++)
            //{
            //    _inner.Move(rand.Next(innerItems), rand.Next(innerItems));
            //}

            //for (int i = innerItems - 1; i >= 0; i--)
            //{
            //    if ((rand.Next() & 1) == 0)
            //    {
            //        _inner.RemoveAt(i);
            //    }
            //}

            AssertAreEquivalent(_standardLinqResults, _target);
        }


        [Test]
        public void CreateQuery_AssignmentInExternalDelegate_PerformsAssignment()
        {
            Func<Person, ReadOnlyContinuousCollection<Person>, Person> personAssociation = (person, associatedPeople) =>
            {
                person.AssociatedPeople = associatedPeople;
                return person;
            };

            var target = from outerPerson in _outer
                         join innerPerson in _inner on outerPerson.Age equals innerPerson.Age into innersMatchingOuterAge
                         select personAssociation(outerPerson, innersMatchingOuterAge);

            CollectionAssert.Contains(_outer[0].AssociatedPeople, _inner[0]);
            CollectionAssert.Contains(_outer[1].AssociatedPeople, _inner[1]);
        }
    }
}
