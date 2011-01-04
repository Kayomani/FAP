using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Waf.Applications;
using System.Waf.Applications.Services;
using System.Windows.Input;
using BookLibrary.Applications.Properties;
using BookLibrary.Applications.Views;

namespace BookLibrary.Applications.ViewModels
{
    [Export]
    public class ShellViewModel : ViewModel<IShellView>
    {
        private readonly IMessageService messageService;
        private readonly DelegateCommand aboutCommand;
        private ICommand saveCommand;
        private ICommand exitCommand;
        private bool isValid = true;
        private object bookListView;
        private object bookView;
        private object personListView;
        private object personView;

        
        [ImportingConstructor]
        public ShellViewModel(IShellView view, IMessageService messageService) : base(view)
        {
            this.messageService = messageService;
            this.aboutCommand = new DelegateCommand(ShowAboutMessage);

            view.Closing += ViewClosing;
        }


        public static string Title { get { return ApplicationInfo.ProductName; } }

        public ICommand AboutCommand { get { return aboutCommand; } }

        public ICommand SaveCommand
        {
            get { return saveCommand; }
            set
            {
                if (saveCommand != value)
                {
                    saveCommand = value;
                    RaisePropertyChanged("SaveCommand");
                }
            }
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

        public bool IsValid
        {
            get { return isValid; }
            set
            {
                if (isValid != value)
                {
                    isValid = value;
                    RaisePropertyChanged("IsValid");
                }
            }
        }

        public object BookListView
        {
            get { return bookListView; }
            set
            {
                if (bookListView != value)
                {
                    bookListView = value;
                    RaisePropertyChanged("BookListView");
                }
            }
        }

        public object BookView
        {
            get { return bookView; }
            set
            {
                if (bookView != value)
                {
                    bookView = value;
                    RaisePropertyChanged("BookView");
                }
            }
        }

        public object PersonListView
        {
            get { return personListView; }
            set
            {
                if (personListView != value)
                {
                    personListView = value;
                    RaisePropertyChanged("PersonListView");
                }
            }
        }

        public object PersonView
        {
            get { return personView; }
            set
            {
                if (personView != value)
                {
                    personView = value;
                    RaisePropertyChanged("PersonView");
                }
            }
        }


        public event CancelEventHandler Closing;


        public void Show()
        {
            ViewCore.Show();
        }

        public void Close()
        {
            ViewCore.Close();
        }

        protected virtual void OnClosing(CancelEventArgs e)
        {
            if (Closing != null) { Closing(this, e); }
        }

        private void ViewClosing(object sender, CancelEventArgs e)
        {
            OnClosing(e);
        }

        private void ShowAboutMessage()
        {
            messageService.ShowMessage(string.Format(CultureInfo.CurrentCulture, Resources.AboutText,
                ApplicationInfo.ProductName, ApplicationInfo.Version));
        }
    }
}
