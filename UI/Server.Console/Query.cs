using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Application.Views;

namespace Server.Console
{
    class Query : IQuery
    {
        #region IQuery Members

        public bool SelectFolder(out string result)
        {
            throw new NotImplementedException();
        }

        public bool SelectFile(out string result)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
