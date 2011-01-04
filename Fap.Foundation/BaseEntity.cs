using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Fap.Foundation
{
    public class BaseEntity : INotifyPropertyChanged
    {
        protected void NotifyChange(string path)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(path));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
