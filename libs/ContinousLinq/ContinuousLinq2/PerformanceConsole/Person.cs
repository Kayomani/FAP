using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ContinuousLinq;

namespace PerformanceConsole
{
    public class Person : INotifyPropertyChanged
    {
        #region Fields

        private string _name;
        private int _age;
        private Person _brother;

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public Person()
        {
        }

        public Person(string name, int age)
        {
            _name = name;
            _age = age;
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {

                if (_name == value)
                    return;
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public int Age
        {
            get
            {
                return _age;
            }
            set
            {
                if (_age == value)
                    return;
                _age = value;
                OnPropertyChanged("Age");
            }
        }

        public Person Brother
        {
            get { return _brother; }
            set
            {
                if (value == _brother)
                    return;

                _brother = value;
                OnPropertyChanged("Brother");
            }
        }

        private ObservableCollection<Person> _parents;
        public ObservableCollection<Person> Parents
        {
            get { return _parents; }
            set
            {
                if (value == _parents)
                    return;

                _parents = value;
                OnPropertyChanged("Parents");
            }
        }
        #region Members

        private void OnPropertyChanged(string property)
        {
            if (this.PropertyChanged == null)
                return;

            this.PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public ReadOnlyContinuousCollection<Person> GetPeopleWithSameAge(ObservableCollection<Person> people)
        {
            return from person in people
                   where person.Age == this.Age
                   select person;
        }

        public ReadOnlyContinuousCollection<Person> GetPeopleWithSameAgeAsBrother(ObservableCollection<Person> people)
        {
            return from person in people
                   where this.Brother != null && person.Age == this.Brother.Age
                   select person;
        }

        #endregion
    }

    
    public class NotifyingPerson : INotifyPropertyChanged, INotifyPropertyChanging
    {
        #region Fields

        private string _name;
        private int _age;
        private NotifyingPerson _brother;
        private ObservableCollection<Person> _parents;

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        #endregion

        #region Constructors

        public NotifyingPerson()
        {
        }

        public NotifyingPerson(string name, int age)
        {
            _name = name;
            _age = age;
        }

        #endregion

        #region Properties

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {

                if (_name == value)
                    return;

                OnPropertyChanging("Name");
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public int Age
        {
            get
            {
                return _age;
            }
            set
            {
                if (_age == value)
                    return;
                OnPropertyChanging("Age");
                _age = value;
                OnPropertyChanged("Age");
            }
        }

        public NotifyingPerson Brother
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

        public ObservableCollection<Person> Parents
        {
            get { return _parents; }
            set
            {
                if (value == _parents)
                    return;

                OnPropertyChanging("Parents");
                _parents = value;
                OnPropertyChanged("Parents");
            }
        }

        ReadOnlyContinuousCollection<NotifyingPerson> _associatedPeople;
        public ReadOnlyContinuousCollection<NotifyingPerson> AssociatedPeople
        {
            get { return _associatedPeople; }
            set
            {
                if (value == _associatedPeople)
                    return;

                OnPropertyChanging("AssociatedPeople");
                _associatedPeople = value;
                OnPropertyChanged("AssociatedPeople");
            }
        }

        #endregion

        #region Methods

        private void OnPropertyChanging(string property)
        {
            if (this.PropertyChanging == null)
                return;

            this.PropertyChanging(this, new PropertyChangingEventArgs(property));
        }

        private void OnPropertyChanged(string property)
        {
            if (this.PropertyChanged == null)
                return;

            this.PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public ReadOnlyContinuousCollection<Person> GetPeopleWithSameAge(ObservableCollection<Person> people)
        {
            return from person in people
                   where person.Age == this.Age
                   select person;
        }

        public ReadOnlyContinuousCollection<Person> GetPeopleWithSameAgeAsBrother(ObservableCollection<Person> people)
        {
            return from person in people
                   where this.Brother != null && person.Age == this.Brother.Age
                   select person;
        }

        public int AddYearsToAge(int amount)
        {
            this.Age += amount;
            return this.Age; ;
        }

        public int SubtractYearsFromAge(int amount)
        {
            this.Age -= amount;
            return this.Age; ;
        }

        #endregion
    }
}
