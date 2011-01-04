using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Writer.Applications.Views;
using System.ComponentModel.Composition;

namespace Writer.Applications.Test.Views
{
    [Export(typeof(IRichTextView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class RichTextViewMock : ViewMock, IRichTextView
    {
    }
}
