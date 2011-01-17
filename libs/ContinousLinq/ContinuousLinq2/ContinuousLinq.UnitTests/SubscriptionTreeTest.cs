using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class SubscriptionTreeTest
    {
        private SubscriptionTree _target;
        private Person _person;

        private PropertyAccessNode _ageAccessNode;

        private PropertyAccessNode _brotherAccessNode;
        private SubscriptionNode _brotherNode;

        private ParameterNode _parameterAccessNode;
        private SubscriptionNode _parameterNode;

        [SetUp]
        public void Setup()
        {
            _person = new Person();

            _ageAccessNode = new PropertyAccessNode(typeof(Person).GetProperty("Age"));
            _brotherAccessNode = new PropertyAccessNode(typeof(Person).GetProperty("Brother")) { Children = new List<PropertyAccessTreeNode>() { _ageAccessNode } };
            _parameterAccessNode = new ParameterNode(typeof(Person), "person") { Children = new List<PropertyAccessTreeNode>() { _brotherAccessNode } };

            _brotherNode = new SubscriptionNode() { AccessNode = _brotherAccessNode };
            _parameterNode = new SubscriptionNode() { AccessNode = _parameterAccessNode, Children = new List<SubscriptionNode>() { _brotherNode } };
            _parameterNode.Subject = _person;

            List<SubscriptionNode> nodes = new List<SubscriptionNode>() { _parameterNode };

            _target = new SubscriptionTree(_person, nodes);
        }

        [Test]
        public void Construct_ChangeFirstLevel_AllNodesSubscribed()
        {
            int callCount = 0;
            _target.PropertyChanged += (sender) => callCount++;

            _person.Brother = new Person();

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void Construct_TwoLevelPropertyTree_AllNodesSubscribed()
        {
            _person.Brother = new Person();

            int callCount = 0;
            _target.PropertyChanged += (sender) => callCount++;

            _person.Brother.Age = 11;

            Assert.AreEqual(1, callCount);
        }
    }
}
