using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Controls;
using BookLibrary.Applications.ViewModels;
using BookLibrary.Applications.Views;

namespace BookLibrary.Presentation.Views
{
    [Export(typeof(IPersonView))]
    public partial class PersonView : UserControl, IPersonView
    {
        private readonly HashSet<ValidationError> errors = new HashSet<ValidationError>();

        
        public PersonView()
        {
            InitializeComponent();
            Validation.AddErrorHandler(this, ErrorChangedHandler);
        }


        private PersonViewModel ViewModel { get { return DataContext as PersonViewModel; } }


        public void FocusFirstControl()
        {
            firstnameBox.Focus();
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
