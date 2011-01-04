using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmailClient.Applications.Views;
using System.ComponentModel.Composition;
using EmailClient.Domain;
using System.Reflection;
using System.ComponentModel;
using System.Waf.Applications;

namespace EmailClient.Applications.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class Pop3SettingsViewModel : ViewModel<IPop3SettingsView>
    {
        private readonly Pop3Settings model;
        private bool useSameUserCredits;
        private string smtpUserName;
        private string smtpPassword;


        [ImportingConstructor]
        public Pop3SettingsViewModel(IPop3SettingsView view, Pop3Settings pop3Settings) : base(view)
        {
            if (pop3Settings == null) { throw new ArgumentNullException("pop3Settings"); }
            
            this.model = pop3Settings;
            AddWeakEventListener(this.model.Pop3UserCredits, Pop3UserCreditsPropertyChanged);
        }


        public Pop3Settings Model { get { return model; } }

        public bool UseSameUserCredits
        {
            get { return useSameUserCredits; }
            set 
            {
                if (useSameUserCredits != value)
                {
                    useSameUserCredits = value;
                    if (useSameUserCredits)
                    {
                        Model.SmtpUserCredits.UserName = Model.Pop3UserCredits.UserName;
                        Model.SmtpUserCredits.Password = Model.Pop3UserCredits.Password;
                    }
                    else
                    {
                        Model.SmtpUserCredits.UserName = SmtpUserName;
                        Model.SmtpUserCredits.Password = SmtpPassword;
                    }
                    RaisePropertyChanged("UseSameUserCredits");
                }
            }
        }

        public string Pop3UserName
        {
            get { return Model.Pop3UserCredits.UserName; }
            set 
            {
                if (Pop3UserName != value)
                {
                    Model.Pop3UserCredits.UserName = value;
                    if (UseSameUserCredits)
                    {
                        Model.SmtpUserCredits.UserName = value;
                    }
                    RaisePropertyChanged("Pop3UserName");
                }
            }
        }

        public string Pop3Password
        {
            get { return Model.Pop3UserCredits.Password; }
            set 
            {
                if (Pop3Password != value)
                {
                    Model.Pop3UserCredits.Password = value;
                    if (UseSameUserCredits)
                    {
                        Model.SmtpUserCredits.Password = value;
                    }
                    RaisePropertyChanged("Pop3Password");
                }
            }
        }

        public string SmtpUserName
        {
            get { return smtpUserName; }
            set 
            {
                if (smtpUserName != value)
                {
                    smtpUserName = value;
                    Model.SmtpUserCredits.UserName = value;
                    RaisePropertyChanged("SmtpUserName");
                }
            }
        }

        public string SmtpPassword
        {
            get { return smtpPassword; }
            set 
            {
                if (smtpPassword != value)
                {
                    smtpPassword = value;
                    Model.SmtpUserCredits.Password = value;
                    RaisePropertyChanged("SmtpPassword");
                }
            }
        }

        private void Pop3UserCreditsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "UserName")
            {
                RaisePropertyChanged("Pop3UserName");
            }
            else if (e.PropertyName == "Password")
            {
                RaisePropertyChanged("Pop3Password");
            }
        }
    }
}
