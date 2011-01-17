using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using ContinuousLinq.Collections;
using NUnit.Framework;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class TakeReadOnlyContinuousCollectionTest : BaseReadOnlyCollectionTest<TakeReadOnlyContinuousCollection<Person>, Person>
    {
        private int take;
        
        #region Overrides
        protected override Func<TakeReadOnlyContinuousCollection<Person>> TargetFactory
        {
            get { return () => new TakeReadOnlyContinuousCollection<Person>(_source, take); }
        }
        protected override void AssertTargetMatchesSource()
        {
            for (int i = 0; i < take && i < _source.Count; i++)
            {
                Assert.AreEqual(_target[i], _source[i]);
            }
        }
        #endregion

        #region Setup
        [SetUp]
        public void SetUp()
        {
            take = 4;
            SetUp6PersonSource();
        }
        #endregion
        #region Tests
        [Test]
        public void IndexerGet_ItemsInSource_ItemsMatchSelection()
        {
            AssertTargetMatchesSource();
        }
        [Test]
        public void CountIsTakeCount_When_SourceCountIsEqualToTakeCount()
        {
            Assert.AreEqual(take, _target.Count);
        }
        [Test]
        public void CountIsTakeCount_When_SourceCountIsGreaterThanTakeCount()
        {
            take = 6;
            SetUp6PersonSource();
            Assert.AreEqual(take, _target.Count); 
        }
        [Test]
        public void CountIsSourceCount_When_SourceCountIsLessThanTakeCount()
        {
            take = 6;
            SetUp2PersonSource();
            Assert.AreEqual(_source.Count, _target.Count);
        }
        [Test]
        public void AddItemToSourceDoesNotFireCollectionChanged_When_IndexGreaterThanTakeCount()
        {
            var newPerson = new Person { Name = "NewPerson" };
            _source.Add(newPerson);
            Assert.IsFalse(collectionChangedHandlersCompleted);
        }
        [Test]
        public void AddItemToSourceFiresRemoveThenAdd_When_IndexLessThanTakeCountAndAlreadyTakingMax()
        {
            var newPerson = new Person { Name = "NewPerson" };
            registerCollectionChangedAssertions(
                forRemoveWithOldIndexAndRemovedPersons(3, _source[3]),
                forAddWithNewIndexAndAddedPersons(1, newPerson));
            _source.Insert(1,newPerson);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }
#if !SILVERLIGHT
        [Test]
        public void AddItemsToSourceFiresRemoveThenAdd_When_IndexLessThanTakeCountAndAlreadyTakingMax()
        {
            var newPersons = new[] {new Person {Name = "NewPerson0"}, new Person {Name = "NewPerson1"}};
            registerCollectionChangedAssertions(
                forRemoveWithOldIndexAndRemovedPersons(2, _source[2],_source[3]),
                forAddWithNewIndexAndAddedPersons(1, newPersons));
            _source.InsertRange(1, newPersons);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }
        [Test]
        public void AddMoreThanTakeCountItemsToSourceFiresRemoveThenAdd_When_IndexLessThanTakeCountAndAlreadyTakingMax()
        {
            var newPersons = new[]
                                 {
                                     new Person {Name = "NewPerson0"}, new Person {Name = "NewPerson1"},
                                     new Person {Name = "NewPerson2"}, new Person {Name = "NewPerson3"},
                                     new Person {Name = "NewPerson4"}, new Person {Name = "NewPerson5"}
                                 };
            registerCollectionChangedAssertions(
                forRemoveWithOldIndexAndRemovedPersons(1, _source[1], _source[2], _source[3]),
                forAddWithNewIndexAndAddedPersons(1, newPersons.Take(3).ToArray()));
            _source.InsertRange(1, newPersons);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }

        [Test]
        public void AddItemsToSourceFiresOnlyAdd_When_NotTakingMax()
        {
            SetUp2PersonSource();
            var newPerson = new Person { Name = "NewPerson" };
            registerCollectionChangedAssertions(forAddWithNewIndexAndAddedPersons(1, newPerson));
            _source.Insert(1, newPerson);
            Assert.IsTrue(collectionChangedHandlersCompleted);
            registerCollectionChangedAssertions(forAddWithNewIndexAndAddedPersons(3, newPerson));
            _source.AddRange(new[] {newPerson, newPerson, newPerson});
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }
#endif
        [Test]
        public void RemoveItemFromSourceFiresOnlyRemove_When_NotTakingMax()
        {
            SetUp2PersonSource();
            Person personToRemove = _source[0];
            registerCollectionChangedAssertions(forRemoveWithOldIndexAndRemovedPersons(0, personToRemove));
            _source.Remove(personToRemove);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }
        [Test]
        public void RemoveItemFromSourceDoesNotFire_When_RemovedIndexGreaterThanOrEqualToTakeCount()
        {
            Person personToRemove = _source[5];
            _source.Remove(personToRemove);
            Assert.IsFalse(collectionChangedHandlersCompleted);
            personToRemove = _source[4];
            _source.Remove(personToRemove);
            Assert.IsFalse(collectionChangedHandlersCompleted);
        }
#if !SILVERLIGHT
        [Test]
        public void RemoveItemsFromSourceDoesNotFire_When_RemovedIndexGreaterThanOrEqualToTakeCount()
        {
            _source.RemoveRange(4,2);
            Assert.IsFalse(collectionChangedHandlersCompleted);
        }
#endif
        [Test]
        public void RemoveItemFromSourceFiresRemoveThenAdd_When_IndexLessThanTakeCountAndAlreadyTakingMax()
        {
            Person personToRemove = _source[1];
            Person personToAdd = _source[4];
            registerCollectionChangedAssertions(forRemoveWithOldIndexAndRemovedPersons(1, personToRemove),
                forAddWithNewIndexAndAddedPersons(3, personToAdd));
            _source.Remove(personToRemove);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }
        #if !SILVERLIGHT
        [Test]
        public void RemoveItemsFromSourceFiresRemoveThenAdd_When_IndexLessThanTakeCountAndAlreadyTakingMax()
        {
           registerCollectionChangedAssertions(forRemoveWithOldIndexAndRemovedPersons(0, _source[0], _source[1]),
                forAddWithNewIndexAndAddedPersons(2, _source[4], _source[5]));
            _source.RemoveRange(0, 2);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }
        [Test]
        public void RemoveItemsFromSourceFiresRemoveThenAdd_When_IndexLessThanTakeCountAndAlreadyTakingMaxAndIndexPlusRemovedItemsCountEqualsTakeCount()
        {
            SetUp6PersonSource();
            registerCollectionChangedAssertions(forRemoveWithOldIndexAndRemovedPersons(2, _source[2], _source[3]),
                 forAddWithNewIndexAndAddedPersons(2, _source[4], _source[5]));
            _source.RemoveRange(2, 2);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }
        [Test]
        public void RemoveRestOfItemsInTakeFromSourceFiresRemoveThenAdd_When_AlreadyTakingMax()
        {
            take = 3;
            SetUp6PersonSource();
            registerCollectionChangedAssertions(
                forRemoveWithOldIndexAndRemovedPersons(1, _source[1], _source[2]),
                forAddWithNewIndexAndAddedPersons(1, _source[3], _source[4]));
            _source.RemoveRange(1, 2);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }
        [Test]
        public void RemoveMoreItemsThanTakeFromSourceFiresRemoveThenAdd_When_AlreadyTakingMax()
        {
            take = 3;
            SetUp6PersonSource();
            registerCollectionChangedAssertions(
                forRemoveWithOldIndexAndRemovedPersons(1, _source[1], _source[2]),
                forAddWithNewIndexAndAddedPersons(1, _source[4], _source[5]));
            _source.RemoveRange(1, 3);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }

        [Test]
        public void RemoveMoreItemsThanTakeFromSourceFiresRemoveThenAdd_When_AlreadyTakingMaxAndLessThanMaxItemsLeft()
        {
            take = 3;
            SetUp6PersonSource();
            registerCollectionChangedAssertions(
                forRemoveWithOldIndexAndRemovedPersons(0, _source[0], _source[1], _source[2]),
                forAddWithNewIndexAndAddedPersons(0, _source[4], _source[5]));
            _source.RemoveRange(0, 4);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }

        [Test]
        public void MoveItemInSourceDoesNotFireCollectionChanged_When_ItemOutsideTakeRangeMovedOutsideOfTakeRange()
        {
            _source.Move(4,5);
            Assert.IsFalse(collectionChangedHandlersCompleted);
        }
        [Test]
        public void MoveItemInSourceFiresMove_When_ItemInsideTakeRangeMovedToIndexInsideTakeRange()
        {
            registerCollectionChangedAssertions(forMoveWithNewIndexOldIndexAndMovedPersons(1,0, _source[0]));
            _source.Move(0,1);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }
        [Test]
        public void MoveItemInSourceFiresRemoveThenAdd_When_ItemInsideTakeRangeMovedToIndexOutsideTakeRange()
        {
            int edge = take - 1;
            registerCollectionChangedAssertions(forRemoveWithOldIndexAndRemovedPersons(0, _source[0]),
                                                forAddWithNewIndexAndAddedPersons(edge, _source[4]));
            _source.Move(0, 5);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }

        [Test]
        public void MoveItemsInSourceFiresRemoveThenAdd_When_ItemOutsideTakeRangeMovedToIndexInsideTakeRange()
        {
            int edge = take - 1;
            registerCollectionChangedAssertions(forRemoveWithOldIndexAndRemovedPersons(edge, _source[edge]),
                                                forAddWithNewIndexAndAddedPersons(1, _source[5]));
            _source.Move(5, 1);
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }
#endif
        [Test]
        public void ClearSource_Always_FiresReset()
        {
            registerCollectionChangedAssertions(forReset());
            _source.Clear();
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }
        [Test]
        public void ReplaceItemsInSourceFiresReplace_When_ItemInsideTakeRangeReplaced()
        {
            Person newPerson = new Person() { Name = "NewPerson" };
            Person oldPerson = _source[0];
            registerCollectionChangedAssertions(forReplaceWithIndexNewPersonsAndReplacedPersons(0, newPerson.ItemToArray(), oldPerson));
            _source[0] = newPerson;
            Assert.IsTrue(collectionChangedHandlersCompleted);
        }
        [Test]
        public void ReplaceItemsInSourceDoesNotFiresReplace_When_ItemOutsideTakeRangeReplaced()
        {
            _source[5] = new Person() { Name = "NewPerson" };
            Assert.IsFalse(collectionChangedHandlersCompleted);
        }
        #endregion
    }
}
