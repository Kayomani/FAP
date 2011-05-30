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
using FAP.Application.Views;

namespace Fap.Presentation.Panels
{
    /// <summary>
    /// Interaction logic for WebPanel.xaml
    /// </summary>
    public partial class WebPanel : UserControl, IWebPanel
    {
        public WebPanel()
        {
            InitializeComponent();
        }

        public string Location
        {
            get
            {
                return browser.Source.AbsolutePath;
            }
            set
            {
                browser.Source = new Uri(value);
            }
        }
    }
}
