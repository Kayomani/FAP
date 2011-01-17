using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkLibrary;

namespace TransactionSender
{
    public class LocalTransactionService : ITransactionService
    {
        #region ITransactionService Members

        public void PublishTransaction(string wareHouseName, double amount, string employeeSource, int quantity, string SKU)
        {
            // Do nothing locally when transactions are published.... we don't care about them :)
            //Console.WriteLine("Published Transaction.");
        }

        #endregion
    }
}
