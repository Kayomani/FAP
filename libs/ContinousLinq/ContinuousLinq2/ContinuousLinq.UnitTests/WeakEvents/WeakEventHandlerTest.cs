using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ContinuousLinq.WeakEvents;
using System.ComponentModel;

namespace ContinuousLinq.UnitTests.WeakEvents
{
    [TestFixture]
    public class WeakEventHandlerTest
    {
        [Test]
        public void Register_ListenerSetToNullAndGarbageCollected_ListenerSetToNull()
        {
            EventListenerStub listener = new EventListenerStub();
            WeakReference listenerRef = new WeakReference(listener);

            Person person = new Person();

            WeakEventHandler weakEventHandler = WeakPropertyChangedEventHandler.Register(
                person,
                (s, eh) => s.PropertyChanged -= eh,
                listener,
                (me, sender, ea) => me.OnPropertyChanged(sender, ea));
            
            listener = null;
            GC.Collect();

            Assert.IsFalse(listenerRef.IsAlive);
        }

        [Test]
        public void FirePropertyChangedEvent_Always_CallsCallback()
        {
            EventListenerStub listener = new EventListenerStub();

            Person person = new Person();

            WeakEventHandler weakEventHandler = WeakPropertyChangedEventHandler.Register(
                person,
                (s, eh) => s.PropertyChanged -= eh,
                listener,
                (me, sender, ea) => me.OnPropertyChanged(sender, ea));

            person.Name = "Bob";

            Assert.AreEqual(1, listener.EventHandlerCallCount);
            Assert.AreEqual("Name", listener.LastPropertyChanged);
        }
    }

    public class EventListenerStub
    {
        public int EventHandlerCallCount { get; set; }

        public string LastPropertyChanged { get; private set; }

        public void OnPropertyChanged(object sender, PropertyChangedEventArgs ea)
        {
            this.EventHandlerCallCount++;
            this.LastPropertyChanged = ea.PropertyName;
        }
    }
}
