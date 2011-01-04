using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using EmailClient.Applications.ViewModels;
using EmailClient.Domain;
using EmailClient.Applications.Views;
using System.ComponentModel.Composition.Hosting;
using System.Waf.Applications;

namespace EmailClient.Applications.Controllers
{
    internal class CreateEmailAccountController : Controller
    {
        private readonly CompositionContainer container;
        private readonly IShellView shellView;
        private readonly EmailClientRoot emailClientRoot;
        private readonly DelegateCommand backCommand;
        private readonly DelegateCommand nextCommand;
        private readonly DelegateCommand cancelCommand;
        private readonly EmailAccountsViewModel emailAccountsViewModel;
        private readonly CreateEmailAccountViewModel createEmailAccountViewModel;
        
        private Pop3SettingsViewModel pop3SettingsViewModel;
        private ExchangeSettingsViewModel exchangeSettingsViewModel;


        [ImportingConstructor]
        public CreateEmailAccountController(CompositionContainer container, IShellView shellView, 
            EmailClientRoot emailClientRoot)
        {
            this.container = container;
            this.shellView = shellView;
            this.emailClientRoot = emailClientRoot;
            emailAccountsViewModel = container.GetExportedValue<EmailAccountsViewModel>();
            createEmailAccountViewModel = container.GetExportedValue<CreateEmailAccountViewModel>();

            backCommand = new DelegateCommand(Back, CanBack);
            nextCommand = new DelegateCommand(Next);
            cancelCommand = new DelegateCommand(Close);
        }


        public void Initialize()
        {
            emailAccountsViewModel.BackCommand = backCommand;
            emailAccountsViewModel.NextCommand = nextCommand;
            emailAccountsViewModel.CancelCommand = cancelCommand;
        }

        public void Run()
        {
            emailAccountsViewModel.ContentView = createEmailAccountViewModel.View;
            emailAccountsViewModel.ShowDialog(shellView);
        }

        private void Close()
        {
            emailAccountsViewModel.Close();
        }


        // Wizard workflow

        private bool CanBack()
        {
            return emailAccountsViewModel.ContentView != createEmailAccountViewModel.View;
        }

        private void Back()
        {
            emailAccountsViewModel.ContentView = createEmailAccountViewModel.View;
            UpdateCommandsState();
        }

        private void Next()
        {
            if (emailAccountsViewModel.ContentView == createEmailAccountViewModel.View)
            {
                if (createEmailAccountViewModel.IsPop3Checked)
                {
                    ShowPop3SettingsView();
                }
                else if (createEmailAccountViewModel.IsExchangeChecked)
                {
                    ShowExchangeSettingsView();
                }
            }
            else if (pop3SettingsViewModel != null
                && emailAccountsViewModel.ContentView == pop3SettingsViewModel.View)
            {
                SavePop3Settings();
                Close();
            }
            else if (exchangeSettingsViewModel != null
                && emailAccountsViewModel.ContentView == exchangeSettingsViewModel.View)
            {
                SaveExchangeSettings();
                Close();
            }

            UpdateCommandsState();
        }

        private void UpdateCommandsState()
        {
            backCommand.RaiseCanExecuteChanged();
            nextCommand.RaiseCanExecuteChanged();
        }

        
        // Show wizard pages

        private void ShowPop3SettingsView()
        {
            Pop3Settings pop3Settings = new Pop3Settings();
            IPop3SettingsView pop3SettingsView = container.GetExportedValue<IPop3SettingsView>();
            pop3SettingsViewModel = new Pop3SettingsViewModel(pop3SettingsView, pop3Settings);
            emailAccountsViewModel.ContentView = pop3SettingsViewModel.View;
        }

        private void ShowExchangeSettingsView()
        {
            ExchangeSettings exchangeSettings = new ExchangeSettings();
            IExchangeSettingsView exchangeSettingsView = container.GetExportedValue<IExchangeSettingsView>();
            exchangeSettingsViewModel = new ExchangeSettingsViewModel(exchangeSettingsView, exchangeSettings);
            emailAccountsViewModel.ContentView = exchangeSettingsViewModel.View;
        }

        private void SavePop3Settings()
        {
            emailClientRoot.AddEmailAccount(pop3SettingsViewModel.Model);
        }

        private void SaveExchangeSettings()
        {
            emailClientRoot.AddEmailAccount(exchangeSettingsViewModel.Model);
        }
    }
}
