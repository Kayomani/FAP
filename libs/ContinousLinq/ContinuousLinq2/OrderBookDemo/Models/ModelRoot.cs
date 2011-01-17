using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using ContinuousLinq;
using ContinuousLinq.Aggregates;
using System.Timers;
using System.ComponentModel;

namespace ContinuousLinq.OrderBookDemo.Models
{
    public class ModelRoot : INotifyPropertyChanged
    {
        private ObservableCollection<StockTransaction> _allTransactions;
        private ReadOnlyContinuousCollection<StockTransaction> _bids;
        private ReadOnlyContinuousCollection<StockTransaction> _asks;
        private ReadOnlyContinuousCollection<StockTransaction> _executions;
        private ReadOnlyContinuousCollection<StockTransaction> _graphExecutions;
        private ContinuousValue<double> _minPrice;
        private double _currentMin;
        private ContinuousValue<double> _maxPrice;
        private double _currentMax;
        private ContinuousValue<double> _vwap;
        private double _currentVwap;        

        private Timer _simulationTimer;

        public ModelRoot()
        {
            _allTransactions = new ObservableCollection<StockTransaction>();

            // Note that all of these queries are defined against an EMPTY
            // source collection...
            _bids = from tx in _allTransactions
                    where tx.TransactionType == TransactionType.Bid &&
                          tx.Symbol == "AAPL"
                    orderby tx.Price ascending
                    select tx;

            _asks = from tx in _allTransactions
                    where tx.TransactionType == TransactionType.Ask &&
                          tx.Symbol == "AAPL"
                    orderby tx.Price descending
                    select tx;

            _executions = from tx in _allTransactions
                          where tx.TransactionType == TransactionType.Execution &&
                                tx.Symbol == "AAPL"                            
                          orderby tx.Price descending
                          select tx;

            _graphExecutions = from tx in _executions
                               where tx.TimeStamp >= DateTime.Now.AddMinutes(-5)
                               select tx;

            _minPrice = _executions.ContinuousMin(tx => tx.Price,
                                                       newPrice => this.CurrentMin = newPrice);
            _maxPrice = _executions.ContinuousMax(tx => tx.Price,
                                                       newPrice => this.CurrentMax = newPrice);

            _vwap = _executions.ContinuousVwap(tx => tx.Price, tx => tx.Quantity,
                newVwap => this.CurrentVwap = newVwap);

            _simulationTimer = new Timer(1000);
            _simulationTimer.Elapsed += GenerateBogusData;
            _simulationTimer.Start();

        }

        private void GenerateBogusData(object sender, ElapsedEventArgs args)
        {
            double newPrice;
            double oldPrice;

            Random rnd = new Random();            

            if (_allTransactions.Count > 0)
            {
                oldPrice = _executions.Last().Price;

                if (rnd.NextDouble() >= 0.7)
                    newPrice = oldPrice - (rnd.NextDouble() * 0.5);
                else
                    newPrice = oldPrice + (rnd.NextDouble() * 1.4);
            }
            else
                newPrice = 100;

            // Fake Executions
            StockTransaction newTx = new StockTransaction()
            {
                Price = newPrice,
                Symbol = "AAPL",
                Quantity = rnd.Next(50) + 50,
                TimeStamp = DateTime.Now,
                TransactionType = TransactionType.Execution
            };
            _allTransactions.Add(newTx);
            StockTransaction newTx2 = new StockTransaction()
            {
                Price = newPrice,
                Symbol = "IBM",
                TimeStamp = DateTime.Now,
                Quantity = rnd.Next(50) + 50,
                TransactionType = TransactionType.Execution
            };
            _allTransactions.Add(newTx2);
            NotifyChanged("GraphExecutions"); // this is because the graph polyline is bound to a property, not knowing it's a collection


            StockTransaction newBid = new StockTransaction()
            {
                Price = newPrice - (newPrice * 0.05),
                TransactionType = TransactionType.Bid,
                Quantity = rnd.Next(50) + 50,
                TimeStamp = DateTime.Now,
                Symbol = "AAPL"
            };
            _allTransactions.Add(newBid);

            StockTransaction newAsk = new StockTransaction()
            {
                Price = newPrice + (newPrice * 0.05),
                TransactionType = TransactionType.Ask,
                TimeStamp = DateTime.Now,
                Quantity = rnd.Next(50) + 50,
                Symbol = "AAPL"
            };

            _allTransactions.Add(newAsk);
        }

        public ReadOnlyContinuousCollection<StockTransaction> Bids
        {
            get
            {
                return _bids;
            }
        }

        public ReadOnlyContinuousCollection<StockTransaction> Asks
        {
            get
            {
                return _asks;
            }
        }

        public ReadOnlyContinuousCollection<StockTransaction> Executions
        {
            get
            {
                return _executions;
            }
        }

        public ReadOnlyContinuousCollection<StockTransaction> GraphExecutions
        {
            get
            {
                return _graphExecutions;
            }
        }

        public double CurrentMin
        {
            get
            {
                return _currentMin;
            }
            set {
                _currentMin = value;
                NotifyChanged("CurrentMin");
            }
        }

        public double CurrentVwap
        {
            get
            {
                return _currentVwap;
            }
            set
            {
                _currentVwap = value;
                NotifyChanged("CurrentVwap");
            }
        }

        public double CurrentMax
        {
            get {
                return _currentMax;
            }
            set {
                _currentMax = value;
                NotifyChanged("CurrentMax");
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
