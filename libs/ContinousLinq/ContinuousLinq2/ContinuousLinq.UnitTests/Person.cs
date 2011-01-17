//#define USE_NOTIFYING_VERSION


using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using ContinuousLinq;


namespace ContinuousLinq.UnitTests
{
    [DebuggerDisplay("Name: {Name}, Age: {Age}")]
    public class Person :
        INotifyPropertyChanged
#if USE_NOTIFYING_VERSION
        ,INotifyPropertyChanging
#endif
    {
        #region Fields

        private string _name;
        private int _age;
        private Person _brother;
        private ObservableCollection<Person> _parents;

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

#if USE_NOTIFYING_VERSION
        public event PropertyChangingEventHandler PropertyChanging;
#endif
        #endregion

        #region Constructors

        public Person()
        {
        }

        public Person(string name, int age)
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
#if USE_NOTIFYING_VERSION
                OnPropertyChanging("Name");
#endif

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

#if USE_NOTIFYING_VERSION
                OnPropertyChanging("Age");
#endif

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
#if USE_NOTIFYING_VERSION
                OnPropertyChanging("Brother");
#endif

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
#if USE_NOTIFYING_VERSION
                OnPropertyChanging("Parents");
#endif

                _parents = value;
                OnPropertyChanged("Parents");
            }
        }

        ReadOnlyContinuousCollection<Person> _associatedPeople;
        public ReadOnlyContinuousCollection<Person> AssociatedPeople
        {
            get { return _associatedPeople; }
            set
            {
                if (value == _associatedPeople)
                    return;
#if USE_NOTIFYING_VERSION
                OnPropertyChanging("AssociatedPeople");
#endif
                _associatedPeople = value;
                OnPropertyChanged("AssociatedPeople");
            }
        }

        #endregion

        #region Methods

        private void OnPropertyChanged(string property)
        {
            if (this.PropertyChanged == null)
                return;

            this.PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

#if USE_NOTIFYING_VERSION

        private void OnPropertyChanging(string property)
        {
            if (this.PropertyChanging == null)
                return;

            this.PropertyChanging(this, new PropertyChangingEventArgs(property));
        }
#endif

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
