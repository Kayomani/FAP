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
using EmailClient.Applications.Views;
using System.ComponentModel.Composition;
using EmailClient.Applications.ViewModels;

namespace EmailClient.Presentation.Views
{
    [Export(typeof(IPop3SettingsView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class Pop3SettingsView : UserControl, IPop3SettingsView
    {
        public Pop3SettingsView()
        {
            InitializeComponent();
        }


        private Pop3SettingsViewModel ViewModel { get { return DataContext as Pop3SettingsViewModel; } }


        private void Pop3PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender;
            if (ViewModel != null)
            {
                ViewModel.Pop3Password = passwordBox.Password;
            }
        }

        private void SmtpPasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender;
            if (ViewModel != null)
            {
                ViewModel.SmtpPassword = passwordBox.Password;
            }
        }
    }
}
