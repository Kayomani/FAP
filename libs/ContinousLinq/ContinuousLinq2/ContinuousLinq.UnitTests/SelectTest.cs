using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.ObjectModel;

namespace ContinuousLinq.UnitTests
{

    [TestFixture]
    public class SelectTest
    {
        private ObservableCollection<Person> _source;
        
        [SetUp]
        public void Setup()
        {
            _source = ClinqTestFactory.CreateTwoPersonSource();
        }

        [Test]
        public void Select_SimplePassThrough_InputContentsMatchOutputContents()
        {
            ReadOnlyContinuousCollection<Person> output = from person in _source
                                                          select person;

            Assert.AreEqual(_source.Count, output.Count);

            for (int i = 0; i < _source.Count; i++)
            {
                Assert.AreSame(_source[i], output[i]);
            }
        }

        [Test]
        public void Select_ReadOnlyObservableCollection_InputContentsMatchOutputContents()
        {
            var readOnlySource = new ReadOnlyObservableCollection<Person>(_source);
            ReadOnlyContinuousCollection<Person> output = from person in readOnlySource
                                                          select person;
            Assert.AreEqual(readOnlySource, output);
        }

        [Test]
        public void Select_SourceContainsNonNotifyingObject_OnlyMonitorsCollectionChanges()
        {
            ObservableCollection<int> source = new ObservableCollection<int> { 0, 1, 2, 3 };
            ReadOnlyContinuousCollection<int> result = source.Select(item => item);
            int callCount = 0;
            result.CollectionChanged += (sender, args) => callCount++;
            
            source.Add(4);

            Assert.AreEqual(1, callCount); 
            Assert.AreEqual(4, result[4]);
        }

        [Test]
        public void DropReference_Always_GarbageCollectsResultCollection()
        {
            var ageCollection = from person in _source
                                select person.Age;

            // LINQ execution is deferred.  This will execute query.
            int count = ageCollection.Count();

            var weakReference = new WeakReference(ageCollection);
            Assert.IsTrue(weakReference.IsAlive);

            ageCollection = null;
            GC.Collect();
            Assert.IsFalse(weakReference.IsAlive);
        }

        [Test]
        public void SelectorCreatesNewObject_Always_ReturnsSameInstance()
        {
            var personCollection = from person in _source
                                   select new Person();

            Person personAtZero = personCollection[0];
            Assert.AreSame(personAtZero, personCollection[0]);
        }
        
        [Test]
        public void SelectorCreatesNewObjectAndAdds_Always_ReturnsSameInstance()
        {
            var personCollection = from person in _source
                                   select new Person();

            var personBeforeAdd = personCollection[0];
            _source.Add(new Person());

            Assert.AreSame(personBeforeAdd, personCollection[0]);
        }

        [Test]
        public void LetClause()
        {
            foreach (var item in _source)
            {
                item.Brother = new Person();
            }

            var personCollection = from person in _source
                                   select person;
            
        }
    }
}
