using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockMonitorDemo.Models
{
    public class StockQuoteTick : ModelObject
    {
        private string _symbol;
        private DateTime _timeStamp;
        private int _quantity;
        private double _price;
        private QuoteSide _side;

        public StockQuoteTick()
            : base()
        {
        }

        public string Symbol
        {
            get { return _symbol; }
            set
            {
                _symbol = value; NotifyPropertyChanged("Symbol");
            }
        }

        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; NotifyPropertyChanged("TimeStamp"); }
        }

        public int Quantity
        {
            get { return _quantity; }
            set { _quantity = value; NotifyPropertyChanged("Quantity"); }
        }

        public double Price
        {
            get { return _price; }
            set { _price = value; NotifyPropertyChanged("Price"); }
        }

        public QuoteSide Side
        {
            get { return _side; }
            set { _side = value; NotifyPropertyChanged("Side"); }
        }

    }

    public enum QuoteSide
    {
        Bid,
        Ask
    }
}
