using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using ContinuousLinq.Collections;
using NUnit.Framework;
using System.Collections;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class SkipReadOnlyContinuousCollectionTest :
        BaseReadOnlyCollectionTest<SkipReadOnlyContinuousCollection<Person>, Person>
    {
        private int skip;

        [SetUp]
        public void SetUp()
        {
            skip = 2;
            SetUp6PersonSource();
        }

        protected override Func<SkipReadOnlyContinuousCollection<Person>> TargetFactory
        {
            get { return () => new SkipReadOnlyContinuousCollection<Person>(_source, skip); }
        }

        protected override void AssertTargetMatchesSource()
        {
            for (int i = skip, targetI = 0; i < _source.Count; i++, targetI++)
            {
                Assert.AreEqual(_target[targetI], _source[i]);
            }
        }

        [Test]
        public void IndexerGet_ItemsInSource_ItemsMatchSelection()
        {
            AssertTargetMatchesSource();
        }
        [Test]
        public void CountIsSourceCountMinusSkipCount_When_SourceCountIsGreaterThanSkipCount()
        {
            Assert.AreEqual(_source.Count - skip, _target.Count);
        }
        [Test]
        public void CountIs0_When_SourceCountIsLessThanSkipCount()
        {
            skip = 10;
            SetUp6PersonSource();
            Assert.AreEqual(0, _target.Count);
        }
        [Test]
        public void CountIs0_When_SourceCountIsEqualToSkipCount()
        {
            skip = 6;
            SetUp6PersonSource();
            Assert.AreEqual(0, _target.Count);
        }
        [Test]
        public void AddItemToSourceFiresAddForItemNextToSkipCountIndex_When_IndexLessThanSkipCount()
        {
            var newPerson = new Person() {Name = "NewPerson"};
            registerCollectionChangedAssertions(forAddWithNewIndexAndAddedPersons(0, _source[skip - 1]));
            _source.Insert(1, newPerson);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }

        [Test]
        public void AddItemToSourceFiresAdd_When_IndexIsGreaterThanSkipCount()
        {
            var newPerson = new Person {Name = "NewPerson"};
            registerCollectionChangedAssertions(forAddWithNewIndexAndAddedPersons(2, newPerson));
            _source.Insert(4, newPerson);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }

        [Test]
        public void AddItemToSourceDoesNotFireCollectionChangedEvent_When_CountAfterItemAddIsLessThanSkipCount()
        {
            skip = 4;
            SetUp2PersonSource();
            var newPerson = new Person {Name = "NewPerson"};
            _source.Add(newPerson);
            Assert.IsFalse(collectionChangedHandlersCompleted);
        }

        [Test]
        public void RemoveItemFromSourceDoesNotFiresCollectionChangedEvent_When_CountIsLessThanSkipCount()
        {
            skip = 4;
            SetUp2PersonSource();
            _source.RemoveAt(0);
            Assert.IsFalse(collectionChangedHandlersCompleted);
        }

        [Test]
        public void RemoveItemFromSourceDoesNotFiresCollectionChangedEvent_When_CountIsEqualToSkipCount()
        {
            skip = 2;
            SetUp2PersonSource();
            _source.RemoveAt(0);
            Assert.IsFalse(collectionChangedHandlersCompleted);
        }
        [Test]
        public void RemoveItemFromSourceFiresRemove_When_IndexGreaterThanOrEqualToSkipCount()
        {
            registerCollectionChangedAssertions(forRemoveWithOldIndexAndRemovedPersons(3, _source[5]));
            _source.RemoveAt(5);
            Assert.IsTrue(collectionChangedHandlersCompleted);
            registerCollectionChangedAssertions(forRemoveWithOldIndexAndRemovedPersons(0, _source[skip]));
            _source.RemoveAt(skip);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }
#if!SILVERLIGHT
        [Test]
        public void RemoveItemsFromSourceFiresRemove_When_IndexGreaterThanOrEqualToSkipCount()
        {
            SetUp10PersonSource();
            registerCollectionChangedAssertions(forRemoveWithOldIndexAndRemovedPersons(3, _source[5], _source[6],
                                                                                       _source[7]));
            _source.RemoveRange(5, 3);
            Assert.IsTrue(collectionChangedHandlersCompleted);
            registerCollectionChangedAssertions(forRemoveWithOldIndexAndRemovedPersons(0, _source[skip],
                                                                                       _source[skip + 1]));
            _source.RemoveRange(skip, 2);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }

        [Test]
        public void RemoveItemsFromSourceFiresRemove_When_IndexLessThanSkipCount()
        {
            registerCollectionChangedAssertions(forRemoveWithOldIndexAndRemovedPersons(0, _source[2], _source[3]));
            _source.RemoveRange(0, 2);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }
        [Test]
        public void RemoveItemsFromSourceFiresRemove_When_IndexLessThanSkipCountAndAlsoRemovingItemsInSkipRange()
        {
            skip = 4;
            SetUp10PersonSource();
            registerCollectionChangedAssertions(forRemoveWithOldIndexAndRemovedPersons(0, _source[skip],
                                                                                       _source[skip + 1],
                                                                                       _source[skip + 2],
                                                                                       _source[skip + 3]));
            _source.RemoveRange(2, 4);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }
        [Test]
        public void RemoveAllItemsFromSourceFiresReset_Always()
        {
            registerCollectionChangedAssertions(forReset());
            _source.RemoveRange(0, _source.Count);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }

        [Test]
        public void RemoveItemsFromSourceFiresReset_When_NoItemsLeftInSkipRange()
        {
            registerCollectionChangedAssertions(forReset());
            _source.RemoveRange(0, 4);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }
#endif
        [Test]
        public void RemoveItemFromSourceFiresRemove_When_IndexLessThanSkipCount()
        {
            registerCollectionChangedAssertions(forRemoveWithOldIndexAndRemovedPersons(0, _source[2]));
            _source.RemoveAt(0);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }

       
        [Test]
        public void ClearSource_Always_FireCollectionChangedEvent()
        {
            registerCollectionChangedAssertions(forReset());
            _source.Clear();
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }

#if !SILVERLIGHT
        [Test]
        public void MoveItemInSourceFiresMove_When_ItemMovedWithinSkipRange()
        {
            registerCollectionChangedAssertions(forMoveWithNewIndexOldIndexAndMovedPersons(3, 1, _source[3]));
            _source.Move(3, 5);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }

        [Test]
        public void MoveItemInSourceFiresRemoveThenAdd_When_ItemMovedFromSkipRangeToOutsideSkipRange()
        {
            registerCollectionChangedAssertions(forRemoveWithOldIndexAndRemovedPersons(5 - skip, new[] {_source[5]}),
                                                forAddWithNewIndexAndAddedPersons(0, _source[skip - 1]));
            _source.Move(5, 1);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }

        [Test]
        public void MoveItemInSourceFiresRemoveThenAdd_When_ItemMovedOutsideSkipRangeToIndexGreaterThanSkipCount()
        {
            registerCollectionChangedAssertions(forRemoveWithOldIndexAndRemovedPersons(0, _source[skip]),
                                                forAddWithNewIndexAndAddedPersons(4 - skip, _source[0]));
            _source.Move(0, 4);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }

        [Test]
        public void MoveItemInSourceFiresReplace_When_ItemMovedOutsideSkipRangeToSkipRangeBoundary()
        {
            registerCollectionChangedAssertions(forReplaceWithIndexNewPersonsAndReplacedPersons(0, new[] {_source[0]},
                                                                                                _source[skip]));
            _source.Move(0, skip);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }

        [Test]
        public void MoveItemInSourceDoesNotFireMove_When_ItemMovedOutsideSkipRangeToOutsideSkipRange()
        {
            _source.Move(0, 1);
            Assert.IsFalse(collectionChangedHandlersCompleted);
        }
#endif

        [Test]
        public void ReplaceItemInSourceFiresReplace_When_ItemReplacedInsideSkipRange()
        {
            var newPerson = new Person {Name = "NewPerson"};
            registerCollectionChangedAssertions(forReplaceWithIndexNewPersonsAndReplacedPersons
                                                    (2, new[] {newPerson}, _source[skip + 2]));
            _source[4] = newPerson;
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }

        [Test]
        public void ReplaceItemInSourceDoesNotFireReplace_When_ItemReplacedOutsideSkipRange()
        {
            _source[0] = new Person {Name = "NewPerson"};
            Assert.IsFalse(collectionChangedHandlersCompleted);
        }
#if !SILVERLIGHT
        [Test]
        public void ReplaceItemsInSourceDoesNotFireReplace_When_ItemsReplacedOutsideSkipRange()
        {
            _source.ReplaceRange(0, new[] {new Person {Name = "NewPerson0"}, new Person {Name = "NewPerson1"}});
            Assert.IsFalse(collectionChangedHandlersCompleted);
        }
        
        [Test]
        public void ReplaceItemsInSourceFiresReplace_When_ItemsReplacedInsideSkipRange()
        {
            var newPeople = new[] {new Person {Name = "NewPerson0"}, new Person {Name = "NewPerson1"}};
            registerCollectionChangedAssertions(forReplaceWithIndexNewPersonsAndReplacedPersons(3 - skip, newPeople, _source[3], _source[4]));
            _source.ReplaceRange(3, newPeople);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }

        [Test]
        public void ReplaceItemsInSourceFiresReplace_When_SomeItemsReplacedOutsideSkipRangeAndSomeItemsReplacedInsideSkipRange()
        {
            var newPeople = new[] { new Person { Name = "NewPerson0" }, new Person { Name = "NewPerson1" }, new Person { Name = "NewPerson2"} };
            registerCollectionChangedAssertions(forReplaceWithIndexNewPersonsAndReplacedPersons(0, newPeople.Skip(1).ToArray(), _source[skip], _source[skip + 1]));
            _source.ReplaceRange(1, newPeople);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }
#endif
    }
}