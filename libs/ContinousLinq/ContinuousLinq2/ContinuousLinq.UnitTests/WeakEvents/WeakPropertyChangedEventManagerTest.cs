using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ContinuousLinq.WeakEvents;
using System.ComponentModel;
using System.Threading;

namespace ContinuousLinq.UnitTests.WeakEvents
{
    [TestFixture]
    public class WeakPropertyChangedEventManagerTest
    {
        private ListenerStub _listener;
        private WeakReference _listenerReference;
        private WeakReference _personReference;
        private Person _person;
        
        [SetUp]
        public void Setup()
        {
            _listener = new ListenerStub();
            _person = new Person();

            _personReference = new WeakReference(_person);
            _listenerReference = new WeakReference(_listener);
        }

        [TearDown]
        public void TearDown()
        {
            _person = null;
            WeakPropertyChangedEventManager.SourceToBridgeTable.Clear();
            GC.Collect();
        }

        private void RegisterOnPersonName()
        {
            WeakPropertyChangedEventManager.Register(
                            _person,
                            "Name",
                            _listener,
                            (me, sender, args) => me.OnPropertyChanged(sender, args));
        }

        [Test]
        public void Register_MonitoredPropertyChanges_CallsCallback()
        {
            RegisterOnPersonName();

            _person.Name = "Bob";

            Assert.AreEqual(1, _listener.CallCount);
            Assert.AreSame(_person, _listener.Sender);
            Assert.IsNotNull(_listener.Args);
            Assert.AreEqual("Name", _listener.Args.PropertyName);
        }

        [Test]
        public void Register_UnmonitoredPropertyChanges_DoesNotCallCallback()
        {
            RegisterOnPersonName();
            _person.Age = 1;

            Assert.AreEqual(0, _listener.CallCount);
        }

        [Test]
        public void Register_ListenerReferencesDropped_ListenerGarbageCollected()
        {

            RegisterOnPersonName();
            
            _listener = null;
            GC.Collect();
            Assert.IsFalse(_listenerReference.IsAlive);
        }

        [Test]
        public void RegisterAndScheduleCleanup_SourceReferencesDropped_SourceGarbageCollected()
        {
           
            RegisterOnPersonName();
            
            _person = null;
            GC.Collect();
            Assert.IsFalse(_personReference.IsAlive);
        }

        [Test]
        public void Unregister_MonitoredPropertyChanged_DoesNotFireCallback()
        {
            RegisterOnPersonName();

            WeakPropertyChangedEventManager.Unregister(_person, "Name", _listener, null);
            _person.Name = "Bob";

            Assert.AreEqual(0, _listener.CallCount);
        }
        
        [Test]
        public void NullOutRefsToSourceAndCollect_Always_SourceCollected()
        {
            RegisterOnPersonName();
            _person = null;
            
            GC.Collect();

            Assert.IsFalse(_personReference.IsAlive);
        }

        [Test]
        [Ignore("This will fail when everything is run because the test fixtures hold on to references.")]
        public void CleanupReferences()
        {
            RegisterOnPersonName();
            WeakReference personRef = new WeakReference(_person);
            _person = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            Assert.IsFalse(personRef.IsAlive);

            WeakPropertyChangedEventManager.RemoveCollectedEntries();
            for (int i = 0; i < 10; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                WeakPropertyChangedEventManager.RemoveCollectedEntries();
            }

            Assert.AreEqual(0, WeakPropertyChangedEventManager.SourceToBridgeTable.Count);
        }
    }
}
