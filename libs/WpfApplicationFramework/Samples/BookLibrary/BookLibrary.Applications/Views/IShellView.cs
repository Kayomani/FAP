using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications;
using System.ComponentModel;

namespace BookLibrary.Applications.Views
{
    public interface IShellView : IView
    {
        event CancelEventHandler Closing;
        

        void Show();

        void Close();
    }
}
