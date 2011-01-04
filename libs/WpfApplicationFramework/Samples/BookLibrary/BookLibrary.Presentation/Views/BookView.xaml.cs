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
using System.ComponentModel.Composition;
using BookLibrary.Applications.Views;
using BookLibrary.Applications.ViewModels;

namespace BookLibrary.Presentation.Views
{
    [Export(typeof(IBookView))]
    public partial class BookView : UserControl, IBookView
    {
        private readonly HashSet<ValidationError> errors = new HashSet<ValidationError>();

        
        public BookView()
        {
            InitializeComponent();
            Validation.AddErrorHandler(this, ErrorChangedHandler);
        }


        private BookViewModel ViewModel { get { return DataContext as BookViewModel; } }


        public void FocusFirstControl()
        {
            titleBox.Focus();
        }
        
        private void ErrorChangedHandler(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
            {
                errors.Add(e.Error);
            }
            else
            {
                errors.Remove(e.Error);
            }

            ViewModel.IsValid = !errors.Any();
        }
    }
}
