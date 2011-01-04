using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Foundation;

namespace EmailClient.Domain
{
    public class Pop3Settings : Model, IEmailAccountSettings
    {
        private readonly UserCredits pop3UserCredits;
        private readonly UserCredits smtpUserCredits;
        private string pop3ServerPath;
        private string smtpServerPath;


        public Pop3Settings()
        {
            pop3UserCredits = new UserCredits();
            smtpUserCredits = new UserCredits();
        }


        public string Pop3ServerPath
        {
            get { return pop3ServerPath; }
            set
            {
                if (pop3ServerPath != value)
                {
                    pop3ServerPath = value;
                    RaisePropertyChanged("Pop3ServerPath");
                }
            }
        }

        public UserCredits Pop3UserCredits
        {
            get { return pop3UserCredits; }
        }

        public string SmtpServerPath
        {
            get { return smtpServerPath; }
            set
            {
                if (smtpServerPath != value)
                {
                    smtpServerPath = value;
                    RaisePropertyChanged("SmtpServerPath");
                }
            }
        }

        public UserCredits SmtpUserCredits
        {
            get { return smtpUserCredits; }
        }


        public override string ToString()
        {
            return "Pop3Settings: " + Pop3ServerPath + " (" + Pop3UserCredits.UserName + ")"
                + " / " + SmtpServerPath + " (" + SmtpUserCredits.UserName + ")";
        }
    }
}
