﻿#region Copyright Kayomani 2010.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
/**
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or any 
    later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fap.Foundation;
using System.Waf.Applications;
using Fap.Foundation.Logging;
using Fap.Application.Views;

namespace Fap.Application.ViewModels
{
    public class LoggerViewModel : ViewModel<ILogView>
    {

        private SafeObservable<Log> logs;

        public LoggerViewModel(ILogView view)
            : base(view)
        {
        }


        public SafeObservable<Log> Logs
        {
            get { return logs; }
            set
            {
                logs = value;
                RaisePropertyChanged("Logs");
            }
        }

    }
}
