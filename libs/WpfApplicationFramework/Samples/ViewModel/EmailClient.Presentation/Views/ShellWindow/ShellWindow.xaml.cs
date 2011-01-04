using System.ComponentModel.Composition;
using System.Windows;
using EmailClient.Applications.Views;

namespace EmailClient.Presentation.Views
{
    [Export(typeof(IShellView))]
    public partial class ShellWindow : Window, IShellView
    {
        public ShellWindow()
        {
            InitializeComponent();
        }
    }
}
