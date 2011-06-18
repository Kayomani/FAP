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

using System.Waf.Applications;
using FAP.Application.Views;

namespace FAP.Application.ViewModels
{
    public class MessageBoxViewModel : ViewModel<IMessageBoxView>
    {
        private string message;
        private string response;

        public MessageBoxViewModel(IMessageBoxView view)
            : base(view)
        {
        }

        public string Message
        {
            set
            {
                message = value;
                RaisePropertyChanged("Message");
            }
            get { return message; }
        }

        public string Response
        {
            set
            {
                response = value;
                RaisePropertyChanged("Response");
            }
            get { return response; }
        }


        public bool ShowDialog(object parent)
        {
            bool? response = ViewCore.ShowDialog(parent);
            if (response.HasValue)
                return response.Value;
            return false;
        }

        public bool ShowDialog()
        {
            bool? response = ViewCore.ShowDialog();
            if (response.HasValue)
                return response.Value;
            return false;
        }
    }
}