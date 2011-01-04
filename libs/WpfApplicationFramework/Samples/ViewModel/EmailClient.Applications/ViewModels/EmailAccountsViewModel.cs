using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmailClient.Applications.Views;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Waf.Applications;
using System.Windows.Input;

namespace EmailClient.Applications.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class EmailAccountsViewModel : ViewModel<IEmailAccountsView>
    {
        private ICommand backCommand;
        private ICommand nextCommand;
        private ICommand cancelCommand;
        private object contentView;


        [ImportingConstructor]
        public EmailAccountsViewModel(IEmailAccountsView view) : base(view)
        {
        }


        public ICommand BackCommand 
        { 
            get { return backCommand; }
            set 
            {
                if (backCommand != value)
                {
                    backCommand = value;
                    RaisePropertyChanged("BackCommand");
                }
            }
        }

        public ICommand NextCommand 
        { 
            get { return nextCommand; }
            set 
            {
                if (nextCommand != value)
                {
                    nextCommand = value;
                    RaisePropertyChanged("NextCommand");
                }
            }
        }

        public ICommand CancelCommand 
        { 
            get { return cancelCommand; }
            set 
            {
                if (cancelCommand != value)
                {
                    cancelCommand = value;
                    RaisePropertyChanged("CancelCommand");
                }
            }
        }

        public object ContentView
        {
            get { return contentView; }
            set 
            {
                if (contentView != value)
                {
                    contentView = value;
                    RaisePropertyChanged("ContentView");
                }
            }
        }


        public void ShowDialog(object owner)
        {
            ViewCore.ShowDialog(owner);
        }

        public void Close()
        {
            ViewCore.Close();
        }
    }
}
