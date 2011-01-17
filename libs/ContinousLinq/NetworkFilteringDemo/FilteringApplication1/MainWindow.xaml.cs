using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ContinuousLinq;
using NetworkLibrary.Implementations;
using NetworkLibrary;
using System.Configuration;
using System.Diagnostics;

namespace FilteringApplication1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BinSplit _binSplit;
        private GenericTransactionService _txService;
       

        public MainWindow()
        {
            InitializeComponent();

            // NOTE: At this point, _allTransactions is EMPTY...yet we're defining
            // a query on it anyway, and CLINQ makes it work...

            leftLabel.Text = ConfigurationManager.AppSettings["leftBin"];
            rightLabel.Text = ConfigurationManager.AppSettings["rightBin"];

            _binSplit = new BinSplit();
            _binSplit.LeftBin = from tx in ModelRoot.Current.AllTransactions
                       where tx.WarehouseName == ConfigurationManager.AppSettings["leftBin"]
                       select tx;
            _binSplit.RightBin = from tx in ModelRoot.Current.AllTransactions
                        where tx.WarehouseName == ConfigurationManager.AppSettings["rightBin"]
                        select tx;

            this.DataContext = _binSplit;

            _txService = new GenericTransactionService();
            _txService.OnTransactionArrived += new TransactionArrivedDelegate(_txService_OnTransactionArrived);
            CommunicationCenter.OpenPeerChannel<ITransactionService, ITransactionServiceChannel>(
                _txService, "net.p2p://clinq/netfilterdemo",
                Int32.Parse(ConfigurationManager.AppSettings["peerPort"]));
            Trace.WriteLine("Filtering App / Data Receiver is Ready to go...");
        }

        void _txService_OnTransactionArrived(Transaction t)
        {
            ModelRoot.Current.AllTransactions.Add(t);
        }
    }
}
