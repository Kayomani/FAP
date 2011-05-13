#region Copyright Kayomani 2010.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
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
using FAP.Application.Views;
using System.Waf.Applications;
using Fap.Foundation;
using System.Windows.Input;
using FAP.Domain.Entities;

namespace FAP.Application.ViewModels
{
    public class TrayIconViewModel : ViewModel<ITrayIconView>
    {
        private ICommand exit;
        private ICommand open;
        private ICommand shares;
        private ICommand settings;
        private ICommand queue;
        private ICommand viewshare;
        private ICommand compare;
        private ICommand openExternal;


        public TrayIconViewModel(ITrayIconView view)
            : base(view)
        {
        }

        public Model Model { set; get; }
       
        public bool ShowIcon
        {
            get
            {
                return ViewCore.ShowIcon;
            }
            set
            {
                ViewCore.ShowIcon = value;
            }
        }

        public ICommand OpenExternal
        {
            set
            {
                openExternal = value;
                RaisePropertyChanged("OpenExternal");
            }
            get
            {
                return openExternal;
            }
        }

        public ICommand Compare
        {
            set
            {
                compare = value;
                RaisePropertyChanged("Compare");
            }
            get
            {
                return compare;
            }
        }

        public ICommand ViewShare
        {
            set
            {
                viewshare = value;
                RaisePropertyChanged("ViewShare");
            }
            get
            {
                return viewshare;
            }
        }

        public ICommand Queue
        {
            set
            {
                queue = value;
                RaisePropertyChanged("Queue");
            }
            get
            {
                return queue;
            }
        }

        public ICommand Settings
        {
            set
            {
                settings = value;
                RaisePropertyChanged("Settings");
            }
            get
            {
                return settings;
            }
        }

        public ICommand Shares
        {
            set
            {
                shares = value;
                RaisePropertyChanged("Shares");
            }
            get
            {
                return shares;
            }
        }


        public ICommand Open
        {
            set
            {
                open = value;
                RaisePropertyChanged("Open");
            }
            get
            {
                return open;
            }
        }

        public ICommand Exit
        {
            set
            {
                exit = value;
                RaisePropertyChanged("Exit");
            }
            get
            {
                return exit;
            }
        }

        public void Dispose()
        {
            ViewCore.Dispose();

        }
    }
}
