using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace NetworkLibrary
{
    [ServiceContract(CallbackContract=typeof(ITransactionService))]
    public interface ITransactionService
    {
        [OperationContract(IsOneWay = true)]
        void PublishTransaction(string wareHouseName,
            double amount,
            string employeeSource,
            int quantity,
            string SKU);
    }

    public interface ITransactionServiceChannel : ITransactionService, IClientChannel
    {
    }
}
