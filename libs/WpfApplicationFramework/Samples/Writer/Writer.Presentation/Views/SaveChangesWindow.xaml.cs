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
using Writer.Applications.Views;
using System.ComponentModel.Composition;

namespace Writer.Presentation.Views
{
    [Export(typeof(ISaveChangesView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class SaveChangesWindow : Window, ISaveChangesView
    {
        public SaveChangesWindow()
        {
            InitializeComponent();
        }


        public void ShowDialog(object owner)
        {
            Owner = owner as Window;
            ShowDialog();
        }
    }
}
