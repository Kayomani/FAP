using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications;
using FAP.Application.Views;
using FAP.Domain.Entities;

namespace FAP.Application.ViewModels
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
