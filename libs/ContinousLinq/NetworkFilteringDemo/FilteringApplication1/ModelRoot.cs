using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContinuousLinq;
using NetworkLibrary.Implementations;

namespace FilteringApplication1
{
    public class ModelRoot
    {
        private static ModelRoot _instance;

        private ContinuousCollection<Transaction> _allTransactions;

        public ModelRoot()
        {
            _allTransactions = new ContinuousCollection<Transaction>();
        }

        public static ModelRoot Current
        {
            get
            {
                if (_instance == null)
                    _instance = new ModelRoot();
                return _instance;
            }
        }

        public ContinuousCollection<Transaction> AllTransactions
        {
            get
            {
                return _allTransactions;
            }
            set
            {
                _allTransactions = value;
            }
        }
    }
}
