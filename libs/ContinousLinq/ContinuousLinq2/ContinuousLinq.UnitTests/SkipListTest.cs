using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class SkipListTest
    {
        private SkipList<int, object> _target;
        private List<object> _values;
        [SetUp]
        public void Setup()
        {
            _values = new List<object>()
            {
                new object(),
                new object(),
                new object(),
            };

            _target = new SkipList<int, object>();
        }

        [Test]
        public void Add_OneItem_ItemInList()
        {
            _target.Add(0, _values[0]);

            var value = _target.GetValue(0);
            Assert.AreEqual(_values[0], value);
        }

        [Test]
        public void Add_TwoItems_ItemsInList()
        {
            _target.Add(0, _values[0]);
            _target.Add(1, _values[1]);

            Assert.AreEqual(_values[0], _target.GetValue(0));
            Assert.AreEqual(_values[1], _target.GetValue(1));
        }

        [Test]
        public void Add_ManyManyItems_AllValuesFound()
        {
            var valueForAll = _values[0];
            int numberOfItems = 10000;
            for (int i = 0; i < numberOfItems; i++)
            {
                _target.Add(i, valueForAll);
            }

            for (int i = 0; i < numberOfItems; i++)
            {
                Assert.AreEqual(valueForAll, _target.GetValue(i));
            }
        }


        [Test]
        public void Remove_TwoItems_ItemsNoLongerInList()
        {
            _target.Add(0, _values[0]);
            _target.Add(1, _values[1]);

            _target.Remove(1);
            _target.Remove(0);

            object value;
            Assert.IsFalse(_target.TryGetValue(0, out value));
            Assert.IsFalse(_target.TryGetValue(1, out value));
        }

        [Test]
        public void Remove_ManyManyItems_AllValuesFound()
        {
            var valueForAll = _values[0];
            int numberOfItems = 10000;
            for (int i = 0; i < numberOfItems; i++)
            {
                _target.Add(i, valueForAll);
            }

            for (int i = 0; i < numberOfItems; i++)
            {
                _target.Remove(i);
            }

            for (int i = 0; i < numberOfItems; i++)
            {
                object value;
                Assert.IsFalse(_target.TryGetValue(i, out value));
            }
        }

        [Test]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void GetValue_NoItems_Throws()
        {
            _target.GetValue(0);
        }

        [Test]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void GetValue_HasItems_Throws()
        {
            _target.Add(1, 0);
            _target.Add(-9, 0);
            _target.Add(100, 0);

            _target.GetValue(0);
        }
    }
}
