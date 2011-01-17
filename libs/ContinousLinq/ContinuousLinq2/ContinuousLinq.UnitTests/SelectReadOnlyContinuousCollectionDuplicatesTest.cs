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
    public class SelectReadOnlyContinuousCollectionDuplicatesTest
    {
        private SelectReadOnlyContinuousCollection<Person, string> _target;
        private ObservableCollection<Person> _source;

        [SetUp]
        public void Setup()
        {
            _source = ClinqTestFactory.CreateSixPersonSourceWithDuplicates();
            _target = new SelectReadOnlyContinuousCollection<Person, string>(
                _source,
                p => p.Name);
        }

        [Test]
        public void IndexerGet_ItemsInSource_ItemsMatchSelection()
        {
            Assert.AreEqual("Bob", _target[0]);
            Assert.AreEqual("Bob", _target[1]);
            Assert.AreEqual("Bob", _target[2]);
            Assert.AreEqual("3", _target[3]);
            Assert.AreEqual("Jim", _target[4]);
            Assert.AreEqual("Jim", _target[5]);
        }

        [Test]
        public void ChangeMonitoredPropertyOnItemInSource_Always_FireCollectionChangedEvent()
        {
            int[] callCounts = new int[3];
            _target.CollectionChanged += (sender, args) =>
            {
                Assert.IsTrue(args.NewStartingIndex >= 0 && args.NewStartingIndex <= 2);
                callCounts[args.NewStartingIndex]++;
                
                Assert.AreEqual(NotifyCollectionChangedAction.Replace, args.Action);
                Assert.IsTrue(args.NewItems.Contains("DifferentName"));
                Assert.IsTrue(args.OldItems.Contains("Bob"));
            };

            _source[0].Name = "DifferentName";
            Assert.AreEqual(1, callCounts[0]);
            Assert.AreEqual(1, callCounts[1]);
            Assert.AreEqual(1, callCounts[2]);
        }

        [Test]
        public void RemoveItemsAndChangeMonitoredPropertyOnItemInSource_Always_FireCollectionChangedEvent()
        {
            int[] callCounts = new int[2];

            _source.Remove(_source[0]);

            _target.CollectionChanged += (sender, args) =>
            {
                Assert.IsTrue(args.NewStartingIndex >= 0 && args.NewStartingIndex <= 1);
                callCounts[args.NewStartingIndex]++;
                
                Assert.AreEqual(NotifyCollectionChangedAction.Replace, args.Action);
                Assert.IsTrue(args.NewItems.Contains("DifferentName"));
                Assert.IsTrue(args.OldItems.Contains("Bob"));
            };

            _source[0].Name = "DifferentName";
            Assert.AreEqual(1, callCounts[0]);
            Assert.AreEqual(1, callCounts[1]);
        }

#if !SILVERLIGHT
        [Test]
        public void MoveItemsAndChangeMonitoredPropertyOnItemInSource_FirstToLast_FireCollectionChangedEvent()
        {
            _source.Move(0, 5);

            List<int> changedIndices = new List<int>();

            _target.CollectionChanged += (sender, args) =>
            {
                changedIndices.Add(args.NewStartingIndex);
                Assert.AreEqual(args.NewStartingIndex, args.OldStartingIndex);
                Assert.AreEqual(NotifyCollectionChangedAction.Replace, args.Action);
                Assert.IsTrue(args.NewItems.Contains("DifferentName"));
                Assert.IsTrue(args.OldItems.Contains("Bob"));
            };

            _source[5].Name = "DifferentName";

            Assert.AreEqual(3, changedIndices.Count);

            Assert.IsTrue(changedIndices.Contains(0));
            Assert.IsTrue(changedIndices.Contains(1));
            Assert.IsTrue(changedIndices.Contains(5));
        }
#endif
    }
}
