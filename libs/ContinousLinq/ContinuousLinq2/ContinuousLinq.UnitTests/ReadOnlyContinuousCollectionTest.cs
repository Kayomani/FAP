using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;

namespace ContinuousLinq.UnitTests
{

    [TestFixture]
    public class ReadOnlyContinuousCollectionTest
    {
        private MockReadOnlyContinuousCollection _target;
        private List<int> _source;

        [SetUp]
        public void Setup()
        {
            _source = new List<int>() { 0, 1, 2, 3, 4 };
            PropertyAccessTree propertyAcessTree = new PropertyAccessTree();
            _target = new MockReadOnlyContinuousCollection(_source); 
        }


        [Test]
        public void IsReadOnly_Alway_True()
        {
            Assert.IsTrue(_target.IsReadOnly);
        }

        [Test]
        [ExpectedException(typeof(AccessViolationException))]
        public void SetIndexer_SourceHasValues_ThrowsException()
        {
            _target[0] = 1;
        }

        [Test]
        public void Count_HasItems_CountOfItemsReturned()
        {
            Assert.AreEqual(5, _target.Count);
        }

        [Test]
        public void GetIndexer_HasValues_MirrorsSourceIndices()
        {
            for (int i = 0; i < _source.Count; i++)
            {
                Assert.AreEqual(_source[i], _target[i]);
            }
        }

        [Test]
        public void IndexOf_ItemInSource_ReturnsIndexOfItem()
        {
            int item = _source[0];
            Assert.AreEqual(0, _target.IndexOf(item));
        }

        [Test]
        public void IndexOf_ItemNotInSource_ReturnsIndexOfItem()
        {
            Assert.AreEqual(-1, _target.IndexOf(500));
        }

        [Test]
        public void CopyTo_SourceHasItems_ItemsCopiedToArray()
        {
            int[] output = new int[5];
            _target.CopyTo(output, 0);
            
            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(_source[i], output[i]);
            }
        }

        [Test]
        public void CopyTo_SourceHasItemsAndStartingInMiddleOfTargetArray_ItemsCopiedToArray()
        {
            int[] output = new int[6];
            _target.CopyTo(output, 1);
            
            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(_source[i], output[i + 1]);
            }
        }

        [Test]
        public void Contains_SourceHasItem_ReturnsTrue()
        {
            Assert.IsTrue(_target.Contains(_source[0]));
        }

        [Test]
        public void IListContains_SourceHasItem_ReturnsTrue()
        {
            ObservableCollection<int> items = new ObservableCollection<int>() { 0, 1, 2, 3, 4 };
            Assert.IsTrue(((IList)items).Contains((object)0));
            Assert.IsTrue(_target.Contains((object)_source[0]));
        }

        [Test]
        public void Contains_SourceMissingItem_ReturnsFalse()
        {
            Assert.IsFalse(_target.Contains(6));
        }

        [Test]
        public void IListContains_SourceMissingItem_ReturnsFalse()
        {
            Assert.IsFalse(_target.Contains((object)6));
        }

        [Test]
        public void FireCollectionChange_AnyActionButReplace_NotifiesCountPropertyChanged()
        {
            int callCount = 0;
            _target.PropertyChanged+= (sender, args) =>
            {
                callCount++;
                Assert.AreEqual("Count", args.PropertyName);
            };

            _target.FireBaseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void FireCollectionChange_Replace_NotifiesCountPropertyChanged()
        {
            int callCount = 0;
            _target.PropertyChanged += (sender, args) =>
            {
                callCount++;
            };

            _target.FireBaseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new object(), new object(), 0));

            Assert.AreEqual(0, callCount);
        }

        public class MockReadOnlyContinuousCollection : ReadOnlyContinuousCollection<int>
        {
            public List<int> Source { get; set; }
 
            public MockReadOnlyContinuousCollection(List<int> source)
            {
                this.Source = source;
            }

            public override int this[int index]
            {
                get
                {
                    return this.Source[index];
                }
                set
                {
                    throw new AccessViolationException();
                }
            }

            public override int Count
            {
                get { return this.Source.Count; }
            }

            public void FireBaseCollectionChanged(NotifyCollectionChangedEventArgs args)
            {
                RefireCollectionChanged(args);
            }
        }
    }
}
