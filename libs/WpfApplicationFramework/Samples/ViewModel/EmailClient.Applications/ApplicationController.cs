using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Waf.Applications;
using EmailClient.Applications.ViewModels;
using EmailClient.Applications.Views;
using EmailClient.Domain;

namespace EmailClient.Applications.Controllers
{
    [Export]
    public class ApplicationController : Controller
    {
        private readonly CompositionContainer container;
        private readonly ShellViewModel shellViewModel;
        private readonly MessageListViewModel messageListViewModel;
        private readonly MessageContentViewModel messageContentViewModel;
        private readonly EmailClientRoot emailClientRoot;


        [ImportingConstructor]
        public ApplicationController(CompositionContainer container)
        {
            if (container == null) { throw new ArgumentNullException("container"); }
            
            this.container = container;
            shellViewModel = container.GetExportedValue<ShellViewModel>();
            messageListViewModel = container.GetExportedValue<MessageListViewModel>();
            messageContentViewModel = container.GetExportedValue<MessageContentViewModel>();
            emailClientRoot = new EmailClientRoot();
        }


        public void Initialize()
        {
            shellViewModel.ExitCommand = new DelegateCommand(Close);
            shellViewModel.EmailAccountsCommand = new DelegateCommand(RunCreateEmailAccountController);
            shellViewModel.MessageListView = messageListViewModel.View;
            shellViewModel.MessageContentView = messageContentViewModel.View;
        }

        public void Run()
        {
            shellViewModel.Show();
        }

        private void Close()
        {
            shellViewModel.Close();
        }

        private void RunCreateEmailAccountController()
        {
            IShellView shellView = container.GetExportedValue<IShellView>();
            CreateEmailAccountController useCaseController = new CreateEmailAccountController(container, 
                shellView, emailClientRoot);
            useCaseController.Initialize();
            useCaseController.Run();

            // Just for debugging
            Trace.WriteLine(string.Format(null, "\n> Current email accounts ({0}):", 
                emailClientRoot.EmailAccounts.Count()));
            emailClientRoot.EmailAccounts.ToList().ForEach(x => Trace.WriteLine(x));
        }
    }
}
