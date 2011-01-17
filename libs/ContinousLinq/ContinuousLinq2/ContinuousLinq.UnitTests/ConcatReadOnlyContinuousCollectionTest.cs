using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ContinuousLinq.Collections;
using NUnit.Framework;
using System.Linq;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class ConcatReadOnlyContinuousCollectionTest
    {
        private ConcatReadOnlyContinuousCollection<Person> _target;
        private ObservableCollection<Person> _first;
        private ObservableCollection<Person> _second;

        private Person _person1;
        private Person _person2;

        [SetUp]
        public void Setup()
        {
            _first = ClinqTestFactory.CreateTwoPersonSource();
            _second = new ObservableCollection<Person>();

            _target = new ConcatReadOnlyContinuousCollection<Person>(_first, _second);

            _person1 = _first[0];
            _person2 = _first[1];
        }

        [Test]
        public void Construct_BothListsAreEmpty_OutputIsEmpty()
        {
            _first = new ObservableCollection<Person>();
            _second = new ObservableCollection<Person>();
            _target = new ConcatReadOnlyContinuousCollection<Person>(_first, _second);

            Assert.AreEqual(0, _target.Count);
        }

        [Test]
        public void Construct_SecondListIsEmpty_OutputIsFirstList()
        {
            CollectionAssert.AreEquivalent(_first, _target);
        }

        [Test]
        public void Construct_FirstListIsEmpty_OutputIsSecondList()
        {
            _first = new ObservableCollection<Person>();
            _second = ClinqTestFactory.CreateTwoPersonSource();

            _target = new ConcatReadOnlyContinuousCollection<Person>(_first, _second);

            CollectionAssert.AreEquivalent(_second, _target);
        }

        [Test]
        public void Construct_BothListsHaveValues_OutputHasItemsFromBothLists()
        {
            _second = ClinqTestFactory.CreateTwoPersonSource();
            _target = new ConcatReadOnlyContinuousCollection<Person>(_first, _second);

            var expectedConcatenation = ConcatenateFirstAndSecond();

            CollectionAssert.AreEquivalent(expectedConcatenation, _target);
        }

        [Test]
        public void AddItemToFirst_IsNotInSecond_WillBeAddedToOutput()
        {
            var newPerson = new Person { Name = "Frank" };
            _first.Add(newPerson);

            Assert.AreEqual(3, _target.Count);
            Assert.IsTrue(_target.Contains(newPerson));
        }

        [Test]
        public void AddItemToFirst_ItemIsDuplicateInFirst_WillBeAddedToOutput()
        {
            var newPerson = new Person { Name = "Frank" };
            _first.Add(newPerson);

            _first.Add(newPerson);

            CollectionAssert.AreEquivalent(_first, _target);
        }

        [Test]
        public void AddItemToFirst_ItemIsInSecond_WillBeAddedToOutputAgain()
        {
            var newPerson = new Person { Name = "Frank" };
            _second.Add(newPerson);

            _first.Add(newPerson);

            var expectedConcatenation = ConcatenateFirstAndSecond();

            CollectionAssert.AreEquivalent(expectedConcatenation, _target);
        }

        [Test]
        public void AddItemToSecond_IsNotInFirst_WillBeAddedToOutput()
        {
            var newPerson = new Person { Name = "Frank" };
            _second.Add(newPerson);

            Assert.AreEqual(3, _target.Count);
            Assert.IsTrue(_target.Contains(newPerson));
        }

        [Test]
        public void AddItemToSecond_ItemIsDuplicateInSecond_WillBeAddedToOutput()
        {
            var newPerson = new Person { Name = "Frank" };
            _second.Add(newPerson);

            _second.Add(newPerson);

            var expectedConcatenation = ConcatenateFirstAndSecond();

            CollectionAssert.AreEquivalent(expectedConcatenation, _target);
        }

        [Test]
        public void AddItemToSecond_ItemIsInFirst_WillBeAddedToOutputAgain()
        {
            var newPerson = new Person { Name = "Frank" };
            _first.Add(newPerson);

            _second.Add(newPerson);

            var expectedConcatenation = ConcatenateFirstAndSecond();

            CollectionAssert.AreEquivalent(expectedConcatenation, _target);
        }

        [Test]
        public void RemoveItemFromFirst_ItemIsDuplicateInFirstAndIsNotInSecond_WillBeRemovedFromOutputOnce()
        {
            var newPerson = new Person { Name = "Frank" };
            _first.Add(newPerson);
            _first.Add(newPerson);

            _first.Remove(newPerson);
            Assert.AreEqual(3, _target.Count);
            Assert.IsTrue(_target.Contains(newPerson));
        }

        [Test]
        public void RemoveItemFromFirst_IsNotInSecond_WillBeRemovedFromOutput()
        {
            var newPerson = new Person { Name = "Frank" };
            _first.Add(newPerson);

            _first.Remove(newPerson);
            Assert.AreEqual(2, _target.Count);
            Assert.IsFalse(_target.Contains(newPerson));
        }

        [Test]
        public void RemoveItemFromFirst_IsInSecond_WillBeRemovedFromOutputOnce()
        {
            var newPerson = new Person { Name = "Frank" };
            _first.Add(newPerson);
            _second.Add(newPerson);

            _first.Remove(newPerson);
            Assert.AreEqual(3, _target.Count);
            Assert.IsTrue(_target.Contains(newPerson));
        }

        [Test]
        public void RemoveItemFromSecond_ItemIsDuplicateInSecondAndIsNotInFirst_WillBeRemovedFromOutputOnce()
        {
            var newPerson = new Person { Name = "Frank" };
            _second.Add(newPerson);
            _second.Add(newPerson);

            _second.Remove(newPerson);
            Assert.AreEqual(3, _target.Count);
            Assert.IsTrue(_target.Contains(newPerson));
        }

        [Test]
        public void RemoveItemFromSecond_IsNotInFirst_WillBeRemovedFromOutput()
        {
            var newPerson = new Person { Name = "Frank" };
            _second.Add(newPerson);

            _second.Remove(newPerson);
            Assert.AreEqual(2, _target.Count);
            Assert.IsFalse(_target.Contains(newPerson));
        }

        [Test]
        public void RemoveItemFromSecond_IsInFirst_WillBeRemovedFromOutputOnce()
        {
            var newPerson = new Person { Name = "Frank" };
            _first.Add(newPerson);
            _second.Add(newPerson);

            _second.Remove(newPerson);
            Assert.AreEqual(3, _target.Count);
            Assert.IsTrue(_target.Contains(newPerson));
        }

        [Test]
        public void ReplaceItemInFirst_NewItemIsInSecond_AppearsInOutputTwice()
        {
            var newPerson = new Person { Name = "Frank" };

            _second.Add(newPerson);
            _first[0] = newPerson;

            var expectedConcatenation = ConcatenateFirstAndSecond();
            CollectionAssert.AreEquivalent(expectedConcatenation, _target);

            int timesInOutput = _target.Where(p => p == newPerson).Count();
            Assert.AreEqual(2, timesInOutput);
        }

        [Test]
        public void ReplaceItemInFirst_NewItemIsNotInSecond_AppearsInOutputOnce()
        {
            var newPerson = new Person { Name = "Frank" };
            _first[0] = newPerson;

            var expectedConcatenation = ConcatenateFirstAndSecond();
            CollectionAssert.AreEquivalent(expectedConcatenation, _target);

            int timesInOutput = _target.Where(p => p == newPerson).Count();
            Assert.AreEqual(1, timesInOutput);
        }

        [Test]
        public void ReplaceItemInSecond_NewItemIsInFirst_AppearsInOutputTwice()
        {
            var newPerson = new Person { Name = "Frank" };
            _second.Add(newPerson);
            _second[0] = _first[0];

            var expectedConcatenation = ConcatenateFirstAndSecond();
            CollectionAssert.AreEquivalent(expectedConcatenation, _target);

            int timesInOutput = _target.Where(p => p == _first[0]).Count();
            Assert.AreEqual(2, timesInOutput);
        }

        [Test]
        public void ReplaceItemInSecond_NewItemIsNotInFirst_AppearsInOutputOnce()
        {
            var newPerson = new Person { Name = "Frank" };
            _second.Add(_first[0]);

            _second[0] = newPerson;

            var expectedConcatenation = ConcatenateFirstAndSecond();
            CollectionAssert.AreEquivalent(expectedConcatenation, _target);

            int timesInOutput = _target.Where(p => p == newPerson).Count();
            Assert.AreEqual(1, timesInOutput);
        }

        [Test]
        public void ReplaceItemInFirst_RemovedItemWasDuplicateAndNewItemIsNotInSecond_IsInOutputOnce()
        {
            _first[1] = _person1;

            _first[1] = _person2;

            Assert.AreEqual(2, _target.Count);
            Assert.IsTrue(_target.Contains(_person1));
            Assert.IsTrue(_target.Contains(_person2));
        }

        [Test]
        public void ReplaceItemInFirst_RemovedItemWasDuplicateAndNewItemIsInSecond_IsInOutputTwice()
        {
            _second.Add(_person2);
            _first[1] = _person1;

            _first[1] = _person2;

            var expectedConcatenation = ConcatenateFirstAndSecond();
            CollectionAssert.AreEquivalent(expectedConcatenation, _target);

            int timesInOutput = _target.Where(p => p == _person2).Count();
            Assert.AreEqual(2, timesInOutput);
        }

        [Test]
        public void ReplaceItemInSecond_RemovedItemWasDuplicateAndNewItemIsNotInFirst_IsInOutputOnce()
        {
            var oldPerson = new Person { Name = "Mark" };
            _second.Add(oldPerson);
            _second.Add(oldPerson);

            var newPerson = new Person { Name = "Frank" };
            _second[1] = newPerson;

            var expectedConcatenation = ConcatenateFirstAndSecond();
            CollectionAssert.AreEquivalent(expectedConcatenation, _target);

            int timesInOutput = _target.Where(p => p == newPerson).Count();
            Assert.AreEqual(1, timesInOutput);
        }

        [Test]
        public void ReplaceItemOnSecond_RemovedItemWasDuplicateAndNewItemIsInFirst_IsInOutputTwice()
        {
            _second.Add(_person2);
            _second.Add(_person2);

            _second[1] = _person1;

            var expectedConcatenation = ConcatenateFirstAndSecond();
            CollectionAssert.AreEquivalent(expectedConcatenation, _target);

            int timesInOutput = _target.Where(p => p == _person1).Count();
            Assert.AreEqual(2, timesInOutput);
        }

        [Test]
        public void ResetFirst_Always_LeavesOutputIntact()
        {
            var people = ClinqTestFactory.CreateSixPersonSource();

            var first = new TestContinuousCollection<Person>(people.ToList());
            var second = new TestContinuousCollection<Person> { people[0] };
            _target = new ConcatReadOnlyContinuousCollection<Person>(first, second);

            first.FireReset();

            Assert.AreEqual(7, _target.Count);
            var expectedConcatenation = Concatenate(first, second);
            CollectionAssert.AreEquivalent(expectedConcatenation, _target);
        }

        [Test]
        public void ResetSecond_Always_LeavesOutputIntact()
        {
            var people = ClinqTestFactory.CreateSixPersonSource();

            var first = new TestContinuousCollection<Person>(people.ToList());
            var second = new TestContinuousCollection<Person> { people[0] };
            _target = new ConcatReadOnlyContinuousCollection<Person>(first, second);

            second.FireReset();

            Assert.AreEqual(7, _target.Count);
            var expectedConcatenation = Concatenate(first, second);
            CollectionAssert.AreEquivalent(expectedConcatenation, _target);
        }

        [Test]
        public void ResetOnFirst_Always_RaisesNotifyCollectionChangedWithResetAction()
        {
            var people = ClinqTestFactory.CreateSixPersonSource();

            var first = new TestContinuousCollection<Person>(people.ToList());
            var second = new TestContinuousCollection<Person> { people[0] };
            _target = new ConcatReadOnlyContinuousCollection<Person>(first, second);

            var eventArgsList = new List<NotifyCollectionChangedEventArgs>();
            _target.CollectionChanged += (sender, e) => eventArgsList.Add(e);

            first.FireReset();

            Assert.AreEqual(1, eventArgsList.Count);
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, eventArgsList[0].Action);
        }

        [Test]
        public void ResetOnSecond_Always_RaisesNotifyCollectionChangedWithResetAction()
        {
            var people = ClinqTestFactory.CreateSixPersonSource();

            var first = new TestContinuousCollection<Person>(people.ToList());
            var second = new TestContinuousCollection<Person> { people[0] };
            _target = new ConcatReadOnlyContinuousCollection<Person>(first, second);

            var eventArgsList = new List<NotifyCollectionChangedEventArgs>();
            _target.CollectionChanged += (sender, e) => eventArgsList.Add(e);

            second.FireReset();

            Assert.AreEqual(1, eventArgsList.Count);
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, eventArgsList[0].Action);
        }

        [Test]
        public void ClearOnFirst_Always_MakesOutputEqualToSecond()
        {
            var newPerson = new Person { Name = "Frank" };
            _second.Add(newPerson);

            _first.Clear();

            CollectionAssert.AreEquivalent(_second, _target);
        }

        [Test]
        public void ClearOnSecond_Always_MakesOutputEqualToFirst()
        {
            _second.Add(_person1);
            _second.Add(_person2);

            _second.Clear();

            CollectionAssert.AreEquivalent(_first, _target);
        }

        [Test]
        public void ClearOnFirst_Always_RaisesNotifyCollectionChangedWithResetAction()
        {
            var eventArgsList = TestUtilities.GetCollectionChangedEventArgsList(_target);

            _first.Clear();

            Assert.AreEqual(1, eventArgsList.Count);

            TestUtilities.AssertReset(eventArgsList[0]);
        }

        [Test]
        public void ClearOnSecond_Always_RaisesNotifyCollectionChangedWithResetAction()
        {
            _second.Add(_person1);

            var eventArgsList = TestUtilities.GetCollectionChangedEventArgsList(_target);

            _second.Clear();

            Assert.AreEqual(1, eventArgsList.Count);

            TestUtilities.AssertReset(eventArgsList[0]);
        }

        [Test]
        public void AddToFirst_Always_RaisesNotifyCollectionChangedWithAddActionAndCorrectValues()
        {
            var person = new Person();

            var eventArgsList = TestUtilities.GetCollectionChangedEventArgsList(_target);

            _first.Add(person);

            Assert.AreEqual(1, eventArgsList.Count);

            TestUtilities.AssertAdd(eventArgsList[0], 2, person);
        }

#if !SILVERLIGHT
        [Test]
        public void AddRangeToFirst_Always_RaisesNotifyCollectionChangedWithAddActionAndCorrectValues()
        {
            var continuousFirstCollection = new ContinuousCollection<Person>(_first.ToList());
            _target = new ConcatReadOnlyContinuousCollection<Person>(continuousFirstCollection, _second);

            var people = new List<Person> { new Person(), new Person() };

            var eventArgsList = TestUtilities.GetCollectionChangedEventArgsList(_target);

            continuousFirstCollection.AddRange(people);

            Assert.AreEqual(1, eventArgsList.Count);

            TestUtilities.AssertAdd(eventArgsList[0], 2, people.ToArray());
        }
#endif

        [Test]
        public void RemoveFromFirst_Always_RaisesNotifyCollectionChangedWithRemoveActionAndCorrectValues()
        {
            var eventArgsList = TestUtilities.GetCollectionChangedEventArgsList(_target);

            _first.Remove(_person1);

            Assert.AreEqual(1, eventArgsList.Count);

            TestUtilities.AssertRemove(eventArgsList[0], 0, _person1);
        }

#if !SILVERLIGHT
        [Test]
        public void RemoveRangeFromFirst_Always_RaisesNotifyCollectionChangedWithRemoveActionAndCorrectValues()
        {
            var continuousFirstCollection = new ContinuousCollection<Person>(_first.ToList());
            _target = new ConcatReadOnlyContinuousCollection<Person>(continuousFirstCollection, _second);

            var eventArgsList = TestUtilities.GetCollectionChangedEventArgsList(_target);

            continuousFirstCollection.RemoveRange(0, 2);

            Assert.AreEqual(1, eventArgsList.Count);

            TestUtilities.AssertRemove(eventArgsList[0], 0, _person1, _person2);
        }
#endif

        [Test]
        public void ReplaceOnFirst_Always_RaisesNotifyCollectionChangedWithReplaceActionAndCorrectValues()
        {
            var person = new Person();

            var eventArgsList = TestUtilities.GetCollectionChangedEventArgsList(_target);

            _first[0] = person;

            Assert.AreEqual(1, eventArgsList.Count);

            TestUtilities.AssertReplace(eventArgsList[0], 0, new[] { person }, new[] { _person1 });
        }

#if !SILVERLIGHT
        [Test]
        public void ReplaceRangeOnFirst_Always_RaisesNotifyCollectionChangedWithReplaceAction()
        {
            var continuousFirstCollection = new ContinuousCollection<Person>(_first.ToList());
            _target = new ConcatReadOnlyContinuousCollection<Person>(continuousFirstCollection, _second);

            var people = new List<Person> { new Person(), new Person() };

            var eventArgsList = TestUtilities.GetCollectionChangedEventArgsList(_target);

            continuousFirstCollection.ReplaceRange(0, people);

            Assert.AreEqual(1, eventArgsList.Count);

            TestUtilities.AssertReplace(eventArgsList[0], 0, people.ToArray(), new[] { _person1, _person2 });
        }
#endif

        [Test]
        public void AddToSecond_Always_RaisesNotifyCollectionChangedWithAddActionAndCorrectValues()
        {
            var eventArgsList = TestUtilities.GetCollectionChangedEventArgsList(_target);

            _second.Add(_person1);

            Assert.AreEqual(1, eventArgsList.Count);

            TestUtilities.AssertAdd(eventArgsList[0], 2, _person1);
        }

#if !SILVERLIGHT
        [Test]
        public void AddRangeToSecond_Always_RaisesNotifyCollectionChangedWithAddActionAndCorrectValues()
        {
            var continuousSecondCollection = new ContinuousCollection<Person>();
            _target = new ConcatReadOnlyContinuousCollection<Person>(_first, continuousSecondCollection);

            var people = new List<Person> { new Person(), new Person() };

            var eventArgsList = TestUtilities.GetCollectionChangedEventArgsList(_target);

            continuousSecondCollection.AddRange(people);

            Assert.AreEqual(1, eventArgsList.Count);

            TestUtilities.AssertAdd(eventArgsList[0], 2, people.ToArray());
        }
#endif

        [Test]
        public void RemoveFromSecond_Always_RaisesNotifyCollectionChangedWithRemoveActionAndCorrectValues()
        {
            _second.Add(_person1);

            var eventArgsList = TestUtilities.GetCollectionChangedEventArgsList(_target);

            _second.Remove(_person1);

            Assert.AreEqual(1, eventArgsList.Count);

            TestUtilities.AssertRemove(eventArgsList[0], 2, _person1);
        }

#if !SILVERLIGHT
        [Test]
        public void RemoveRangeFromSecond_Always_RaisesNotifyCollectionChangedWithRemoveActionAndCorrectValuesForEachItemRemoved()
        {
            var continuousSecondCollection = new ContinuousCollection<Person>(_first.ToList());
            _target = new ConcatReadOnlyContinuousCollection<Person>(_first, continuousSecondCollection);

            var eventArgsList = TestUtilities.GetCollectionChangedEventArgsList(_target);

            continuousSecondCollection.RemoveRange(0, 2);

            Assert.AreEqual(1, eventArgsList.Count);

            TestUtilities.AssertRemove(eventArgsList[0], 2, _person1, _person2);
        }
#endif

        [Test]
        public void ReplaceOnSecond_Always_RaisesNotifyCollectionChangedWithReplaceAction()
        {
            _second.Add(_person1);

            var eventArgsList = TestUtilities.GetCollectionChangedEventArgsList(_target);

            var person = new Person { Name = "Frank" };
            _second[0] = person;

            Assert.AreEqual(1, eventArgsList.Count);

            TestUtilities.AssertReplace(eventArgsList[0], 2, new[] { person }, new[] { _person1 });
        }

#if !SILVERLIGHT
        [Test]
        public void ReplaceRangeOnSecond_Always_RaisesNotifyCollectionChangedWithReplaceAction()
        {
            var continuousSecondCollection = new ContinuousCollection<Person>(_first.ToList());
            _target = new ConcatReadOnlyContinuousCollection<Person>(_first, continuousSecondCollection);

            var people = new List<Person> { new Person(), new Person() };

            var eventArgsList = TestUtilities.GetCollectionChangedEventArgsList(_target);

            continuousSecondCollection.ReplaceRange(0, people);

            Assert.AreEqual(1, eventArgsList.Count);

            TestUtilities.AssertReplace(eventArgsList[0], 2, people.ToArray(), new[] { _person1, _person2 });
        }
#endif

        private IEnumerable<Person> ConcatenateFirstAndSecond()
        {
            return Concatenate(_first.AsEnumerable(), _second.AsEnumerable());
        }

        private static IEnumerable<T> Concatenate<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            return first.Concat(second);
        }
    }
}