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
    public class MessageListViewModel : ViewModel<IMessageListView>
    {
        [ImportingConstructor]
        public MessageListViewModel(IMessageListView view) : base(view)
        {
        }
    }
}
