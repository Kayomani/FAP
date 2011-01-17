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
    public class FilteringReadOnlyContinuousCollectionTest
    {
        private FilteringReadOnlyContinuousCollection<Person> _target;

        ObservableCollection<Person> _source;

        [SetUp]
        public void Setup()
        {
            _source = ClinqTestFactory.CreateTwoPersonSource();
            _target = new FilteringReadOnlyContinuousCollection<Person>(
                _source,
                p => p.Age > 10);
        }

        [Test]
        public void IndexerGet_ItemsInSource_ItemsMatchSelection()
        {
            Assert.AreEqual(_source[1], _target[0]);
        }

        [Test]
        public void Count_OneItemPassingFilter_CountIsOne()
        {
            Assert.AreEqual(1, _target.Count);
        }

        [Test]
        public void AddItemToSource_ItemPassesFilter_FireCollectionChangedEvent()
        {
            Person newPerson = new Person() { Name = "NewPerson", Age = 100 };
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
                Assert.AreEqual(newPerson, args.NewItems[0]);
            };

            _source.Add(newPerson);
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void AddItemToSource_ItemFailsFilter_DoesNotFireCollectionChangedEvent()
        {
            Person newPerson = new Person() { Name = "NewPerson", Age = 8 };
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
            };

            _source.Add(newPerson);
            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void RemoveItemFromSource_ItemWasPassingFilter_FireCollectionChangedEvent()
        {
            Person personToRemove = _source[1];
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
                Assert.AreEqual(personToRemove, args.OldItems[0]);
            };

            _source.Remove(personToRemove);
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void RemoveItemFromSource_ItemWasFailingFilter_DoesNotFireCollectionChangedEvent()
        {
            Person personToRemove = _source[0];
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
            };

            _source.Remove(personToRemove);
            Assert.AreEqual(0, callCount);
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
            Assert.AreEqual(0, _target.Count);
        }

#if !SILVERLIGHT
        [Test]
        public void MoveItemsInSource_Never_FireCollectionChangedEvent()
        {
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
            };

            _source.Move(0, 1);

            Assert.AreEqual(0, callCount);
        }
#endif

        [Test]
        public void ReplaceItemsInSource_ItemPassesFilter_FiresRemoveAndAddCollectionChangedEvents()
        {
            Person oldPerson = _source[1];
            Person newPerson = new Person() { Name = "NewPerson", Age = 1000 };

            int addCount = 0;
            int removeCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                	addCount++;
                    Assert.AreEqual(newPerson, args.NewItems[0]);
                }

                if (args.Action == NotifyCollectionChangedAction.Remove)
                {
                    removeCount++;
                    Assert.AreEqual(oldPerson, args.OldItems[0]);
                }
            };

            _source[1] = newPerson;

            Assert.AreEqual(1, addCount);
            Assert.AreEqual(1, removeCount);
        }

        [Test]
        public void ChangeMonitoredPropertyOnItemInSource_ItemNowPassesFilter_ItemAddedToOutputCollection()
        {
            _source[0].Age = 12321;

            Assert.AreEqual(2, _target.Count);
            Assert.IsTrue(_target.Contains(_source[0]));
        }

        [Test]
        public void Where_AllFalseThenMadeAllTrue_ExpectedResult()
        {
            for (int i = 0; i < _source.Count; i++)
            {
                _source[i].Age = 100;
            }

            var result = _source.Where(p => p.Age >= 100);

            Assert.AreEqual(2, result.Count);

            for (int i = 0; i < _source.Count; i++)
            {
                _source[i].Age = 0;
            }
            
            Assert.AreEqual(0, result.Count);

            for (int i = 0; i < _source.Count; i++)
            {
                _source[i].Age = 100;
            }

            Assert.AreEqual(2, result.Count);
        }
    }
}
