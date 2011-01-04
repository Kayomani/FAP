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
using Writer.Applications.Views;
using System.ComponentModel.Composition;
using Writer.Applications.ViewModels;

namespace Writer.Presentation.Views
{
    [Export(typeof(IRichTextView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class RichTextView : UserControl, IRichTextView
    {
        private bool suppressTextChanged;
        

        public RichTextView()
        {
            InitializeComponent();
            DataContextChanged += ThisDataContextChanged;
            Loaded += ThisLoaded;
        }

        
        private RichTextViewModel ViewModel { get { return DataContext as RichTextViewModel; } }


        private void ThisDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            suppressTextChanged = true;
            richTextBox.Document = ViewModel.Document.Content;
            suppressTextChanged = false;
        }

        private void ThisLoaded(object sender, RoutedEventArgs e)
        {
            richTextBox.Focus();
        }

        private void RichTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!suppressTextChanged) { ViewModel.Document.Modified = true; }
        }
    }
}
