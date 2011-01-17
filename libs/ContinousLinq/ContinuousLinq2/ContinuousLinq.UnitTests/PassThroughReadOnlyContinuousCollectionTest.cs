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
    public class PassThroughReadOnlyContinuousCollectionTest
    {
        private ObservableCollection<Person> _source;
        private PassThroughReadOnlyContinuousCollection<Person> _target;

        [SetUp]
        public void Setup()
        {
            _source = ClinqTestFactory.CreateTwoPersonSource();
            _target = new PassThroughReadOnlyContinuousCollection<Person>(_source);
        }

        [Test]
        public void GetByIndex_ItemsInSource_RetrievesAllItems()
        {
            for (int i = 0; i < _source.Count; i++)
            {
                Assert.AreEqual(_source[i], _target[i]);
            }
        }

        [Test]
        public void AddItemToSource_NewItem_FiresPropertyChanged()
        {
            int callCount = 0;
            _target.CollectionChanged += (sender, args) => callCount++;
            
            _source.Add(new Person());

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void Count_SourceHasItems_SameAsSource()
        {
            Assert.AreEqual(_source.Count, _target.Count);
        }
    }
}
