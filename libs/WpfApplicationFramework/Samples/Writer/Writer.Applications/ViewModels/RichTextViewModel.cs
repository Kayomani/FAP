using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications;
using Writer.Applications.Views;
using System.ComponentModel.Composition;
using Writer.Applications.Documents;

namespace Writer.Applications.ViewModels
{
    public class RichTextViewModel : ViewModel<IRichTextView>
    {
        private readonly RichTextDocument document;

        
        public RichTextViewModel(IRichTextView view, RichTextDocument document) : base(view)
        {
            this.document = document;
        }


        public RichTextDocument Document { get { return document; } }
    }
}
