using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ContinuousLinq;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class AsReadOnlyTest
    {
        private ObservableCollection<Person> _source;

        [SetUp]
        public void Setup()
        {
            _source = ClinqTestFactory.CreateTwoPersonSource();
        }

        [Test]
        public void AsReadOnly_SourceHasItems_ReturnsPassThroughReadOnlyContinuousCollection()
        {
            ReadOnlyContinuousCollection<Person> result = _source.AsReadOnly();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(typeof(PassThroughReadOnlyContinuousCollection<Person>), result);
        }

        [Test]
        public void AsReadOnly_SourceHasItems_AllItemsInResult()
        {
            ReadOnlyContinuousCollection<Person> result = _source.AsReadOnly();
            for (int i = 0; i < _source.Count; i++)
            {
                Assert.AreEqual(_source[i], result[i]);
            }
        }
    }
}
