using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Application.Views;

namespace Server.Console
{
    class SharesView : ISharesView
    {
        #region IView Members

        public object DataContext
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
            }
        }

        #endregion
    }
}
