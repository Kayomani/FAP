using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookLibrary.Applications.Views;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace BookLibrary.Applications.Test.Views
{
    [Export(typeof(IShellView)), Export]
    public class ShellViewMock : ViewMock, IShellView
    {
        public bool IsVisible { get; private set; }


        public event CancelEventHandler Closing;


        public void Show()
        {
            IsVisible = true;
        }

        public void Close()
        {
            CancelEventArgs e = new CancelEventArgs();
            OnClosing(e);
            if (!e.Cancel)
            {
                IsVisible = false;
            }
        }

        protected virtual void OnClosing(CancelEventArgs e)
        {
            if (Closing != null) { Closing(this, e); }
        }
    }
}
