using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContinuousLinq.WeakEvents;
using NUnit.Framework;

namespace ContinuousLinq.UnitTests.WeakEvents
{
    [TestFixture]
    public class WeakPropertyBridgeTest
    {
        private WeakPropertyBridge _target;

        private ListenerStub _listener;
        private Person _person;

        [SetUp]
        public void Setup()
        {
            _person = new Person();
            _target = new WeakPropertyBridge(_person);
            _listener = new ListenerStub();
        }

        [Test]
        public void AddListenerAndChangeMonitoredProperty_NewValue_CallsCallback()
        {
            _target.AddListener("Name", _listener, (me, sender, args) => me.OnPropertyChanged(sender, args));
            _person.Name = "Foooo";

            Assert.AreEqual(1, _listener.CallCount);
        }

        [Test]
        public void AddListenerAndChangeUnmonitoredProperty_NewValue_DoesNotCallCallback()
        {
            _target.AddListener("Name", _listener, (me, sender, args) => me.OnPropertyChanged(sender, args));
            _person.Age = 1123124;

            Assert.AreEqual(0, _listener.CallCount);
        }

        [Test]
        public void RemoveListenerAndChangedProperty_NewValue_DoesNotCallCallback()
        {
            _target.AddListener("Name", _listener, (me, sender, args) => me.OnPropertyChanged(sender, args));
            _target.RemoveListener(_listener, "Name", null);
            _person.Name = "Foooo";

            Assert.AreEqual(0, _listener.CallCount);
        }

        [Test]
        public void AddListenerAndCollect_NoListenerReferences_ListenerCollected()
        {
            WeakReference listenerRef = new WeakReference(_listener);
            _target.AddListener("Name", _listener, (me, sender, args) => me.OnPropertyChanged(sender, args));
            _listener = null;
            GC.Collect();
            Assert.IsFalse(listenerRef.IsAlive);
        }
    }
}
