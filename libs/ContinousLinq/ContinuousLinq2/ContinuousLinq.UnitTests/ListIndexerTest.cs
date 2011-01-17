using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections.ObjectModel;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class ListIndexerTest
    {
        private ListIndexer<Person> _target;
        private ObservableCollection<Person> _source;

        private Person _first;
        private Person _second;
        private Person _third;

        [SetUp]
        public void Setup()
        {
            _source = ClinqTestFactory.CreateSixPersonSourceWithDuplicates();
            _first = _source[0];
            _second = _source[3];
            _third = _source[4];
            _target = new ListIndexer<Person>(_source);
        }

        [Test]
        public void Construct_Always_HasIndicesOfItemsInSource()
        {
            Assert.AreEqual(3, _target[_first].Count());
            Assert.AreEqual(1, _target[_second].Count());
            Assert.AreEqual(2, _target[_third].Count());

            int index = 0;
            Assert.IsTrue(_target[_first].Contains(index++));
            Assert.IsTrue(_target[_first].Contains(index++));
            Assert.IsTrue(_target[_first].Contains(index++));
            Assert.IsTrue(_target[_second].Contains(index++));
            Assert.IsTrue(_target[_third].Contains(index++));
            Assert.IsTrue(_target[_third].Contains(index++));
        }

        [Test]
        public void Contains_ItemInIndex_ReturnsTrue()
        {
            Assert.IsTrue(_target.Contains(_source[0]));
        }

        [Test]
        public void Contains_ItemNotInIndex_ReturnsFalse()
        {
            Assert.IsFalse(_target.Contains(new Person()));
        }

        [Test]
        public void Add_BeginningOfListItemNotAlreadyInList_IndicesCorrect()
        {
            var people = new List<Person>() { new Person() };

            _source.Insert(0, people[0]);
            _target.Add(0, people);

            Assert.AreEqual(1, _target[people[0]].Count());
            Assert.AreEqual(3, _target[_first].Count());
            Assert.AreEqual(1, _target[_second].Count());
            Assert.AreEqual(2, _target[_third].Count());

            int index = 0; 
            Assert.IsTrue(_target[people[0]].Contains(index++));
            Assert.IsTrue(_target[_first].Contains(index++));
            Assert.IsTrue(_target[_first].Contains(index++));
            Assert.IsTrue(_target[_first].Contains(index++));
            Assert.IsTrue(_target[_second].Contains(index++));
            Assert.IsTrue(_target[_third].Contains(index++));
            Assert.IsTrue(_target[_third].Contains(index++));
        }

        [Test]
        public void Add_BeginningOfListItemAlreadyInList_IndicesCorrect()
        {
            var people = new List<Person>() { _first };

            _source.Insert(0, people[0]);
            _target.Add(0, people);

            Assert.AreEqual(4, _target[_first].Count());
            Assert.AreEqual(1, _target[_second].Count());
            Assert.AreEqual(2, _target[_third].Count());

            int index = 0;
            Assert.IsTrue(_target[_first].Contains(index++));
            Assert.IsTrue(_target[_first].Contains(index++));
            Assert.IsTrue(_target[_first].Contains(index++));
            Assert.IsTrue(_target[_first].Contains(index++));
            Assert.IsTrue(_target[_second].Contains(index++));
            Assert.IsTrue(_target[_third].Contains(index++));
            Assert.IsTrue(_target[_third].Contains(index++));
        }

        [Test]
        public void Add_MiddleOfListItemAlreadyInList_IndicesCorrect()
        {
            var people = new List<Person>() { _first };

            _source.Insert(4, people[0]);
            _target.Add(4, people);

            Assert.AreEqual(4, _target[_first].Count());
            Assert.AreEqual(1, _target[_second].Count());
            Assert.AreEqual(2, _target[_third].Count());

            int index = 0; 
            Assert.IsTrue(_target[_first].Contains(index++));
            Assert.IsTrue(_target[_first].Contains(index++));
            Assert.IsTrue(_target[_first].Contains(index++));
            Assert.IsTrue(_target[_second].Contains(index++));
            Assert.IsTrue(_target[_first].Contains(index++));
            Assert.IsTrue(_target[_third].Contains(index++));
            Assert.IsTrue(_target[_third].Contains(index++));
        }

        [Test]
        public void Remove_BeginningOfListItemInList_IndicesCorrect()
        {
            var people = new List<Person>() { _first };

            _source.RemoveAt(0);
            _target.Remove(0, people);

            Assert.AreEqual(2, _target[_first].Count());
            Assert.AreEqual(1, _target[_second].Count());
            Assert.AreEqual(2, _target[_third].Count());

            int index = 0;
            Assert.IsTrue(_target[_first].Contains(index++));
            Assert.IsTrue(_target[_first].Contains(index++));
            Assert.IsTrue(_target[_second].Contains(index++));
            Assert.IsTrue(_target[_third].Contains(index++));
            Assert.IsTrue(_target[_third].Contains(index++));
        }

        [Test]
        public void Remove_EndOfListItemInList_IndicesCorrect()
        {
            var people = new List<Person>() { _third };

            _source.RemoveAt(5);
            _target.Remove(5, people);

            Assert.AreEqual(3, _target[_first].Count());
            Assert.AreEqual(1, _target[_second].Count());
            Assert.AreEqual(1, _target[_third].Count());

            int index = 0;
            Assert.IsTrue(_target[_first].Contains(index++));
            Assert.IsTrue(_target[_first].Contains(index++));
            Assert.IsTrue(_target[_first].Contains(index++));
            Assert.IsTrue(_target[_second].Contains(index++));
            Assert.IsTrue(_target[_third].Contains(index++));
        }

        [Test]
        public void Remove_TwoFromEndOfListItemInList_IndicesCorrect()
        {
            var people = new List<Person>() { _third };

            _source.RemoveAt(5);
            _target.Remove(5, people);

            _source.RemoveAt(4);
            _target.Remove(4, people);

            Assert.AreEqual(3, _target[_first].Count());
            Assert.AreEqual(1, _target[_second].Count());

            int index = 0;
            Assert.IsTrue(_target[_first].Contains(index++));
            Assert.IsTrue(_target[_first].Contains(index++));
            Assert.IsTrue(_target[_first].Contains(index++));
            Assert.IsTrue(_target[_second].Contains(index++));

            Assert.IsFalse(_target.Contains(_third));
        }

        [Test]
        public void Remove_AllItems_IndicesCorrect()
        {
            var people = new List<Person>() { _first, _first, _first, _second, _third, _third };

            _source.RemoveAt(0);
            _source.RemoveAt(0);
            _source.RemoveAt(0);
            _source.RemoveAt(0);
            _source.RemoveAt(0);
            _source.RemoveAt(0);
            _target.Remove(0, people);

            Assert.IsFalse(_target.Contains(_first)); 
            Assert.IsFalse(_target.Contains(_second)); 
            Assert.IsFalse(_target.Contains(_third));
        }
    }
}
