using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections.ObjectModel;
using ContinuousLinq;
using System.Collections.Specialized;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class VersionedLinkedListTest
    {
        private VersionedLinkedList<object> _target;
        private List<object> _testObjects;

        [SetUp]
        public void Setup()
        {
            _target = new VersionedLinkedList<object>();
            _testObjects = new List<object>()
		            {
		                new object(),
		                new object(),
		                new object(),
		            };
        }

        [Test]
        public void IsEmpty_HasItems_False()
        {
            _target.AddLast(_testObjects[0]);
            Assert.IsFalse(_target.IsEmpty);
        }

        [Test]
        public void IsEmpty_NoItems_True()
        {
            Assert.IsTrue(_target.IsEmpty);
        }

        [Test]
        public void AddLast_Once_AddsToList()
        {
            _target.AddLast(_testObjects[0]);

            Assert.AreEqual(_testObjects[0], _target.First.Value);
            Assert.AreEqual(_target.First, _target.Last);
            Assert.AreEqual(1, _target.Version);
            Assert.AreEqual(1, _target.First.Version);
        }

        [Test]
        public void AddLast_Twice_AddsToList()
        {
            _target.AddLast(_testObjects[0]);
            _target.AddLast(_testObjects[1]);

            Assert.AreEqual(_testObjects[0], _target.First.Value);
            Assert.AreEqual(_testObjects[1], _target.Last.Value);
            Assert.AreNotEqual(_target.First, _target.Last);

            Assert.AreEqual(1, _target.First.Version);
            Assert.AreEqual(2, _target.Last.Version);
            Assert.AreEqual(2, _target.Version);
        }

        [Test]
        public void Iterate_ListEmpty_ReturnsNull()
        {
            ulong versionToIterate = 1;

            var currentNode = _target.StartIterating(versionToIterate);
            Assert.IsNull(currentNode);
        }

        [Test]
        public void Iterate_ListHasOneMember_ReturnsFirstNode()
        {
            _target.AddLast(_testObjects[0]);

            ulong versionToIterate = _target.Version;

            var currentNode = _target.StartIterating(versionToIterate);
            Assert.AreEqual(_testObjects[0], currentNode.Value);
        }

        [Test]
        public void GetNext_ListHasOneMember_ReturnsNull()
        {
            _target.AddLast(_testObjects[0]);

            ulong versionToIterate = _target.Version;

            var currentNode = _target.StartIterating(versionToIterate);
            var nextNode = _target.GetNext(versionToIterate, currentNode);
            Assert.IsNull(nextNode);
        }

        [Test]
        public void GetNext_AddDuringIteration_OnlyIteratesCurrentMembers()
        {
            _target.AddLast(_testObjects[0]);

            ulong versionToIterate = _target.Version;

            var currentNode = _target.StartIterating(versionToIterate);

            _target.AddLast(_testObjects[0]);

            var nextNode = _target.GetNext(versionToIterate, currentNode);

            Assert.IsNull(nextNode);
        }

        [Test]
        public void GetNext_RemoveDuringIteration_DoesNotIncludeThoseMembers()
        {
            _target.AddLast(_testObjects[0]);
            _target.AddLast(_testObjects[1]);
            _target.AddLast(_testObjects[2]);

            ulong versionToIterate = _target.Version;

            var currentNode = _target.StartIterating(versionToIterate);

            _target.MarkForRemove(_target.Last.Previous);

            currentNode = _target.GetNext(versionToIterate, currentNode);
            Assert.AreEqual(_testObjects[2], currentNode.Value);

            currentNode = _target.GetNext(versionToIterate, currentNode);
            Assert.IsNull(currentNode);
        }

        [Test]
        public void StopIterating_RemoveDuringIteration_ExpungesListOfToBeDeleted()
        {
            _target.AddLast(_testObjects[0]);
            _target.AddLast(_testObjects[1]);
            _target.AddLast(_testObjects[2]);

            ulong versionToIterate = _target.Version;

            var firstNode = _target.StartIterating(versionToIterate);

            _target.MarkForRemove(firstNode);
            
            _target.StopIterating();

            Assert.AreNotEqual(firstNode, _target.First);
        }

        [Test]
        public void StopIterating_RemoveDuringIterationMiddleItem_ExpungesListOfToBeDeleted()
        {
            _target.AddLast(_testObjects[0]);
            _target.AddLast(_testObjects[1]);
            _target.AddLast(_testObjects[2]);

            ulong versionToIterate = _target.Version;

            var firstNode = _target.StartIterating(versionToIterate);

            _target.MarkForRemove(_target.Last.Previous);

            _target.StopIterating();

            Assert.AreEqual(_target.First.Next, _target.Last);
            Assert.AreEqual(_target.Last.Previous, _target.First);
        }

        [Test]
        public void StopIterating_RemoveAll_ExpungesListOfToBeDeleted()
        {
            _target.AddLast(_testObjects[0]);
            _target.AddLast(_testObjects[1]);

            ulong versionToIterate = _target.Version;

            var firstNode = _target.StartIterating(versionToIterate);

            _target.MarkForRemove(_target.First);
            _target.MarkForRemove(_target.Last);

            _target.StopIterating();

            Assert.IsNull(_target.First);
            Assert.IsNull(_target.Last);
        }

        [Test]
        public void StopIterating_RecursiveCaseAndRemoveDuringIteration_ExpungesListOfToBeDeletedAtEndOfRecursion()
        {
            _target.AddLast(_testObjects[0]);
            _target.AddLast(_testObjects[1]);
            _target.AddLast(_testObjects[2]);

            ulong versionToIterate = _target.Version;

            var firstNode = _target.StartIterating(versionToIterate);
            
            _target.StartIterating(versionToIterate);

            _target.MarkForRemove(firstNode);

            _target.StopIterating();
            Assert.AreEqual(firstNode, _target.First);
            
            _target.StopIterating();
            Assert.AreNotEqual(firstNode, _target.First);
        }

        [Test]
        public void MarkForRemove_NotIterating_RemovesImmediately()
        {
            _target.AddLast(_testObjects[0]);
            _target.AddLast(_testObjects[1]);

            ulong versionToIterate = _target.Version;

            var firstNode = _target.StartIterating(versionToIterate);

            _target.MarkForRemove(_target.First);
            _target.MarkForRemove(_target.Last);

            _target.StopIterating();

            Assert.IsNull(_target.First);
            Assert.IsNull(_target.Last);
        }

        public event Action<int> TestEvent;

        [Test]
        [Ignore("Semantics of standard C# events")]
        public void SubscribeDuringAnEventTest()
        {
            Action<int> subscribedDuringFiring = (arg) =>
            {
                Console.WriteLine("subscribedDuringFiring " + arg);
            };

            Action<int> subscribeAction = (arg) =>
            {
                Console.WriteLine("subscribeAction " + arg);
                if (arg == 0)
                    TestEvent += subscribedDuringFiring;
            };

            Action<int> refire = (arg) =>
            {
                Console.WriteLine("refire " + arg);
                if (arg == 0)
                    TestEvent(1);
            };

            TestEvent += subscribeAction;
            TestEvent += refire;

            TestEvent(0);
        }

        [Test]
        [Ignore("Semantics of standard C# events")]
        public void EventCopyTest()
        {
            Action<int> callback = (arg) =>
            {
                Console.WriteLine("callback " + arg);
            };

            Action<int> callback1 = (arg) =>
            {
                Console.WriteLine("callback1 " + arg);
            };

            TestEvent += callback;
            TestEvent += callback1;

            Action<int> copyOfTestEvent = TestEvent;

            TestEvent -= callback1;

            TestEvent(0);

            Console.WriteLine("\n\tCopy\n");

            copyOfTestEvent(0);

            TestEvent -= callback;
        }

        [Test]
        [Ignore("Semantics of standard C# events")]
        public void UnsubscribeDuringAnEventTest()
        {
            Action<int> unsubscribedDuringFiring = (arg) =>
            {
                Console.WriteLine("unsubscribedDuringFiring " + arg);
            };

            Action<int> unsubscribeAction = (arg) =>
            {
                Console.WriteLine("unsubscribeAction " + arg);
                if (arg == 0)
                    TestEvent -= unsubscribedDuringFiring;
            };

            Action<int> refire = (arg) =>
            {
                Console.WriteLine("refire " + arg);
                if (arg == 0)
                    TestEvent(1);
            };

            TestEvent += unsubscribeAction;
            TestEvent += unsubscribedDuringFiring;
            TestEvent += refire;

            TestEvent(0);
        }
    }
}
