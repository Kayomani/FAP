using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmailClient.Applications.Views;
using System.ComponentModel.Composition;
using System.Waf.Applications;
using System.Windows.Input;

namespace EmailClient.Applications.ViewModels
{
    [Export]
    public class ShellViewModel : ViewModel<IShellView>
    {
        private ICommand exitCommand;
        private ICommand emailAccountsCommand;
        private object messageListView;
        private object messageContentView;


        [ImportingConstructor]
        public ShellViewModel(IShellView view) : base(view)
        {
        }


        public ICommand ExitCommand 
        { 
            get { return exitCommand; }
            set 
            {
                if (exitCommand != value)
                {
                    exitCommand = value;
                    RaisePropertyChanged("ExitCommand");
                }
            }
        }

        public ICommand EmailAccountsCommand 
        { 
            get { return emailAccountsCommand; }
            set 
            {
                if (emailAccountsCommand != value)
                {
                    emailAccountsCommand = value;
                    RaisePropertyChanged("EmailAccountsCommand");
                }
            }
        }

        public object MessageListView
        {
            get { return messageListView; }
            set 
            {
                if (messageListView != value)
                {
                    messageListView = value;
                    RaisePropertyChanged("MessageListView");
                }
            }
        }

        public object MessageContentView
        {
            get { return messageContentView; }
            set 
            {
                if (messageContentView != value)
                {
                    messageContentView = value;
                    RaisePropertyChanged("MessageContentView");
                }
            }
        }


        public void Show()
        {
            ViewCore.Show();
        }

        public void Close()
        {
            ViewCore.Close();
        }
    }
}
