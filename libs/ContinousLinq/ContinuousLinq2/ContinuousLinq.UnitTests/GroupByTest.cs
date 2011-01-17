using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ContinuousLinq;
using ContinuousLinq.Collections;

namespace ContinuousLinq.UnitTests
{
    [TestFixture]
    public class GroupByTest
    {
        private ObservableCollection<Person> _source;

        [SetUp]
        public void Setup()
        {
            _source = ClinqTestFactory.CreateGroupablePersonSource();
        }

        [Test]
        public void GroupBy_DotSyntax_CountGroups()
        {
            GroupingReadOnlyContinuousCollection<int, Person> liveGroup =
                _source.GroupBy(p => p.Age);

            Assert.AreEqual(10, liveGroup.Count);
            Assert.AreEqual(6, liveGroup[5].Count);

        }

        [Test]
        public void GroupBy_CountGroups()
        {
            GroupingReadOnlyContinuousCollection<int, Person> liveGroup =
                from p in _source
                group p by p.Age;

            Assert.AreEqual(10, liveGroup.Count);
            Assert.AreEqual(6, liveGroup[5].Count);

        }

        [Test]
        public void ChangeItemInExistingGroup_NewValueNotMatchingCurrentKey_NewGroupFormed()
        {
            GroupingReadOnlyContinuousCollection<int, Person> liveGroup =
                from p in _source
                group p by p.Age;

            _source[0].Age = 999;

            Assert.AreEqual(11, liveGroup.Count);
            Assert.AreEqual(1, liveGroup[liveGroup.Count - 1].Count);
        }

        [Test]
        public void ChangingItem_LastItemInGroup_GroupRemoved()
        {
            _source = ClinqTestFactory.CreateTwoPersonSource();

            var group = from p in _source group p by p.Name;

            _source[0].Name = "Foo";

            Assert.AreEqual(2, group.Count);
        }

        [Test]
        public void GroupBy_MultipleGroups_UsesAnonymouseTypeForComparison()
        {
            _source = ClinqTestFactory.CreateTwoPersonSource();

            var group = from p in _source group p by new { p.Name, p.Age };

            _source.Add(new Person("Bob", 10));
            _source.Add(new Person("Jim", 20));

            Assert.AreEqual(2, group.Count);
            Assert.AreEqual(2, group[group.Count - 1].Count);
        }

        [Test]
        public void AddItemToSource_ItemIsADuplicate_AddedToCorrectGroup()
        {
            _source = ClinqTestFactory.CreateTwoPersonSource();

            var group = from p in _source group p 
                        by new { p.Name, p.Age };

            _source.Add(_source[0]);

            Assert.AreEqual(2, group.Count);
            Assert.AreEqual(2, group[0].Count);
        }

        [Test]
        public void RemoveItemFromSource_ItemIsADuplicate_AddedToCorrectGroup()
        {
            _source = ClinqTestFactory.CreateTwoPersonSource();
            _source.Add(_source[0]);

            var group = from p in _source 
                        group p by new { p.Name, p.Age };

            _source.Remove(_source[0]);

            Assert.AreEqual(2, group.Count);
            Assert.AreEqual(1, group[0].Count);
        }
    }
}
