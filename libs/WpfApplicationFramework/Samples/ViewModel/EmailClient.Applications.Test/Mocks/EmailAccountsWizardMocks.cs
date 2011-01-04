using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmailClient.Applications.Views;
using EmailClient.Applications.ViewModels;

namespace EmailClient.Applications.Test.Mocks
{
    public class EmailAccountsViewMock : ViewMock<EmailAccountsViewModel>, IEmailAccountsView
    {
        public bool IsVisible { get; private set; }

        public bool? ShowDialog(object owner)
        {
            IsVisible = true;
            return true;
        }

        public void Close()
        {
            IsVisible = false;
        }
    }

    public class CreateEmailAccountViewMock : ViewMock<CreateEmailAccountViewModel>, ICreateEmailAccountView
    {
    }

    public class ExchangeSettingsViewMock : ViewMock<ExchangeSettingsViewModel>, IExchangeSettingsView
    {
    }

    public class Pop3SettingsViewMock : ViewMock<Pop3SettingsViewModel>, IPop3SettingsView
    {
    }
}
