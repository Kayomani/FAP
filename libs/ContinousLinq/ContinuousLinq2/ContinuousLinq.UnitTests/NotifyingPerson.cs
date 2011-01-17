using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace ContinuousLinq.UnitTests
{
    [DebuggerDisplay("Name: {Name}, Age: {Age}")]
    public class NotifyingPerson : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private string _name;
        private int _age;
        private NotifyingPerson _brother;
        private ObservableCollection<NotifyingPerson> _parents;

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        public NotifyingPerson()
        {
        }

        public NotifyingPerson(string name, int age)
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

        public ObservableCollection<NotifyingPerson> Parents
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


        private void OnPropertyChanged(string property)
        {
            if (this.PropertyChanged == null)
                return;

            this.PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        private void OnPropertyChanging(string property)
        {
            if (this.PropertyChanging == null)
                return;

            this.PropertyChanging(this, new PropertyChangingEventArgs(property));
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

    }
}
