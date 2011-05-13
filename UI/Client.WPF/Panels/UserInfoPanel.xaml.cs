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
    /// Interaction logic for UserInfoPanel.xaml
    /// </summary>
    public partial class UserInfoPanel : UserControl, IUserInfo
    {
        public UserInfoPanel()
        {
            InitializeComponent();
        }
    }
}
