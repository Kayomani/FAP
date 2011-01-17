using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockMonitorDemo.Models
{    
    public class StockSaleTick : ModelObject
    {
        private string _symbol;
        private double _price;        
        private DateTime _timeStamp;
        private int _quantity;

        public StockSaleTick() : base()
        {
            _symbol = string.Empty;
            _price = 0;
            _timeStamp = DateTime.Now;
            _quantity = 0;
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
                NotifyPropertyChanged("Quantity");
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
                NotifyPropertyChanged("Symbol");
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
                NotifyPropertyChanged("Price");                
            }
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
                NotifyPropertyChanged("TimeStamp");
            }
        }
    }
}
