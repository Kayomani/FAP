using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookLibrary.Applications.Views;
using System.ComponentModel.Composition;

namespace BookLibrary.Applications.Test.Views
{
    [Export(typeof(IPersonListView)), Export]
    public class PersonListViewMock : ViewMock, IPersonListView
    {
    }
}
