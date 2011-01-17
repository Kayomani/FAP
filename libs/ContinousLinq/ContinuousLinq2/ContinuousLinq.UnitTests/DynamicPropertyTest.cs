using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ContinuousLinq.Expressions;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class DynamicPropertyTest
    {
        private DynamicProperty _target;
        private Person _person;
        [SetUp]
        public void Setup()
        {
            _person = new Person();
        }
        
        [TearDown]
        public void TearDown()
        {
            DynamicProperty.ClearCaches();
        }

        [Test]
        public void GetValue_PropertyIsValueType_GetsBoxedAndReturnsCorrectValue()
        {
            _target = DynamicProperty.Create(typeof(Person), "Age");
            _person.Age = 100;
            object age = _target.GetValue(_person);
            Assert.AreEqual(100, age);
        }

        [Test]
        public void GetValue_PropertyIsReferenceType_GetsBoxedAndReturnsCorrectValue()
        {
            Person brother = new Person();
            _person.Brother = brother;

            _target = DynamicProperty.Create(typeof(Person), "Brother");
            object brotherDynamic = _target.GetValue(_person);
            Assert.AreSame(brother, brotherDynamic);
        }

        [Test]
        public void Create_CalledTwice_ReturnsCachedProperty()
        {
            DynamicProperty one = DynamicProperty.Create(typeof(Person), "Brother");
            DynamicProperty two = DynamicProperty.Create(typeof(Person), "Brother");

            Assert.AreSame(one, two);
        }

        [Test]
        public void Create_CalledTwiceWithPropertyInfo_ReturnsCachedProperty()
        {
            DynamicProperty one = DynamicProperty.Create(typeof(Person).GetProperty("Brother"));
            DynamicProperty two = DynamicProperty.Create(typeof(Person).GetProperty("Brother"));

            Assert.AreSame(one, two);
        }
    }
}
