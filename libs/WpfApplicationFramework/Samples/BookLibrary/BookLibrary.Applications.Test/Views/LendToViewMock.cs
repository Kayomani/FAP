using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookLibrary.Applications.Views;
using System.ComponentModel.Composition;

namespace BookLibrary.Applications.Test.Views
{
    [Export(typeof(ILendToView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class LendToViewMock : ViewMock, ILendToView
    {
        public static Action<LendToViewMock> ShowDialogAction { get; set; }
        public bool IsVisible { get; private set; }
        public object Owner { get; private set; }

        
        public void ShowDialog(object owner)
        {
            Owner = owner;
            IsVisible = true;
            if (ShowDialogAction != null) { ShowDialogAction(this); }
        }

        public void Close()
        {
            Owner = null;
            IsVisible = false;
        }
    }
}
