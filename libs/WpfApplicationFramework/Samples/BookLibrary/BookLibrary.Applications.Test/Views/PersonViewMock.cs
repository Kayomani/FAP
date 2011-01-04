using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookLibrary.Applications.Views;
using System.ComponentModel.Composition;

namespace BookLibrary.Applications.Test.Views
{
    [Export(typeof(IPersonView)), Export]
    public class PersonViewMock : ViewMock, IPersonView
    {
        public bool FirstControlHasFocus { get; set; }
        

        public void FocusFirstControl()
        {
            FirstControlHasFocus = true;
        }
    }
}
