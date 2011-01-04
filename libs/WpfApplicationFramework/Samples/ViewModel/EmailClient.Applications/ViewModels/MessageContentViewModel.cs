using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmailClient.Applications.Views;
using System.ComponentModel.Composition;
using System.Waf.Applications;

namespace EmailClient.Applications.ViewModels
{
    [Export]
    public class MessageContentViewModel : ViewModel<IMessageContentView>
    {
        [ImportingConstructor]
        public MessageContentViewModel(IMessageContentView view) : base(view)
        {
        }
    }
}
