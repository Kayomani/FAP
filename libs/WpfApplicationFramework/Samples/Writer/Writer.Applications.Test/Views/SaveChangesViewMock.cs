using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications;
using Writer.Applications.Views;
using System.ComponentModel.Composition;
using Writer.Applications.ViewModels;

namespace Writer.Applications.Test.Views
{
    [Export(typeof(ISaveChangesView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class SaveChangesViewMock : ViewMock, ISaveChangesView
    {
        public static Action<SaveChangesViewMock> ShowDialogAction { get; set; }
        public bool IsVisible { get; private set; }
        public object Owner { get; private set; }
        public SaveChangesViewModel ViewModel { get { return this.GetViewModel<SaveChangesViewModel>(); } }


        public void ShowDialog(object owner)
        {
            Owner = owner;
            IsVisible = true;
            if (ShowDialogAction != null) { ShowDialogAction(this); }
        }

        public void Close()
        {
            IsVisible = false;
        }
    }
}
