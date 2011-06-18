#region Copyright Kayomani 2011.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.

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

using System.Waf.Applications;
using System.Windows.Input;
using FAP.Application.Views;
using FAP.Domain.Entities;
using Fap.Foundation;

namespace FAP.Application.ViewModels
{
    public class CompareViewModel : ViewModel<ICompareView>
    {
        private SafeObservable<CompareNode> data;
        private bool enableStart = true;
        private ICommand run;
        private string status;

        public CompareViewModel(ICompareView view)
            : base(view)
        {
        }

        public bool EnableRun
        {
            get { return enableStart; }
            set
            {
                enableStart = value;
                RaisePropertyChanged("EnableRun");
            }
        }

        public ICommand Run
        {
            get { return run; }
            set
            {
                run = value;
                RaisePropertyChanged("Run");
            }
        }

        public SafeObservable<CompareNode> Data
        {
            get { return data; }
            set
            {
                data = value;
                RaisePropertyChanged("Data");
            }
        }

        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                RaisePropertyChanged("Status");
            }
        }
    }
}