using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections.ObjectModel;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class ReferenceCountTrackerTest
    {
        private ReferenceCountTracker<Person> _target;

        [SetUp]
        public void Setup()
        {
            _target = new ReferenceCountTracker<Person>();
        }

        [Test]
        public void Add_ItemNotCurrentlyBeingTracked_ReturnsTrue()
        {
            Person person = new Person();
            Assert.IsTrue(_target.Add(person));
        }

        [Test]
        public void Add_ItemCurrentlyBeingTracked_ReturnsFalse()
        {
            Person person = new Person();
            _target.Add(person);
            Assert.IsFalse(_target.Add(person));
        }

        [Test]
        public void Remove_ItemCurrentlyBeingTrackedAndLastReferenceCount_ReturnsTrue()
        {
            Person person = new Person();
            _target.Add(person);
            Assert.IsTrue(_target.Remove(person));
        }

        [Test]
        public void Remove_ItemReferenceCountIsTwo_ReturnsFalse()
        {
            Person person = new Person();
            _target.Add(person);
            _target.Add(person);
            Assert.IsFalse(_target.Remove(person));
        }

        [Test]
        public void Clear_ItemReferenceCountIsTwo_NextAddReturnsTrue()
        {
            Person person = new Person();
            _target.Add(person);
            _target.Add(person);

            _target.Clear();

            Assert.IsTrue(_target.Add(person));
        }
    }
}
