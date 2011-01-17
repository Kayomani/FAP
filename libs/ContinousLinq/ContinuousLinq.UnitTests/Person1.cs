using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using ContinuousLinq;
using ContinuousLinq.Aggregates;
using NUnit.Framework;

namespace ContinuousLinq.UnitTests
{
    public class Person : INotifyPropertyChanged
    {
        private string _name;
        private int _age;

        public event PropertyChangedEventHandler PropertyChanged;

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

        #region Members

        public override int GetHashCode()
        {
            return (this.Name.GetHashCode() ^ this.Age.GetHashCode());
        }

        private void OnPropertyChanged(string property)
        {
            if (this.PropertyChanged == null)
                return;

            this.PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion
    }
}
