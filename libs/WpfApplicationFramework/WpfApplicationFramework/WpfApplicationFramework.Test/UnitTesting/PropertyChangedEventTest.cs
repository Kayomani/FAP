using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Waf.UnitTesting;
using System.Waf.Foundation;
using System.ComponentModel;

namespace Test.Waf.UnitTesting
{
    [TestClass]
    public class PropertyChangedEventTest
    {
        public TestContext TestContext { get; set; }


        [TestMethod]
        public void PropertyChangedEventTest1()
        {
            Person person = new Person();
            AssertHelper.PropertyChangedEvent(person, x => x.Name, () => person.Name = "Luke");

            AssertHelper.ExpectedException<ArgumentNullException>(
                () => AssertHelper.PropertyChangedEvent((Person)null, x => x.Name, () => person.Name = "Han"));
            AssertHelper.ExpectedException<ArgumentNullException>(
                () => AssertHelper.PropertyChangedEvent(person, null, () => person.Name = "Han"));
            AssertHelper.ExpectedException<ArgumentNullException>(
                () => AssertHelper.PropertyChangedEvent(person, x => x.Name, null));
        }

        [TestMethod]
        public void PropertyChangedEventTest2()
        {
            WrongPerson wrongPerson = new WrongPerson();
            AssertHelper.ExpectedException<AssertException>(() =>
                AssertHelper.PropertyChangedEvent(wrongPerson, x => x.Name, () => wrongPerson.Name = "Luke"));

            AssertHelper.ExpectedException<AssertException>(() =>
                AssertHelper.PropertyChangedEvent(wrongPerson, x => x.Age, () => wrongPerson.Age = 31));

            AssertHelper.ExpectedException<AssertException>(() =>
                AssertHelper.PropertyChangedEvent(wrongPerson, x => x.Weight, () => wrongPerson.Weight = 80));

            Person person = new Person();
            AssertHelper.ExpectedException<ArgumentException>(() =>
                AssertHelper.PropertyChangedEvent(person, x => x.Name.Length, () => person.Name = "Luke"));
        }

        [TestMethod]
        public void WrongEventSenderTest()
        {
            WrongPerson person = new WrongPerson();
            AssertHelper.ExpectedException<AssertException>(() =>
                AssertHelper.PropertyChangedEvent(person, x => x.Name, () => person.RaiseWrongNamePropertyChanged()));
        }

        [TestMethod]
        public void WrongExpressionTest()
        {
            Person person = new Person();
            
            AssertHelper.ExpectedException<ArgumentException>(() =>
                AssertHelper.PropertyChangedEvent(person, x => x, () => person.Name = "Luke"));

            AssertHelper.ExpectedException<ArgumentException>(() =>
                AssertHelper.PropertyChangedEvent(person, x => x.ToString(), () => person.Name = "Luke"));

            AssertHelper.ExpectedException<ArgumentException>(() =>
                AssertHelper.PropertyChangedEvent(person, x => Math.Abs(1), () => person.Name = "Luke"));
        }



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

        private class WrongPerson : INotifyPropertyChanged
        {
            private string name;
            private double weight;


            public event PropertyChangedEventHandler PropertyChanged;


            public string Name
            {
                get { return name; }
                set
                {
                    if (name != value)
                    {
                        name = value;
                        OnPropertyChanged(new PropertyChangedEventArgs("WrongName"));
                    }
                }
            }

            public int Age { get; set; }

            public double Weight
            {
                get { return weight; }
                set
                {
                    if (weight != value)
                    {
                        weight = value;
                        OnPropertyChanged(new PropertyChangedEventArgs("Weight"));
                        OnPropertyChanged(new PropertyChangedEventArgs("Weight"));
                    }
                }
            }


            public void RaiseWrongNamePropertyChanged()
            {
                if (PropertyChanged != null) { PropertyChanged(null, new PropertyChangedEventArgs("Name")); }
            }

            protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
            {
                if (PropertyChanged != null) { PropertyChanged(this, e); }
            }
        }
    }
}
