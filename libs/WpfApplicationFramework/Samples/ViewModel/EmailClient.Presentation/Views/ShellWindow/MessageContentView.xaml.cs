using System.ComponentModel.Composition;
using System.Windows.Controls;
using EmailClient.Applications.Views;

namespace EmailClient.Presentation.Views
{
    [Export(typeof(IMessageContentView))]
    public partial class MessageContentView : UserControl, IMessageContentView
    {
        public MessageContentView()
        {
            InitializeComponent();
        }
    }
}
