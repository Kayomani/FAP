using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Waf.UnitTesting;
using System.ComponentModel;
using System.Diagnostics;
using System.Waf.Foundation;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Waf;

namespace Test.Waf.Domain
{
    [TestClass]
    public class ModelTest
    {
        private bool originalDebugSetting;
        
        
        public TestContext TestContext { get; set; }


        [TestInitialize]
        public void TestInitialize()
        {
            originalDebugSetting = WafConfiguration.Debug;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            WafConfiguration.Debug = originalDebugSetting;
        }


        [TestMethod]
        public void RaisePropertyChangedTest()
        {
            Person luke = new Person();

            WafConfiguration.Debug = true;
            AssertHelper.PropertyChangedEvent(luke, x => x.Name, () => luke.Name = "Luke");

            WafConfiguration.Debug = false;
            AssertHelper.PropertyChangedEvent(luke, x => x.Name, () => luke.Name = "Skywalker");
        }

        [TestMethod]
        public void AddAndRemoveEventHandler()
        {
            Person luke = new Person();
            bool eventRaised;

            PropertyChangedEventHandler eventHandler = (sender, e) =>
            {
                eventRaised = true;
            };

            eventRaised = false;
            luke.PropertyChanged += eventHandler;
            luke.Name = "Luke";
            Assert.IsTrue(eventRaised, "The property changed event needs to be raised");

            eventRaised = false;
            luke.PropertyChanged -= eventHandler;
            luke.Name = "Luke Skywalker";
            Assert.IsFalse(eventRaised, "The event handler must not be called because it was removed from the event.");
        }

        [TestMethod]
        public void WrongPropertyName()
        {
            WrongDocument document = new WrongDocument();

            WafConfiguration.Debug = true;
            AssertHelper.ExpectedException<InvalidOperationException>(() => 
                document.Header = "WPF Application Framework");

            WafConfiguration.Debug = false;
            document.Header = "WPF Application Framework (WAF)";
        }

        [TestMethod]
        public void SerializationTest()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (MemoryStream stream = new MemoryStream())
            {
                Person person = new Person() { Name = "Hugo" };
                formatter.Serialize(stream, person);

                stream.Position = 0;
                Person newPerson = (Person)formatter.Deserialize(stream);
                Assert.AreEqual(person.Name, newPerson.Name);
            }
        }



        [Serializable]
        private class Person : Model
        {
            private string name;

            public string Name
            {
                get { return name; }
                set
                {
                    if (name != value)
                    {
                        name = value;
                        RaisePropertyChanged("Name");
                    }
                }
            }
        }

        private class WrongDocument : Model
        {
            private string header;

            public string Header
            {
                get { return header; }
                set
                {
                    if (header != value)
                    {
                        header = value;
                        RaisePropertyChanged("WrongPropertyName");
                    }
                }
            }
        }
    }
}
