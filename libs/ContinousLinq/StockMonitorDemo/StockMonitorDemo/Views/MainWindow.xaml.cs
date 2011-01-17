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
using StockMonitorDemo.Models;

namespace StockMonitorDemo.Views
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MonitorWindowModel _model;

        public MainWindow()
        {
            InitializeComponent();

            _model = new MonitorWindowModel();
            _model.MonitoredSymbol = "AAPL";
            this.DataContext = _model;
            
        }

        private void toggleHighAgg_Click(object sender, RoutedEventArgs e)
        {
            if (_model.LiveHigh.IsPaused)
            {
                _model.LiveHigh.Resume();
                toggleHighAgg.Content = "Pause";
            }
            else
            {
                _model.LiveHigh.Pause();
                toggleHighAgg.Content = "Resume";
            }
        }

        private void toggleVwapAgg_Click(object sender, RoutedEventArgs e)
        {
            if (_model.LiveVwap.IsPaused)
            {
                _model.LiveVwap.Resume();
                toggleVwapAgg.Content = "Pause";
            }
            else
            {
                _model.LiveVwap.Pause();
                toggleVwapAgg.Content = "Resume";
            }
        }
    }
}
