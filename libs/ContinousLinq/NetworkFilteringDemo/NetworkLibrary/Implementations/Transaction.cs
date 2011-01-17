using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace NetworkLibrary.Implementations
{
    public class Transaction : INotifyPropertyChanged
    {
        private string _employeeSource;
        private int _quantity;
        private double _amount;
        private string _warehouseName;
        private string _sku;

        public string EmployeeSource
        {
            get
            {
                return _employeeSource;
            }
            set
            {
                _employeeSource = value;
                NotifyChanged("EmployeeSource");
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

        public double Amount
        {
            get
            {
                return _amount;
            }
            set
            {
                _amount = value;
                NotifyChanged("Amount");
            }
        }

        public string WarehouseName
        {
            get
            {
                return _warehouseName;
            }
            set
            {
                _warehouseName = value;
                NotifyChanged("WarehouseName");
            }
        }

        public string SKU
        {
            get
            {
                return _sku;
            }
            set
            {
                _sku = value;
                NotifyChanged("SKU");
            }
        }
       
        private void NotifyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
