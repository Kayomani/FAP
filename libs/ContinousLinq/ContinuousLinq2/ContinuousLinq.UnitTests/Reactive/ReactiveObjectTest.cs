using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ContinuousLinq.Reactive;
using System.ComponentModel;

namespace ContinuousLinq.UnitTests.Reactive
{
    [TestFixture]
    public class ReactiveObjectTest
    {
        private ReactivePerson _target;

        [SetUp]
        public void Initialize()
        {
            _target = new ReactivePerson();
        }

        [TearDown]
        public void Cleanup()
        {
        }

        [Test]
        public void TestPersonsAreUnique()
        {
            Person p1 = new Person();
            Person p2 = new Person();

            Assert.AreNotEqual(p1, p2);
        }

        [Test]
        public void OnPropertyChangedRaisesPropertyChangedEvent()
        {
            bool eventFired = false;
            string propertyFired = "";

            _target.PropertyChanged +=
                delegate(object sender, PropertyChangedEventArgs e)
                {
                    eventFired = true;
                    propertyFired = e.PropertyName;
                };

            _target.Age++;
            Assert.IsTrue(eventFired);
            Assert.AreEqual("Age", propertyFired);
        }

        [Test]
        public void DependsOn_AgeSetToNewValue_OnAgeChangedCalledOnce()
        {
            _target.Age = 1000;
            Assert.AreEqual(1, _target.OnAgeChangedCalledCount);
        }

        [Test]
        public void DependsOn_BrotherSetToNewValue_OnBrotherChangedCalledOnce()
        {
            _target.Brother = new Person();
            Assert.AreEqual(1, _target.OnBrotherChangedCalledCount);
        }

        [Test]
        public void DependsOn_BrotherSetToNewValue_OnBrotherAgeChangedCalledOnce()
        {
            _target.Brother = new Person();
            Assert.AreEqual(1, _target.OnBrotherAgeChangedCalledCount);
        }

        [Test]
        public void DependsOn_BrotherAgeSetToNewValue_OnBrotherAgeChangedCalledTwice()
        {
            _target.Brother = new Person();
            _target.Brother.Age = 10;
            Assert.AreEqual(2, _target.OnBrotherAgeChangedCalledCount);
        }

        [Test]
        public void DependsOn_BrotherAgeSetToNewValue_OnBrotherChangedCalledOnce()
        {
            _target.Brother = new Person();
            _target.Brother.Age = 10;
            Assert.AreEqual(1, _target.OnBrotherChangedCalledCount);
        }

        [Test]
        public void DependsOn_BrotherSetToNewValueThenBackToNull_OnBrotherChangedCalledTwice()
        {
            _target.Brother = new Person();
            _target.Brother = null;
            Assert.AreEqual(2, _target.OnBrotherChangedCalledCount);
        }

        [Test]
        public void DependsOn_BrotherSetToNewValueThenBackToNull_OnBrotherAgeChangedCalledTwice()
        {
            _target.Brother = new Person();
            _target.Brother = null;
            Assert.AreEqual(2, _target.OnBrotherAgeChangedCalledCount);
        }

        [Test]
        public void DependsOn_BrotherSetToNewValueThenBackToNullAndAgeChangedAfterward_DoesNotTriggerAnyDependsOnMethods()
        {
            Person brother = new Person();
            _target.Brother = brother;
            _target.Brother = null;
            brother.Age = 10;
            Assert.AreEqual(2, _target.OnBrotherChangedCalledCount);
            Assert.AreEqual(2, _target.OnBrotherAgeChangedCalledCount);
        }


        //[Test]
        ////[ExpectedException(typeof(InvalidProgramException))]
        //public void ConstructDerivedClass_DerivedClassMethodSignatureMissingCorrectArguments_ThrowsException()
        //{
        //    try
        //    {
        //        ClassWithIncorrectDependsOn classWithIncorrectDependsOn = new ClassWithIncorrectDependsOn();
        //    }
        //    catch(Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //    }
        //}

        [Test]
        public void DependsOn_ReactiveBrotherSetToNewValue_OnReactiveBrotherChangedCalledOnce()
        {
            _target.ReactiveBrother = new ReactivePerson();
            Assert.AreEqual(1, _target.OnReactiveBrotherChangedCalledCount);
        }

        [Test]
        public void DependsOn_ReactiveBrotherSetToNewValue_OnReactiveBrotherAgeChangedCalledOnce()
        {
            _target.ReactiveBrother = new ReactivePerson();
            Assert.AreEqual(1, _target.OnReactiveBrotherAgeChangedCalledCount);
        }

        [Test]
        public void DependsOn_ReactiveBrotherAgeSetToNewValue_OnReactiveBrotherAgeChangedCalledTwice()
        {
            _target.ReactiveBrother = new ReactivePerson();
            _target.ReactiveBrother.Age = 10;
            Assert.AreEqual(2, _target.OnReactiveBrotherAgeChangedCalledCount);
        }

        [Test]
        public void DependsOn_ReactiveBrotherAgeSetToNewValue_OnReactiveBrotherChangedCalledOnce()
        {
            _target.ReactiveBrother = new ReactivePerson();
            _target.ReactiveBrother.Age = 10;
            Assert.AreEqual(1, _target.OnReactiveBrotherChangedCalledCount);
        }

        [Test]
        public void DependsOn_ReactiveBrotherSetToNewValueThenBackToNull_OnReactiveBrotherChangedCalledTwice()
        {
            _target.ReactiveBrother = new ReactivePerson();
            _target.ReactiveBrother = null;
            Assert.AreEqual(2, _target.OnReactiveBrotherChangedCalledCount);
        }

        [Test]
        public void DependsOn_ReactiveBrotherSetToNewValueThenBackToNull_OnReactiveBrotherAgeChangedCalledTwice()
        {
            _target.ReactiveBrother = new ReactivePerson();
            _target.ReactiveBrother = null;
            Assert.AreEqual(2, _target.OnReactiveBrotherAgeChangedCalledCount);
        }

        [Test]
        public void DependsOn_ReactiveBrotherSetToNewValueThenBackToNullAndAgeChangedAfterward_DoesNotTriggerAnyDependsOnMethods()
        {
            ReactivePerson ReactiveBrother = new ReactivePerson();
            _target.ReactiveBrother = ReactiveBrother;
            _target.ReactiveBrother = null;
            ReactiveBrother.Age = 10;
            Assert.AreEqual(2, _target.OnReactiveBrotherChangedCalledCount);
            Assert.AreEqual(2, _target.OnReactiveBrotherAgeChangedCalledCount);
        }

        [Test]
        public void DependsOn_TwoLevelTreeWithVaryingTypes_FiresUpdatesProperly()
        {
            var root = new ReactiveType0();
            var child = new ReactiveType1();
            var person = new ReactivePerson();

            root.ChildReactiveType1 = child;
            child.ChildReactivePerson = person;
            person.Name = "Jim";
            //child.Name = "Bob";
            
            Assert.AreEqual(3, root.Updates);
        }

        public class ReactiveType0 : ReactiveObject
        {
            private string _name;
            public string Name
            {
                get { return _name; }
                set
                {
                    if (value == _name)
                        return;

                    OnPropertyChanging("Name");
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }

            private ReactiveType1 _childReactiveType1;
            public ReactiveType1 ChildReactiveType1
            {
                get { return _childReactiveType1; }
                set
                {
                    if (value == _childReactiveType1)
                        return;

                    OnPropertyChanging("ChildReactiveType1");
                    _childReactiveType1 = value;
                    OnPropertyChanged("ChildReactiveType1");
                }
            }

            public int Updates { get; set; }

            static ReactiveType0()
            {
                var dependsOn = Register<ReactiveType0>();
                dependsOn.Call(obj => obj.Updates++)
                    .OnChanged(obj => obj.ChildReactiveType1.ChildReactivePerson.Name);
            }
        }

        public class ReactiveType1 : ReactiveObject
        {
            private string _name;
            public string Name
            {
                get { return _name; }
                set
                {
                    if (value == _name)
                        return;

                    OnPropertyChanging("Name");
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }

            private ReactivePerson _childReactivePerson;
            public ReactivePerson ChildReactivePerson
            {
                get { return _childReactivePerson; }
                set
                {
                    if (value == _childReactivePerson)
                        return;

                    OnPropertyChanging("ChildReactivePerson");
                    _childReactivePerson = value;
                    OnPropertyChanged("ChildReactivePerson");
                }
            }
        }

        public class ReactivePerson : ReactiveObject
        {
            private int _age;
            private string _name;

            public string Name
            {
                get { return _name; }
                set
                {
                    if (value == _name)
                        return;

                    OnPropertyChanging("Name");
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }

            public int Age
            {
                get { return _age; }
                set
                {
                    if (value == _age)
                        return;
                    OnPropertyChanging("Age");
                    _age = value;
                    OnPropertyChanged("Age");
                }
            }

            static ReactivePerson()
            {
                var dependsOn = Register<ReactivePerson>();

                dependsOn.Call(obj => obj.OnAgeChanged())
                    .OnChanged(obj => obj.Age);

                dependsOn.Call(obj => obj.OnBrotherChanged())
                    .OnChanged(obj => obj.Brother);

                dependsOn.Call(obj => obj.OnBrotherAgeChanged())
                    .OnChanged(obj => obj.Brother.Age);

                dependsOn.Call(obj => obj.OnReactiveBrotherChanged())
                    .OnChanged(obj => obj.ReactiveBrother);

                dependsOn.Call(obj => obj.OnReactiveBrotherAgeChanged())
                    .OnChanged(obj => obj.ReactiveBrother.Age);
            }

            private Person _brother;
            public Person Brother
            {
                get { return _brother; }
                set
                {
                    if (value == _brother)
                        return;

                    OnPropertyChanging("Brother");
                    _brother = value;
                    OnPropertyChanged("Brother");
                }
            }

            private ReactivePerson _reactiveBrother;
            public ReactivePerson ReactiveBrother
            {
                get { return _reactiveBrother; }
                set
                {
                    if (value == _reactiveBrother)
                        return;
                    OnPropertyChanging("ReactiveBrother");
                    _reactiveBrother = value;
                    OnPropertyChanged("ReactiveBrother");
                }
            }

            public int OnAgeChangedCalledCount { get; set; }
            private void OnAgeChanged()
            {
                this.OnAgeChangedCalledCount++;
            }

            public int OnBrotherChangedCalledCount { get; set; }
            private void OnBrotherChanged()
            {
                this.OnBrotherChangedCalledCount++;
            }

            public int OnBrotherAgeChangedCalledCount { get; set; }
            private void OnBrotherAgeChanged()
            {
                this.OnBrotherAgeChangedCalledCount++;
            }

            public int OnReactiveBrotherChangedCalledCount { get; set; }
            private void OnReactiveBrotherChanged()
            {
                this.OnReactiveBrotherChangedCalledCount++;
            }

            public int OnReactiveBrotherAgeChangedCalledCount { get; set; }
            private void OnReactiveBrotherAgeChanged()
            {
                this.OnReactiveBrotherAgeChangedCalledCount++;
            }
        }

        [Test]
        public void Test()
        {
            var myClass = new MyClass();
            myClass.CallCount = 0;
            
            int callCount = 0;
            myClass.PropertyChanged += (sender, args) => callCount++;

            myClass.Collection.Add(1);

            Assert.AreEqual(1, myClass.CallCount);
        }

        public class MyClass : ReactiveObject
        {

            private ContinuousCollection<int> _collection;
            public ContinuousCollection<int> Collection
            {
                get { return _collection; }
                set
                {
                    if (value == _collection)
                        return;

                    _collection = value;
                    OnPropertyChanged("Collection");
                }
            }
            private int _callCount;
            public int CallCount
            {
                get { return _callCount; }
                set
                {
                    if (value == _callCount)
                        return;

                    _callCount = value;
                    OnPropertyChanged("CallCount");
                }
            }
            
            public MyClass()
            {
                this.Collection = new ContinuousCollection<int>();
            }

            static MyClass()
            {
                var dependsOn = Register<MyClass>();
                dependsOn.Call(obj => obj.UpdateCollection())
                    .OnChanged(obj => obj.Collection.Count);
            }
            
            public void UpdateCollection()
            {
                this.CallCount++;
            }

        }
    }
}
