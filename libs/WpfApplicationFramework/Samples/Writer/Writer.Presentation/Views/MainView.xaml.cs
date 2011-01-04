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
using Writer.Applications.ViewModels;
using System.ComponentModel.Composition;
using Writer.Applications.Views;

namespace Writer.Presentation.Views
{
    [Export(typeof(IMainView))]
    public partial class MainView : UserControl, IMainView
    {
        public MainView()
        {
            InitializeComponent();
            DataContextChanged += ThisDataContextChanged;
        }


        private MainViewModel ViewModel { get { return DataContext as MainViewModel; } }


        private void ThisDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            newKeyBinding.Command = ViewModel.NewCommand;
            openKeyBinding.Command = ViewModel.OpenCommand;
            closeKeyBinding.Command = ViewModel.CloseCommand;
            saveKeyBinding.Command = ViewModel.SaveCommand;
            printKeyBinding.Command = ViewModel.PrintCommand;
            aboutKeyBinding.Command = ViewModel.AboutCommand;
            nextDocumentKeyBinding.Command = ViewModel.NextDocumentCommand;
        }
    }
}
