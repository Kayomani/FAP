using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Foundation;

namespace EmailClient.Domain
{
    public class ExchangeSettings : Model, IEmailAccountSettings
    {
        private string serverPath;
        private string userName;


        public string ServerPath
        {
            get { return serverPath; }
            set 
            {
                if (serverPath != value)
                {
                    serverPath = value;
                    RaisePropertyChanged("ServerPath");
                }
            }
        }

        public string UserName
        {
            get { return userName; }
            set 
            {
                if (userName != value)
                {
                    userName = value;
                    RaisePropertyChanged("UserName");
                }
            }
        }


        public override string ToString()
        {
            return "ExchangeSettings: " + ServerPath + " (" + UserName + ")";
        }
    }
}
