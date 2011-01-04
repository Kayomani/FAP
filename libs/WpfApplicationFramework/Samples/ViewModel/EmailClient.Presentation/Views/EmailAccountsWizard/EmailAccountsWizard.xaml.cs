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
using System.Windows.Shapes;
using EmailClient.Applications.Views;
using System.ComponentModel.Composition;

namespace EmailClient.Presentation.Views
{
    [Export(typeof(IEmailAccountsView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class EmailAccountsWizard : Window, IEmailAccountsView
    {
        public EmailAccountsWizard()
        {
            InitializeComponent();
        }


        public bool? ShowDialog(object owner)
        {
            Owner = owner as Window;
            return ShowDialog();
        }
    }
}
