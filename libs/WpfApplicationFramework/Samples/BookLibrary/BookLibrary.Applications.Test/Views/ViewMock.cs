using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications;

namespace BookLibrary.Applications.Test.Views
{
    public abstract class ViewMock : IView
    {
        public object DataContext { get; set; }
    }
}
