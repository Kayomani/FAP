using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace NetworkLibrary.Implementations
{
    public delegate void TransactionArrivedDelegate(Transaction t);

    public class GenericTransactionService : ITransactionService
    {
        public event TransactionArrivedDelegate OnTransactionArrived;

        #region ITransactionService Members

        public void PublishTransaction(string wareHouseName, double amount, string employeeSource, int quantity, string sku)
        {
            Trace.WriteLine("Received TX for " + wareHouseName);
            Transaction t = new Transaction()
            {
                WarehouseName = wareHouseName,
                Amount = amount,
                EmployeeSource = employeeSource,
                Quantity = quantity,
                SKU = sku
            };
            if (OnTransactionArrived != null)
                OnTransactionArrived(t);
        }

        #endregion
    }
}
