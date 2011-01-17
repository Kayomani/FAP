using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections.ObjectModel;
using ContinuousLinq;
using System.Collections.Specialized;
using System.Collections;

namespace ContinuousLinq.UnitTests
{

    [TestFixture]
    public class SelectManyReadOnlyContinuousCollectionTest
    {
        private ReadOnlyContinuousCollection<Person> _target;
        private ObservableCollection<ObservableCollection<Person>> _parents;

        ObservableCollection<Person> _source;

        [SetUp]
        public void Setup()
        {
            _source = ClinqTestFactory.CreateTwoPersonSourceWithParents();

            _parents = new ObservableCollection<ObservableCollection<Person>>()
            {
                _source[0].Parents,
                _source[1].Parents
            };

            _target = _source.SelectMany(src => src.Parents);
        }

        [Test]
        public void IndexerGet_ItemsInSource_ItemsMatchSelection()
        {
            Assert.AreEqual("BobParent0", _target[0].Name);
            Assert.AreEqual("BobParent1", _target[1].Name);
            Assert.AreEqual("JimParent0", _target[2].Name);
            Assert.AreEqual("JimParent1", _target[3].Name);
        }

        [Test]
        public void Count_ItemsInSource_IsTotalOfAllSubcollections()
        {
            Assert.AreEqual(4, _target.Count);
        }

        [Test]
        public void AddItemToSource_FirstSublist_FireCollectionChangedEvent()
        {
            Person newPerson = new Person() { Name = "NewPerson", Age = 5 };
            int callCount = 0;

            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
                Assert.AreEqual(2, args.NewStartingIndex);
                Assert.AreEqual(newPerson, args.NewItems[0]);
            };

            _parents[0].Add(newPerson);
            Assert.AreEqual(1, callCount);

            Assert.AreEqual(5, _target.Count);
            Assert.AreEqual("BobParent0", _target[0].Name);
            Assert.AreEqual("BobParent1", _target[1].Name);
            Assert.AreEqual("NewPerson", _target[2].Name);
            Assert.AreEqual("JimParent0", _target[3].Name);
            Assert.AreEqual("JimParent1", _target[4].Name);
        }

        [Test]
        public void AddItemToSource_SecondSublist_FireCollectionChangedEvent()
        {
            Person newPerson = new Person() { Name = "NewPerson", Age = 5 };
            int callCount = 0;

            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
                Assert.AreEqual(4, args.NewStartingIndex);
                Assert.AreEqual(newPerson, args.NewItems[0]);
            };

            _parents[1].Add(newPerson);
            Assert.AreEqual(1, callCount);

            Assert.AreEqual(5, _target.Count);
            Assert.AreEqual("BobParent0", _target[0].Name);
            Assert.AreEqual("BobParent1", _target[1].Name);
            Assert.AreEqual("JimParent0", _target[2].Name);
            Assert.AreEqual("JimParent1", _target[3].Name);
            Assert.AreEqual("NewPerson", _target[4].Name);
        }

        [Test]
        public void RemoveItemFromSource_FirstSublist_FireCollectionChangedEvent()
        {
            Person personToRemove = _parents[0][1];
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
                Assert.AreEqual(1, args.OldStartingIndex);
                Assert.AreEqual(personToRemove, args.OldItems[0]);
            };

            _parents[0].Remove(personToRemove);
            Assert.AreEqual(1, callCount);
            Assert.AreEqual(3, _target.Count);
        }

        [Test]
        public void RemoveItemFromSource_SecondSublist_FireCollectionChangedEvent()
        {
            Person personToRemove = _parents[1][1];
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
                Assert.AreEqual(3, args.OldStartingIndex);
                Assert.AreEqual(personToRemove, args.OldItems[0]);
            };

            _parents[1].Remove(personToRemove);
            Assert.AreEqual(1, callCount);
            Assert.AreEqual(3, _target.Count);
            Assert.AreEqual("BobParent0", _target[0].Name);
            Assert.AreEqual("BobParent1", _target[1].Name);
            Assert.AreEqual("JimParent0", _target[2].Name);
        }

#if !SILVERLIGHT
        [Test]
        public void MoveItemInSource_InFirstSublist_FireCollectionChangedEvent()
        {
            Person personToMove = _parents[0][0];
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Move, args.Action);
                Assert.AreEqual(0, args.OldStartingIndex);
                Assert.AreEqual(1, args.NewStartingIndex);
                Assert.AreEqual(personToMove, args.OldItems[0]);
                Assert.AreEqual(personToMove, args.NewItems[0]);
            };

            _parents[0].Move(0, 1);
            Assert.AreEqual(1, callCount);

            Assert.AreEqual(4, _target.Count);
            Assert.AreEqual("BobParent1", _target[0].Name);
            Assert.AreEqual("BobParent0", _target[1].Name);
            Assert.AreEqual("JimParent0", _target[2].Name);
            Assert.AreEqual("JimParent1", _target[3].Name);
        }
#endif

#if !SILVERLIGHT
        [Test]
        public void MoveItemInSource_InSecond_FireCollectionChangedEvent()
        {
            Person personToMove = _parents[1][0];
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Move, args.Action);
                Assert.AreEqual(2, args.OldStartingIndex);
                Assert.AreEqual(3, args.NewStartingIndex);
                Assert.AreEqual(personToMove, args.OldItems[0]);
                Assert.AreEqual(personToMove, args.NewItems[0]);
            };

            _parents[1].Move(0, 1);
            Assert.AreEqual(1, callCount);

            Assert.AreEqual(4, _target.Count);
            Assert.AreEqual("BobParent0", _target[0].Name);
            Assert.AreEqual("BobParent1", _target[1].Name);
            Assert.AreEqual("JimParent1", _target[2].Name);
            Assert.AreEqual("JimParent0", _target[3].Name);
        }
#endif

        [Test]
        public void ReplaceItemInSource_FirstCollection_FireCollectionChangedEvent()
        {
            Person replacement = new Person("Replacement", 7);
            Person oldValue = _parents[0][1];

            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Replace, args.Action);
                Assert.AreEqual(1, args.NewStartingIndex);
                Assert.AreEqual(oldValue, args.OldItems[0]);
                Assert.AreEqual(replacement, args.NewItems[0]);
            };

            _parents[0][1] = replacement;

            Assert.AreEqual(1, callCount);

            Assert.AreEqual(4, _target.Count);
            Assert.AreEqual("BobParent0", _target[0].Name);
            Assert.AreEqual("Replacement", _target[1].Name);
            Assert.AreEqual("JimParent0", _target[2].Name);
            Assert.AreEqual("JimParent1", _target[3].Name);
        }

        [Test]
        public void ReplaceItemInSource_SecondCollection_FireCollectionChangedEvent()
        {
            Person replacement = new Person("Replacement", 7);
            Person oldValue = _parents[1][0];

            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Replace, args.Action);
                Assert.AreEqual(2, args.NewStartingIndex);
                Assert.AreEqual(oldValue, args.OldItems[0]);
                Assert.AreEqual(replacement, args.NewItems[0]);
            };

            _parents[1][0] = replacement;

            Assert.AreEqual(1, callCount);

            Assert.AreEqual(4, _target.Count);
            Assert.AreEqual("BobParent0", _target[0].Name);
            Assert.AreEqual("BobParent1", _target[1].Name);
            Assert.AreEqual("Replacement", _target[2].Name);
            Assert.AreEqual("JimParent1", _target[3].Name);

        }

        [Test]
        public void ResetSource_FirstSublist_FireCollectionChangedEvent()
        {
            List<Person> oldValues = new List<Person>(_parents[0]);
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Reset, args.Action);
            };

            _parents[0].Clear();
            Assert.AreEqual(1, callCount);

            Assert.AreEqual(2, _target.Count);
        }

        [Test]
        public void ResetSource_SecondSublist_FireCollectionChangedEvent()
        {
            List<Person> oldValues = new List<Person>(_parents[1]);
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Reset, args.Action);
            };

            _parents[1].Clear();
            Assert.AreEqual(1, callCount);

            Assert.AreEqual(2, _target.Count);
        }

#if SILVERLIGHT
        [Test]
        public void AddNewSublistToSource_Always_FireCollectionChangedEvent()
        {
            Person newPerson = new Person("Ninja", 23);
            ClinqTestFactory.InitializeParents(newPerson);

            int callCount = 0;

            _target.CollectionChanged += (sender, args) =>
            {
                Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
                Assert.AreEqual(2 + callCount, args.NewStartingIndex);
                Assert.AreEqual(newPerson.Parents[callCount], args.NewItems[0]);
                callCount++;
            };

            _source.Insert(1, newPerson);
            Assert.AreEqual(2, callCount);

            Assert.AreEqual(6, _target.Count);
            Assert.AreEqual("BobParent0", _target[0].Name);
            Assert.AreEqual("BobParent1", _target[1].Name);
            Assert.AreEqual("NinjaParent0", _target[2].Name);
            Assert.AreEqual("NinjaParent1", _target[3].Name);
            Assert.AreEqual("JimParent0", _target[4].Name);
            Assert.AreEqual("JimParent1", _target[5].Name);
        }

#else
        [Test]
        public void AddNewSublistToSource_Always_FireCollectionChangedEvent()
        {
            Person newPerson = new Person("Ninja", 23);
            ClinqTestFactory.InitializeParents(newPerson);

            int callCount = 0;

            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
                Assert.AreEqual(2, args.NewStartingIndex);
                CollectionAssert.AreEquivalent(newPerson.Parents, args.NewItems);
            };

            _source.Insert(1, newPerson);
            Assert.AreEqual(1, callCount);

            Assert.AreEqual(6, _target.Count);
            Assert.AreEqual("BobParent0", _target[0].Name);
            Assert.AreEqual("BobParent1", _target[1].Name);
            Assert.AreEqual("NinjaParent0", _target[2].Name);
            Assert.AreEqual("NinjaParent1", _target[3].Name);
            Assert.AreEqual("JimParent0", _target[4].Name);
            Assert.AreEqual("JimParent1", _target[5].Name);
        }

#endif
        [Test]
        public void AddNewSublistToSource_SublistIsNull_DoesNotFireCollectionChangedEvent()
        {
            Person newPerson = new Person("Ninja", 23);

            int callCount = 0;

            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
            };

            _source.Insert(0, newPerson);
            Assert.AreEqual(0, callCount);

            Assert.AreEqual(4, _target.Count);
            Assert.AreEqual("BobParent0", _target[0].Name);
            Assert.AreEqual("BobParent1", _target[1].Name);
            Assert.AreEqual("JimParent0", _target[2].Name);
            Assert.AreEqual("JimParent1", _target[3].Name);
        }

#if SILVERLIGHT
        [Test]
        public void RemoveSublistFromSource_Always_FireCollectionChangedEvent()
        {
            int callCount = 0;

            _target.CollectionChanged += (sender, args) =>
            {
                Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
                Assert.AreEqual(2 + callCount, args.OldStartingIndex);
                Assert.AreEqual(_parents[1][callCount], args.OldItems[0]);
                callCount++;
            };

            _source.RemoveAt(1);

            Assert.AreEqual(2, callCount);

            Assert.AreEqual(2, _target.Count);
            Assert.AreEqual("BobParent0", _target[0].Name);
            Assert.AreEqual("BobParent1", _target[1].Name);
        }
#else
        [Test]
        public void RemoveSublistFromSource_Always_FireCollectionChangedEvent()
        {
            int callCount = 0;

            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
                Assert.AreEqual(2, args.OldStartingIndex);
                CollectionAssert.AreEquivalent(_parents[1], args.OldItems);
            };

            _source.RemoveAt(1);

            Assert.AreEqual(1, callCount);

            Assert.AreEqual(2, _target.Count);
            Assert.AreEqual("BobParent0", _target[0].Name);
            Assert.AreEqual("BobParent1", _target[1].Name);
        }
#endif

#if SILVERLIGHT
        [Test]
        public void ReplaceItemInSource_Always_FireCollectionChangedEvent()
        {
            Person replacement = new Person("Ninja", 23);
            ClinqTestFactory.InitializeParents(replacement);

            int callCount = 0;

            _target.CollectionChanged += (sender, args) =>
                {
                    Assert.AreEqual(NotifyCollectionChangedAction.Replace, args.Action);
                    
                    Assert.AreEqual(2 + callCount, args.NewStartingIndex);
                    Assert.AreEqual(replacement.Parents[callCount], args.NewItems[0]);
                    Assert.AreEqual(_parents[1][callCount], args.OldItems[0]);
                    
                    callCount++;
                };

            _source[1] = replacement;

            Assert.AreEqual(2, callCount);
        }
#else
        [Test]
        public void ReplaceItemInSource_Always_FireCollectionChangedEvent()
        {
            Person replacement = new Person("Ninja", 23);
            ClinqTestFactory.InitializeParents(replacement);

            int callCount = 0;

            _target.CollectionChanged += (sender, args) =>
                {
                    callCount++;
                    TestUtilities.AssertReplace(args, 2, replacement.Parents.ToArray(), _parents[1].ToArray());
                };

            _source[1] = replacement;

            Assert.AreEqual(1, callCount);
        }
#endif

#if SILVERLIGHT
        [Test]
        public void ReplaceExistingSublist_NewSublistEmpty_FiresRemoveCollectionChanged()
        {
            var replacementParents = new ObservableCollection<Person>()
            {
            };

            int callCount = 0;

            _target.CollectionChanged += (sender, args) =>
            {
                Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
                Assert.AreEqual(callCount + 2, args.OldStartingIndex);
                Assert.AreEqual(_parents[1][callCount], args.OldItems[0]);
                callCount++;
            };

            _source[1].Parents = replacementParents;

            Assert.AreEqual(2, callCount);

            Assert.AreEqual(2, _target.Count);
            Assert.AreEqual("BobParent0", _target[0].Name);
            Assert.AreEqual("BobParent1", _target[1].Name);
        }

#else
        [Test]
        public void ReplaceExistingSublist_NewSublistEmpty_FiresRemoveCollectionChanged()
        {
            var replacementParents = new ObservableCollection<Person>()
            {
            };

            int callCount = 0;

            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;

                Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
                Assert.AreEqual(2, args.OldStartingIndex);
                CollectionAssert.AreEquivalent(_parents[1], args.OldItems);

                Assert.AreEqual(2, _target.Count);
                Assert.AreEqual("BobParent0", _target[0].Name);
                Assert.AreEqual("BobParent1", _target[1].Name);
            };

            _source[1].Parents = replacementParents;

            Assert.AreEqual(1, callCount);
        }
#endif

#if SILVERLIGHT
        [Test]
        public void ReplaceExistingSublist_OldSublistEmpty_FiresAddCollectionChanged()
        {
            Person replacement = new Person("Ninja", 23);
            ClinqTestFactory.InitializeParents(replacement);
            _source[1].Parents = new ObservableCollection<Person>();

            int callCount = 0;

            _target.CollectionChanged += (sender, args) =>
            {
                Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
                Assert.AreEqual(2 + callCount, args.NewStartingIndex);
                Assert.AreEqual(replacement.Parents[callCount], args.NewItems[0]);
                
                callCount++;
            };

            _source[1] = replacement;

            Assert.AreEqual(2, callCount);

            Assert.AreEqual(4, _target.Count);
            Assert.AreEqual("BobParent0", _target[0].Name);
            Assert.AreEqual("BobParent1", _target[1].Name);
            Assert.AreEqual("NinjaParent0", _target[2].Name);
            Assert.AreEqual("NinjaParent1", _target[3].Name);
        }
#else
        [Test]
        public void ReplaceExistingSublist_OldSublistEmpty_FiresAddCollectionChanged()
        {
            Person replacement = new Person("Ninja", 23);
            ClinqTestFactory.InitializeParents(replacement);
            _source[1].Parents = new ObservableCollection<Person>();

            int callCount = 0;

            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;

                Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
                Assert.AreEqual(2, args.NewStartingIndex);
                CollectionAssert.AreEquivalent(replacement.Parents, args.NewItems);

                Assert.AreEqual(4, _target.Count);
                Assert.AreEqual("BobParent0", _target[0].Name);
                Assert.AreEqual("BobParent1", _target[1].Name);
                Assert.AreEqual("NinjaParent0", _target[2].Name);
                Assert.AreEqual("NinjaParent1", _target[3].Name);
            };

            _source[1] = replacement;

            Assert.AreEqual(1, callCount);
        }
#endif

#if SILVERLIGHT
        [Test]
        public void ReplaceExistingSublist_SublistIsNull_FireCollectionChangedEvent()
        {
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
                Assert.AreEqual(callCount, args.OldStartingIndex);
                
                Assert.AreEqual(_parents[0][callCount], args.OldItems[0]);

                callCount++;
            };

            _source[0].Parents = null;

            Assert.AreEqual(2, callCount);

            Assert.AreEqual(2, _target.Count);
            Assert.AreEqual("JimParent0", _target[0].Name);
            Assert.AreEqual("JimParent1", _target[1].Name);
        }
#else
        [Test]
        public void ReplaceExistingSublist_SublistIsNull_FireCollectionChangedEvent()
        {
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;

                Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
                Assert.AreEqual(0, args.OldStartingIndex);
                CollectionAssert.AreEquivalent(_parents[0], args.OldItems);
            };

            _source[0].Parents = null;

            Assert.AreEqual(1, callCount);

            Assert.AreEqual(2, _target.Count);
            Assert.AreEqual("JimParent0", _target[0].Name);
            Assert.AreEqual("JimParent1", _target[1].Name);
        }
#endif
        [Test]
        public void ResetSource_Always_FireCollectionChangedEvent()
        {
            int callCount = 0;
            _target.CollectionChanged += (sender, args) =>
            {
                callCount++;
                Assert.AreEqual(NotifyCollectionChangedAction.Reset, args.Action);
            };

            _source.Clear();
            Assert.AreEqual(1, callCount);

            Assert.AreEqual(0, _target.Count);
        }

        [Test]
        public void Enumerate_Always_GoesThroughCollection()
        {
            var items = ((IEnumerable<Person>)_source).SelectMany(p => p.Parents);

            IEnumerator<Person> itemsEnumerator = items.GetEnumerator();
            Assert.IsTrue(itemsEnumerator.MoveNext());
            Assert.AreEqual(items.Count(), _target.Count);

            foreach (var item in _target)
            {
                Assert.AreEqual(itemsEnumerator.Current, item);
                itemsEnumerator.MoveNext();
            }
        }
    }
}
