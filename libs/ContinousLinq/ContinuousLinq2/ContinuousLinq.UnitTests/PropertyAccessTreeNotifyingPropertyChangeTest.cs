using System;
using NUnit.Framework;
using System.Reflection;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class PropertyAccessTreeNotifyingPropertyChangeTest
    {
        private PropertyAccessTree _target;
        private NotifyingPerson _person;

        private PropertyInfo _ageProperty;
        private PropertyAccessNode _agePropertyAccessNode;

        private PropertyInfo _brotherProperty;
        private PropertyAccessNode _brotherPropertyAccessNode;

        [SetUp]
        public void Setup()
        {
            _target = new PropertyAccessTree();
            _person = new NotifyingPerson();
            _ageProperty = typeof(NotifyingPerson).GetProperty("Age");
            _brotherProperty = typeof(NotifyingPerson).GetProperty("Brother");
        }

        private void InitializeTargetJustAgeAccess()
        {
            ParameterNode parameterNode = new ParameterNode(typeof(NotifyingPerson), "person");
            _target.Children.Add(parameterNode);

            _agePropertyAccessNode = new PropertyAccessNode(_ageProperty);
            parameterNode.Children.Add(_agePropertyAccessNode);
        }

        private void InitializeTargetBrothersAgeAccess()
        {
            ParameterNode parameterNode = new ParameterNode(typeof(NotifyingPerson), "person");
            _target.Children.Add(parameterNode);

            _brotherPropertyAccessNode = new PropertyAccessNode(_brotherProperty);
            parameterNode.Children.Add(_brotherPropertyAccessNode);

            _agePropertyAccessNode = new PropertyAccessNode(_ageProperty);
            _brotherPropertyAccessNode.Children.Add(_agePropertyAccessNode);

        }

        [Test]
        public void SubscribeToChanges_OneFirstLevelPropertyChanges_FiresExpectedChange()
        {
            InitializeTargetJustAgeAccess();

            NotifyingPerson person = new NotifyingPerson();

            int callCount = 0;
            var callbackSubscription = _target.CreateCallbackSubscription<PropertyAccessTreeNotifyingPropertyChangeTest>(
                (me, sender) =>
                {
                    Assert.AreEqual(this, me);
                    Assert.AreEqual(person, sender);
                    callCount++;
                });

            callbackSubscription.SubscribeToChanges(
                person,
                this);

            person.Age = 1234;

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void SubscribeToChanges_OneFirstLevelPropertyChangesWithTwoLevelAccessTree_FiresExpectedChange()
        {
            InitializeTargetBrothersAgeAccess();

            NotifyingPerson person = new NotifyingPerson();
            person.Brother = new NotifyingPerson();

            int callCount = 0;
            var callbackSubscription = _target.CreateCallbackSubscription<PropertyAccessTreeNotifyingPropertyChangeTest>(
                (me, sender) =>
                {
                    Assert.AreEqual(this, me);
                    Assert.AreEqual(person, sender);
                    callCount++;
                });

            callbackSubscription.SubscribeToChanges(
                person,
                this);

            person.Brother = new NotifyingPerson();

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void SubscribeToChanges_OneSecondLevelPropertyChanges_FiresExpectedChange()
        {
            InitializeTargetBrothersAgeAccess();

            NotifyingPerson person = new NotifyingPerson();
            person.Brother = new NotifyingPerson();

            int callCount = 0;
            var callbackSubscription = _target.CreateCallbackSubscription<PropertyAccessTreeNotifyingPropertyChangeTest>(
                (me, sender) =>
                {
                    Assert.AreEqual(this, me);
                    Assert.AreEqual(person, sender);
                    callCount++;
                });

            callbackSubscription.SubscribeToChanges(
                person,
                this);

            person.Brother.Age = 1234;

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void UnsubscribeFromChanges_OneFirstLevelPropertyChanges_DoesNotFireChange()
        {
            InitializeTargetJustAgeAccess();

            NotifyingPerson person = new NotifyingPerson();

            int callCount = 0;
            var callbackSubscription = _target.CreateCallbackSubscription<PropertyAccessTreeNotifyingPropertyChangeTest>(
                (me, sender) =>
                {
                    Assert.AreEqual(this, me);
                    Assert.AreEqual(person, sender);
                    callCount++;
                });

            callbackSubscription.SubscribeToChanges(
                person,
                this);


            callbackSubscription.UnsubscribeFromChanges(person, this);

            person.Age = 1234;

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void UnsubscribeFromChanges_OneFirstLevelPropertyChangesWithTwoLevelAccessTree_DoesNotFireChange()
        {
            InitializeTargetBrothersAgeAccess();

            NotifyingPerson person = new NotifyingPerson();
            person.Brother = new NotifyingPerson();

            int callCount = 0;
            var callbackSubscription = _target.CreateCallbackSubscription<PropertyAccessTreeNotifyingPropertyChangeTest>(
                (me, sender) =>
                {
                    Assert.AreEqual(this, me);
                    Assert.AreEqual(person, sender);
                    callCount++;
                });

            callbackSubscription.SubscribeToChanges(
                person,
                this);

            callbackSubscription.UnsubscribeFromChanges(person, this);

            person.Brother = new NotifyingPerson();

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void UnsubscribeFromChanges_OneSecondLevelPropertyChanges_DoesNotFireChange()
        {
            InitializeTargetBrothersAgeAccess();

            NotifyingPerson person = new NotifyingPerson();
            person.Brother = new NotifyingPerson();

            int callCount = 0;
            var callbackSubscription = _target.CreateCallbackSubscription<PropertyAccessTreeNotifyingPropertyChangeTest>(
                (me, sender) =>
                {
                    Assert.AreEqual(this, me);
                    Assert.AreEqual(person, sender);
                    callCount++;
                });

            callbackSubscription.SubscribeToChanges(
                person,
                this);

            callbackSubscription.UnsubscribeFromChanges(person, this);

            person.Brother.Age = 1234;

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void SubscribeToChanges_AllReferencesToSubjectDropped_SubjectIsGarbageCollected()
        {
            InitializeTargetJustAgeAccess();

            NotifyingPerson person = new NotifyingPerson();
            WeakReference personRef = new WeakReference(person);

            var callbackSubscription = _target.CreateCallbackSubscription<PropertyAccessTreeNotifyingPropertyChangeTest>(
                (me, sender) => { });

            callbackSubscription.SubscribeToChanges(
                person,
                this);

            person = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.IsFalse(personRef.IsAlive);
        }

        [Test]
        public void SubscribeToChanges_TwoLevelAndFirstLevelReferenceDropped_SubjectIsGarbageCollected()
        {
            InitializeTargetBrothersAgeAccess();

            NotifyingPerson person = new NotifyingPerson();
            WeakReference personRef = new WeakReference(person);

            NotifyingPerson brother = new NotifyingPerson();
            WeakReference brotherRef = new WeakReference(brother);

            person.Brother = brother;

            var callbackSubscription = _target.CreateCallbackSubscription<PropertyAccessTreeNotifyingPropertyChangeTest>(
                (me, sender) => {});

            callbackSubscription.SubscribeToChanges(
                person,
                this);

            brother = null;
            person = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.IsFalse(personRef.IsAlive);
            Assert.IsFalse(brotherRef.IsAlive);
        }
    
        [Test]
        public void SubscribeToChanges_TwoLevelAndSecondLevelReferenceDropped_SecondLevelIsGarbageCollected()
        {
            InitializeTargetBrothersAgeAccess();

            NotifyingPerson person = new NotifyingPerson();
            WeakReference personRef = new WeakReference(person);

            NotifyingPerson brother = new NotifyingPerson();
            WeakReference brotherRef = new WeakReference(brother);

            person.Brother = brother;

            var callbackSubscription = _target.CreateCallbackSubscription<PropertyAccessTreeNotifyingPropertyChangeTest>(
                (me, sender) => {});

            callbackSubscription.SubscribeToChanges(
                person,
                this);

            brother = null;
            person.Brother = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.IsFalse(brotherRef.IsAlive);
        }

        [Test]
        public void DoesEntireTreeSupportINotifyPropertyChanging_OneLevelAndDoes_ReturnsTrue()
        {
            InitializeTargetJustAgeAccess();
            Assert.IsTrue(_target.DoesEntireTreeSupportINotifyPropertyChanging);
        }

        [Test]
        public void DoesEntireTreeSupportINotifyPropertyChanging_TwoLevelAndDoes_ReturnsTrue()
        {
            InitializeTargetBrothersAgeAccess();
            Assert.IsTrue(_target.DoesEntireTreeSupportINotifyPropertyChanging);
        }
    }
}
