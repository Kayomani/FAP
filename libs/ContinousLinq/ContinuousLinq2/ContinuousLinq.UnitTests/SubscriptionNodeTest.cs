using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class SubscriptionNodeTest
    {
        private Person _person;

        private SubscriptionNode _target;

        private PropertyAccessNode _ageAccessNode;
        
        private PropertyAccessNode _brotherAccessNode;
        private SubscriptionNode _brotherNode;
        
        private ParameterNode _parameterAccessNode;
        
        [SetUp]
        public void Setup()
        {
            _person = new Person();

            _ageAccessNode = new PropertyAccessNode(typeof(Person).GetProperty("Age"));
            _brotherAccessNode = new PropertyAccessNode(typeof(Person).GetProperty("Brother")) { Children = new List<PropertyAccessTreeNode>() { _ageAccessNode } };
            _parameterAccessNode = new ParameterNode(typeof(Person), "person") { Children = new List<PropertyAccessTreeNode>() { _brotherAccessNode } };

            _brotherNode = new SubscriptionNode() { AccessNode = _brotherAccessNode };
            _target = new SubscriptionNode() { AccessNode = _parameterAccessNode, Children = new List<SubscriptionNode>() { _brotherNode } };
            _target.Subject = _person;
        }

        [Test]
        public void ChangeFirstLevelProperty_TwoLevelPropertyTreeAndPropertyInTree_AllNodesSubscribed()
        {
            int callCount = 0;
            _target.PropertyChanged += () => callCount++;

            Person brother = new Person();
            _person.Brother = brother;

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void ChangeSecondLevelProperty_TwoLevelPropertyTreeAndPropertyInTree_AllNodesSubscribed()
        {
            Person brother = new Person(); 

            _person.Brother = brother;

            int parameterNodeCallCount = 0;
            _target.PropertyChanged += () => parameterNodeCallCount++;

            int brotherNodeCallCount = 0;
            _brotherNode.PropertyChanged += () => brotherNodeCallCount++;

            _person.Brother.Age = 100;

            Assert.AreEqual(0, parameterNodeCallCount);
            Assert.AreEqual(1, brotherNodeCallCount);
        }

        [Test]
        public void ChangeFirstLevelProperty_TwoLevelPropertyTreeAndPropertyNotInTree_EventNotFired()
        {
            int callCount = 0;
            _target.PropertyChanged += () => callCount++;

            Person brother = new Person();
            _person.Age = 12132231;

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void ChangeSecondLevelProperty_TwoLevelPropertyTreeAndPropertyNotInTree_EventNotFired()
        {
            Person brother = new Person();

            _person.Brother = brother;

            int parameterNodeCallCount = 0;
            _target.PropertyChanged += () => parameterNodeCallCount++;

            int brotherNodeCallCount = 0;
            _brotherNode.PropertyChanged += () => brotherNodeCallCount++;

            _person.Brother.Name = "adfja";

            Assert.AreEqual(0, parameterNodeCallCount);
            Assert.AreEqual(0, brotherNodeCallCount);
        }

        [Test]
        public void Unsubscribe_TwoLevelPropertyTree_EventNotFired()
        {
            Person brother = new Person();
            _person.Brother = brother;
            _person.Brother = null;

            int parameterNodeCallCount = 0;
            _target.PropertyChanged += () => parameterNodeCallCount++;

            int brotherNodeCallCount = 0;
            _brotherNode.PropertyChanged += () => brotherNodeCallCount++;

            brother.Age = 1092213;

            Assert.AreEqual(0, parameterNodeCallCount);
            Assert.AreEqual(0, brotherNodeCallCount);
        }
    }
}