﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications;

namespace EmailClient.Applications.Views
{
    public interface IShellView : IView
    {
        void Show();
        void Close();
    }
}
