using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookLibrary.Applications.Views;
using System.ComponentModel.Composition;

namespace BookLibrary.Applications.Test.Views
{
    [Export(typeof(IBookView)), Export]
    public class BookViewMock : ViewMock, IBookView
    {
        public bool FirstControlHasFocus { get; set; }


        public void FocusFirstControl()
        {
            FirstControlHasFocus = true;
        }
    }
}
