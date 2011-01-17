using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class SelectReadOnlyContinuousCollectionTest
    {
        private SelectReadOnlyContinuousCollection<Person, string> _target;

        ObservableCollection<Person> _source;

        [SetUp]
        public void Setup()
        {
            _source = ClinqTestFactory.CreateTwoPersonSource();
            _target = new SelectReadOnlyContinuousCollection<Person, string>(
                _source,
                p => p.Name);
        }

        [Test]
        public void IndexerGet_ItemsInSource_ItemsMatchSelection()
        {
            Assert.AreEqual("Bob", _target[0]);
            Assert.AreEqual("Jim", _target[1]);
        }

        [Test]
        public void AddItemToSource_Always_FireCollectionChangedEvent()
        {
            Person newPerson = new Person() { Name = "NewPerson" };
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
                Assert.AreEqual(2, args.NewStartingIndex);
                Assert.AreEqual("NewPerson", args.NewItems[0]);
            };

            _source.Add(newPerson);
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void RemoveItemFromSource_Always_FireCollectionChangedEvent()
        {
            Person personToRemove = _source[0];
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
                Assert.AreEqual(0, args.OldStartingIndex);
                Assert.AreEqual("Bob", args.OldItems[0]);
            };

            _source.Remove(personToRemove);
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void ClearSource_Always_FireCollectionChangedEvent()
        {
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Reset, args.Action);
                Assert.IsNull(args.OldItems);
                Assert.IsNull(args.NewItems);
            };

            _source.Clear();
            Assert.AreEqual(1, callCount);
        }
#if !SILVERLIGHT
        [Test]
        public void MoveItemsInSource_Always_FireCollectionChangedEvent()
        {
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Move, args.Action);
                Assert.AreEqual(1, args.NewStartingIndex);
                Assert.AreEqual(0, args.OldStartingIndex);
                Assert.IsTrue(args.NewItems.Contains("Bob"));
                Assert.AreEqual(args.NewItems, args.OldItems);
            };

            _source.Move(0, 1);

            Assert.AreEqual(1, callCount);
        }

#endif
        [Test]
        public void ReplaceItemsInSource_Always_FireCollectionChangedEvent()
        {
            Person newPerson = new Person() { Name = "NewPerson" };

            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Replace, args.Action);
                Assert.AreEqual(0, args.NewStartingIndex);
                Assert.IsTrue(args.NewItems.Contains("NewPerson"));
                Assert.IsTrue(args.OldItems.Contains("Bob"));
            };

            _source[0] = newPerson;

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void ChangeMonitoredPropertyOnItemInSource_Always_FireCollectionChangedEvent()
        {
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Replace, args.Action);
                Assert.AreEqual(0, args.NewStartingIndex);
                Assert.IsTrue(args.NewItems.Contains("DifferentName"));
                Assert.IsTrue(args.OldItems.Contains("Bob"));
            };

            _source[0].Name = "DifferentName";
            Assert.AreEqual(1, callCount);
        }
    }


}
