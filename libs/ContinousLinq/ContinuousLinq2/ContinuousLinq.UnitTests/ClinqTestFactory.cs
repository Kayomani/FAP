using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ContinuousLinq.UnitTests
{
    public static class ClinqTestFactory
    {
        public static ObservableCollection<Person> CreateTwoPersonSource()
        {
            return new ObservableCollection<Person>() 
            {
                new Person("Bob", 10), 
                new Person("Jim", 20),
            };
        }

        public static ObservableCollection<Person> CreateTwoPersonSourceWithParents()
        {
            var source = CreateTwoPersonSource();
            InitializeParents(source[0]);
            InitializeParents(source[1]);

            return source;
        }

        public static void InitializeParents(Person person)
        {
            person.Parents = CreateParents(person);
        }

        public static ObservableCollection<Person> CreateParents(Person person)
        {
            return new ObservableCollection<Person>()
            {
                new Person(person.Name + "Parent0", 40),
                new Person(person.Name + "Parent1", 41),
            };
        }
        public static ObservableCollection<Person> CreateSixPersonSource()
        {
            return CreateAnyPersonSource(6);
        }
        public static ObservableCollection<Person>CreateAnyPersonSource(int persons)
        {
            ObservableCollection<Person> source = new ObservableCollection<Person>();
            for (int i = 0; i < persons; i++)
            {
                source.Add(new Person(i.ToString(), i * 10));
            }
            return source;
        }
        public static ObservableCollection<Person> CreateSixPersonSourceWithDuplicates()
        {
            Person bob = new Person("Bob", 10);
            Person jim = new Person("Jim", 20);

            ObservableCollection<Person> source = CreateSixPersonSource();
            source[0] = bob;
            source[1] = bob;
            source[2] = bob;

            source[4] = jim;
            source[5] = jim;

            return source;
        }

        internal static ObservableCollection<Person> CreateGroupablePersonSource()
        {
            ObservableCollection<Person> col = new ObservableCollection<Person>();
            // add person objects to collection sufficient to create 10 groups of 6
            // based on their age.
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    Person p = new Person()
                    {
                        Age = x * 20 + 5,
                        Name = "Person " + x.ToString() + " " + y.ToString()
                    };
                    col.Add(p);
                }
            }

            return col;
        }
    }
}
