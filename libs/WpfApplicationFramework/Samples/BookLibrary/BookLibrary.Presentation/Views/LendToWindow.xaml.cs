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
using System.ComponentModel.Composition;
using BookLibrary.Applications.Views;
using BookLibrary.Applications.ViewModels;

namespace BookLibrary.Presentation.Views
{
    [Export(typeof(ILendToView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class LendToWindow : Window, ILendToView
    {
        public LendToWindow()
        {
            InitializeComponent();
        }


        private LendToViewModel ViewModel { get { return DataContext as LendToViewModel; } }


        public void ShowDialog(object owner)
        {
            Owner = owner as Window;
            ShowDialog();
        }

        private void PersonsListMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewModel.OkCommand.Execute(null);
        }
    }
}
