using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Foundation;
using System.Collections.ObjectModel;

namespace EmailClient.Domain
{
    public class EmailClientRoot : Model
    {
        private readonly ObservableCollection<IEmailAccountSettings> emailAccounts;


        public EmailClientRoot()
        {
            emailAccounts = new ObservableCollection<IEmailAccountSettings>();
        }


        public IEnumerable<IEmailAccountSettings> EmailAccounts
        {
            get { return emailAccounts; }
        }


        public void AddEmailAccount(IEmailAccountSettings emailAccount)
        {
            emailAccounts.Add(emailAccount);
        }
    }
}
