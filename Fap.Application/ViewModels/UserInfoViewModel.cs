using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications;
using Fap.Application.Views;
using Fap.Network.Entity;

namespace Fap.Application.ViewModels
{
    public class UserInfoViewModel : ViewModel<IUserInfo>
    {

        public UserInfoViewModel(IUserInfo view)
            : base(view)
        {
        }

        public Node Node { set; get; }
    }
}
