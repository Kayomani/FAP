using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookLibrary.Applications.Views;
using System.ComponentModel.Composition;

namespace BookLibrary.Applications.Test.Views
{
    [Export(typeof(IBookListView)), Export]
    public class BookListViewMock : ViewMock, IBookListView
    {
    }
}
