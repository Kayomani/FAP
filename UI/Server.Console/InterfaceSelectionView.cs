using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Application.Views;

namespace Server.Console
{
    public class InterfaceSelectionView : IInterfaceSelectionView
    {
        #region IInterfaceSelectionView Members

        public bool? ShowDialog()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        #endregion

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
