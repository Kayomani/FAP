using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections.ObjectModel;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class NotifyCollectionChangedMonitorTest
    {
        private NotifyCollectionChangedMonitor<Person> _target;
        private ObservableCollection<Person> _source;

        private PropertyAccessTree _propertyAccessTree;
        
        [SetUp]
        public void Setup()
        {
            _source = ClinqTestFactory.CreateTwoPersonSource();

            _propertyAccessTree = new PropertyAccessTree();
            ParameterNode parameterNode = new ParameterNode(typeof(Person), "person");
            _propertyAccessTree.Children.Add(parameterNode);

            var agePropertyAccessNode = new PropertyAccessNode(typeof(Person).GetProperty("Age"));
            parameterNode.Children.Add(agePropertyAccessNode);

            _target = new NotifyCollectionChangedMonitor<Person>(_propertyAccessTree, _source);
        }

        [Test]
        public void AddToSource_SingleItem_FiresAdd()
        {
            int callCount = 0;
            _target.Add += (sender, index, items) =>
            {
                callCount++;
                Assert.AreEqual(2, index);
                Assert.IsNotNull(items);
            };

            Person newPerson = new Person("New", 100);
            _source.Add(newPerson);
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void RemoveFromSource_SingleItem_FiresRemove()
        {
            int callCount = 0;
            _target.Remove += (sender, index, items) =>
            {
                callCount++;
                Assert.AreEqual(1, index);
                Assert.IsNotNull(items);
            };

            _source.RemoveAt(1);
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void ReplaceInSource_SingleItem_FiresReplace()
        {
            int callCount = 0;
            _target.Replace += (sender, oldItems, newIndex, newItems) =>
            {
                callCount++;
                Assert.IsNotNull(oldItems);
                Assert.AreEqual(1, oldItems.Count());
                
                Assert.AreEqual(0, newIndex);
                Assert.IsNotNull(newItems);
                Assert.AreEqual(1, newItems.Count());
            };

            _source[0] = new Person();
            Assert.AreEqual(1, callCount);
        }

        #if !SILVERLIGHT
        [Test]
        public void MoveInSource_SingleItem_FiresMove()
        {
            int callCount = 0;
            _target.Move += (sender, oldIndex, oldItems, newIndex, newItems) =>
            {
                callCount++;
                Assert.AreEqual(0, oldIndex);
                Assert.IsNotNull(oldItems);
                Assert.AreEqual(1, oldItems.Count());

                Assert.AreEqual(1, newIndex);
                Assert.IsNotNull(newItems);
                Assert.AreEqual(1, newItems.Count());
            };

            _source.Move(0, 1);
            Assert.AreEqual(1, callCount);
        }
        #endif

        [Test]
        public void InsertInSource_SingleItem_FiresReplace()
        {
            int callCount = 0;
            _target.Add += (sender, index, items) =>
            {
                callCount++;
            };

            _source.Insert(0, new Person());
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void ClearSource_SingleItem_FiresReset()
        {
            int callCount = 0; 
            _target.Reset += (sender) =>
            {
                callCount++;
            };

            _source.Clear();
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void SetPropertyOnItemInSourceCollection_PropertyDifferent_FiresItemChangedEvent()
        {
            int callCount = 0;
            _target.ItemChanged += (sender, item) =>
            {
                callCount++;
                Assert.AreSame(_source[0], item);
            };

            _source[0].Age = 1000;
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void RemoveFromSourceAndChangePropertyOnItem_SingleItem_ItemChangedNotFired()
        {
            Person person = _source[0];

            _target.ItemChanged += (sender, item) => Assert.Fail();

            _source.Remove(person);
            person.Age = 1000;
        }

        [Test]
        public void ReplaceItemInSourceAndChangePropertyOnItem_SingleItem_ItemChangedNotFired()
        {
            Person person = _source[0];

            _target.ItemChanged += (sender, item) => Assert.Fail();

            _source[0] = new Person();
            person.Age = 1000;
        }

        [Test]
        public void ReplaceItemInSourceAndChangePropertyOnNewItem_SingleItem_ItemChangedFired()
        {
            int callCount = 0;
            _target.ItemChanged += (sender, item) => callCount++;

            Person newPerson = new Person();
            
            _source[0] = newPerson;
            newPerson.Age = 1000;

            Assert.AreEqual(1, callCount);
        }

#if !SILVERLIGHT
        [Test]
        public void MoveItemInSourceAndChangePropertyOnItem_SingleItem_ItemChangedNotFired()
        {
            Person person = _source[0];

            int callCount = 0;
            _target.ItemChanged += (sender, item) => callCount++;

            _source.Move(0, 1);
            person.Age = 1000;
            
            Assert.AreEqual(1, callCount);
        }
#endif

        [Test]
        public void ClearSourceAndChangePropertyOnItem_SingleItem_ItemChangedNotFired()
        {
            Person person = _source[0];

            _target.ItemChanged += (sender, item) => Assert.Fail();

            _source.Clear();
            person.Age = 1000;
        }

        [Test]
        public void DuplicateInSource_ChangePropertyOnItem_OnlyNotifiedOnce()
        {
            Person person = _source[0];
            
            int callCount = 0;
            _target.ItemChanged += (sender, item) => callCount++;

            _source.Add(person);

            person.Age++;

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void DuplicateInSource_AddAndThenRemove_OnlyNotifiedOnce()
        {
            Person person = _source[0];

            int callCount = 0;
            _target.ItemChanged += (sender, item) => callCount++;

            _source.Add(person);
            _source.Remove(person);

            person.Age++;

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void DuplicateInSource_AddAndThenRemoveBoth_NotNotified()
        {
            Person person = _source[0];

            int callCount = 0;
            _target.ItemChanged += (sender, item) => callCount++;

            _source.Add(person);
            _source.Remove(person);
            _source.Remove(person);

            person.Age++;

            Assert.AreEqual(0, callCount);
        }
    }
}
