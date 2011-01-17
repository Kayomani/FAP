using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using ContinuousLinq.Aggregates;
using StockMonitorDemo.Extensions;
using System.Timers;
using ContinuousLinq;
using System.ComponentModel;

namespace StockMonitorDemo.Models
{
    public class MonitorWindowModel : ModelObject
    {
        private ContinuousCollection<StockSaleTick> _tickerData;
        private ContinuousCollection<StockSaleTick> _allTicks;
        private ContinuousCollection<StockQuoteTick> _allQuotes;
        private ContinuousCollection<StockSaleTick> _graphWindowTicks;
        private ContinuousCollection<StockSaleTick> _vwapTicks;
        private ContinuousCollection<StockQuoteTick> _bidTicks;
        private ContinuousCollection<StockQuoteTick> _askTicks;
        private ContinuousValue<double> _liveVwap;
        private ContinuousValue<double> _liveHigh;
        private ContinuousValue<double> _liveMin;
        private Timer _simulationTimer;
        private string _monitoredSymbol;

        public MonitorWindowModel() : base()
        {
            _tickerData = new ContinuousCollection<StockSaleTick>();
            _vwapTicks = new ContinuousCollection<StockSaleTick>();
            _allQuotes = new ContinuousCollection<StockQuoteTick>();

            _bidTicks = from quote in _allQuotes
                        where quote.Side == QuoteSide.Bid
                        select quote;

            _askTicks = from quote in _allQuotes
                        where quote.Side == QuoteSide.Ask
                        select quote;
            
            _allTicks =
                from tick in _tickerData
                where tick.Symbol == "AAPL"
                select tick;

            _graphWindowTicks =
                from tick in _tickerData
                where tick.TimeStamp >= DateTime.Now.AddMinutes(-5) &&
                      tick.Symbol == "AAPL"
                select tick; // return last 5 minutes worth of ticks for AAPL */

            _liveVwap = _allTicks.ContinuousVwap<StockSaleTick>(
                tick => tick.Price,
                tick => tick.Quantity);             

            _liveHigh = _allTicks.ContinuousMax<StockSaleTick>(
                tick => tick.Price);

            _liveMin = _allTicks.ContinuousMin<StockSaleTick>(
                tick => tick.Price);             

            _liveMin.PropertyChanged += 
                new PropertyChangedEventHandler(_liveMin_PropertyChanged);
            _liveHigh.PropertyChanged += 
                new PropertyChangedEventHandler(_liveHigh_PropertyChanged);
            _liveVwap.PropertyChanged += 
                new PropertyChangedEventHandler(_liveVwap_PropertyChanged);

            _simulationTimer = new Timer();
            _simulationTimer.Elapsed += new ElapsedEventHandler(_simulationTimer_Elapsed);
            _simulationTimer.Interval = 1000;
            _simulationTimer.Start(); // create a new tick every second
        }

        void _liveVwap_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Using Stocktick instead of some custom value just to make it easy
            // to re-use the point collection converter
            _vwapTicks.Add(new StockSaleTick()
            {
                Price = _liveVwap.CurrentValue,
                Symbol = "VWAP",
                Quantity = 0,
                TimeStamp = DateTime.Now
            });
            NotifyPropertyChanged("VwapTicks");
        }

        void _liveHigh_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
           
        }

        void _liveMin_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            
        }

        void _simulationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {            
            double newPrice;
            double oldPrice;            

            Random rnd = new Random();

            double variance = rnd.NextDouble() * 1.50; // stock can change +- 1.50

            if (_tickerData.Count > 0)
            {
                oldPrice = _tickerData.Last().Price;

                if (rnd.Next(0, 4) > 1)
                    newPrice = oldPrice - variance;
                else
                    newPrice = oldPrice + variance;
            }
            else
                newPrice = 100;

           
            // Simulation values.                
            StockSaleTick newTick = new StockSaleTick()
            {
                Price = newPrice,                    
                TimeStamp = DateTime.Now,
                Symbol = "AAPL",
                Quantity = rnd.Next(50) + 50 // QTY range: 50-100
            };
            _tickerData.Add(newTick);
            newTick = new StockSaleTick()
            {
                Price = newPrice,                    
                TimeStamp = DateTime.Now,
                Symbol = "IBM",
                Quantity = rnd.Next(50) + 50 // QTY range: 50-100
            };
            _tickerData.Add(newTick); 
            NotifyPropertyChanged("GraphWindowTicks");

            StockQuoteTick newQuote = new StockQuoteTick()
            {
                Price = newPrice - (newPrice * 0.05),
                Side = QuoteSide.Bid,
                Quantity = rnd.Next(50) + 50,
                Symbol = "AAPL",
                TimeStamp = DateTime.Now
            };
            _allQuotes.Add(newQuote);
            _allQuotes.Add(newQuote);
            newQuote = new StockQuoteTick()
            {
                Price = newPrice + (newPrice * 0.05),
                Side = QuoteSide.Ask,
                Quantity = rnd.Next(50) + 50,
                Symbol = "AAPL",
                TimeStamp = DateTime.Now
            };
            _allQuotes.Add(newQuote);
        }

        public string MonitoredSymbol
        {
            get
            {
                return _monitoredSymbol;
            }
            set
            {
                _monitoredSymbol = value;
                NotifyPropertyChanged("MonitoredSymbol");
            }
        }

        public ContinuousValue<double> LiveMin
        {
            get
            {
                return _liveMin;
            }
        }

        public ContinuousValue<double> LiveHigh
        {
            get
            {
                return _liveHigh;
            }
        }

        public ContinuousValue<double> LiveVwap
        {
            get
            {
                return _liveVwap;
            }
        }

        public ContinuousCollection<StockSaleTick> AllTicks
        {
            get
            {
                return _allTicks;
            }
            set
            {
                _allTicks = value;
                NotifyPropertyChanged("AllTicks");
            }
        }

        public ContinuousCollection<StockQuoteTick> BidTicks
        {
            get
            {
                return _bidTicks;
            }
        }

        public ContinuousCollection<StockQuoteTick> AskTicks
        {
            get
            {
                return _askTicks;
            }
        }
              

        public ContinuousCollection<StockSaleTick> GraphWindowTicks
        {
            get
            {
                return _graphWindowTicks;
            }
            set
            {
                _graphWindowTicks = value;
                NotifyPropertyChanged("GraphWindowTicks");
            }
        }

        public ContinuousCollection<StockSaleTick> VwapTicks
        {
            get
            {
                return _vwapTicks;
            }
            set
            {
                _vwapTicks = value;
                NotifyPropertyChanged("VwapTicks");
            }
        }
    }
}
