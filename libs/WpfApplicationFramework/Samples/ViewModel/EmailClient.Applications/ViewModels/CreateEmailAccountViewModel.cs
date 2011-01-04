using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmailClient.Applications.Views;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Waf.Applications;

namespace EmailClient.Applications.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class CreateEmailAccountViewModel : ViewModel<ICreateEmailAccountView>
    {
        private bool isPop3Checked = true;
        private bool isExchangeChecked;


        [ImportingConstructor]
        public CreateEmailAccountViewModel(ICreateEmailAccountView view) : base(view)
        {
        }


        public bool IsPop3Checked
        {
            get { return isPop3Checked; }
            set 
            {
                if (isPop3Checked != value)
                {
                    isPop3Checked = value;
                    IsExchangeChecked = !value;
                    RaisePropertyChanged("IsPop3Checked");
                }
            }
        }

        public bool IsExchangeChecked
        {
            get { return isExchangeChecked; }
            set 
            {
                if (isExchangeChecked != value)
                {
                    isExchangeChecked = value;
                    IsPop3Checked = !value;
                    RaisePropertyChanged("IsExchangeChecked");
                }
            }
        }
    }
}
