using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Writer.Applications.Views;
using System.ComponentModel.Composition;

namespace Writer.Applications.Test.Views
{
    [Export(typeof(IPrintPreviewView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class PrintPreviewViewMock : ViewMock, IPrintPreviewView
    {
        public bool PrintCalled { get; set; }


        public void Print()
        {
            PrintCalled = true;    
        }
    }
}
