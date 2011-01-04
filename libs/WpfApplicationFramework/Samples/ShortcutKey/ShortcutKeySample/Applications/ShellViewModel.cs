using System.Diagnostics;
using System.Waf.Applications;
using System.Windows.Input;

namespace ShortcutKeySample.Applications
{
    internal class ShellViewModel : ViewModel<IShellView>
    {
        private readonly DelegateCommand openCommand;


        public ShellViewModel(IShellView view) : base(view)
        {
            openCommand = new DelegateCommand(Open);
        }


        public ICommand OpenCommand { get { return openCommand; } }


        public void Show()
        {
            ViewCore.Show();
        }

        private void Open()
        {
            Trace.WriteLine("OpenCommand executed.");
        }
    }
}
