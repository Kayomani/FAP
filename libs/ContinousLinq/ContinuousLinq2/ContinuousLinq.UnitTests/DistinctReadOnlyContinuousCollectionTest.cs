using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ContinuousLinq.Collections;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class DistinctReadOnlyContinuousCollectionTest
    {
        private DistinctReadOnlyContinuousCollection<Person> _target;
        private ObservableCollection<Person> _source;

        [SetUp]
        public void Setup()
        {
            _source = ClinqTestFactory.CreateSixPersonSourceWithDuplicates();
            _target = new DistinctReadOnlyContinuousCollection<Person>(_source);
        }

        [Test]
        public void Construct_SourceHasDuplicates_ContainsOnlyThreeItems()
        {
            Assert.AreEqual(3, _target.Count);
        }

        [Test]
        public void Indexer_SourceHasDuplicates_ContainsDistinctItems()
        {
            Assert.AreEqual(_source[0], _target[0]);
            Assert.AreEqual(_source[3], _target[1]);
            Assert.AreEqual(_source[4], _target[2]);
        }

        [Test]
        public void AddItemToSource_NewPerson_FireCollectionChangedEvent()
        {
            Person person = new Person();
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
                Assert.AreEqual(3, args.NewStartingIndex);
                Assert.AreEqual(person, args.NewItems[0]);
            };

            _source.Add(person);
            Assert.AreEqual(1, callCount);
        }


        [Test]
        public void AddItemToSource_ItemAlreadyInSource_DoesNotFireCollectionChangedEvent()
        {
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
            };

            _source.Add(_source[0]);
            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void RemoveItemFromSource_LastInstanceOfItemInSource_FireCollectionChangedEvent()
        {
            Person person = _source[3];

            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
                Assert.AreEqual(1, args.OldStartingIndex);
                Assert.AreEqual(person, args.OldItems[0]);
            };

            _source.Remove(person);
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void RemoveItemFromSource_NotLastItemInSource_DoesNotFireCollectionChangedEvent()
        {
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
            };

            _source.Remove(_source[0]);
            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void ReplaceItemInSource_LastInstanceOfItemInSource_RemoveCollectionChangedEventFired()
        {
            Person person = new Person();
            Person oldPerson = _source[3];
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Remove)
                {
                    callCount++;
                    Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
                    Assert.AreEqual(1, args.OldStartingIndex);
                    Assert.AreEqual(oldPerson, args.OldItems[0]);
                }
            };

            _source[3] = person;
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void ReplaceItemInSource_NewInstanceOfItemInSource_AddCollectionChangedEventFired()
        {
            Person person = new Person();
  
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    callCount++;
                    Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
                    Assert.AreEqual(3, args.NewStartingIndex);
                    Assert.AreEqual(person, args.NewItems[0]);
                }
            };

            _source[3] = person;
            Assert.AreEqual(1, callCount);
        }
    }
}
