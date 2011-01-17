using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ContinuousLinq.OrderBookDemo.Models
{
    public enum TransactionType
    {
        Bid,
        Ask,
        Execution
    }

    public class StockTransaction : INotifyPropertyChanged
    {
        private string _symbol;
        private int _quantity;
        private double _price;
        private TransactionType _txType;
        private DateTime _timeStamp;

        private void NotifyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public DateTime TimeStamp
        {
            get
            {
                return _timeStamp;
            }
            set
            {
                _timeStamp = value;
                NotifyChanged("TimeStamp");
            }
        }

        public string Symbol
        {
            get
            {
                return _symbol;
            }
            set
            {
                _symbol = value;
                NotifyChanged("Symbol");
            }
        }

        public int Quantity
        {
            get
            {
                return _quantity;
            }
            set
            {
                _quantity = value;
                NotifyChanged("Quantity");
            }
        }

        public double Price
        {
            get
            {
                return _price;
            }
            set
            {
                _price = value;
                NotifyChanged("Price");
            }
        }

        public TransactionType TransactionType
        {
            get
            {
                return _txType;
            }
            set
            {
                _txType = value;
                NotifyChanged("TransactionType");
            }
        }

    
#region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler  PropertyChanged;

#endregion
}
}
