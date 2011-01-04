using System.ComponentModel.Composition;
using System.Windows.Controls;
using EmailClient.Applications.Views;

namespace EmailClient.Presentation.Views
{
    [Export(typeof(IMessageListView))]
    public partial class MessageListView : UserControl, IMessageListView
    {
        public MessageListView()
        {
            InitializeComponent();
        }
    }
}
