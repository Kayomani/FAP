using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ContinuousLinq;
using ContinuousLinq.Collections;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class DistinctTest
    {
        private ObservableCollection<Person> _source;

        [SetUp]
        public void Setup()
        {
            _source = ClinqTestFactory.CreateSixPersonSourceWithDuplicates();
        }

        [Test]
        public void Construct_Always_ReturnsReadOnlyContinuousCollection()
        {
            ReadOnlyContinuousCollection<Person> result = _source.Distinct();
            Assert.IsInstanceOfType(typeof(DistinctReadOnlyContinuousCollection<Person>), result);
        }
    }
}
