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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications;
using FAP.Application.Views;
using System.Windows.Input;
using Fap.Foundation;
using FAP.Domain.Entities;
using FAP.Application.Controllers;

namespace FAP.Application.ViewModels
{
    public class ConversationViewModel: ViewModel<IConverstationView>
    {
        private string currentChatMessage;
        private ICommand sendChatMessage;
        private Conversation conversation;
        private ICommand close;

        private PopupWindowController popupWindowController;

        public ConversationViewModel(IConverstationView view, PopupWindowController p)
            : base(view)
        {
            popupWindowController = p;
        }


        public Conversation Conversation
        {
            get { return conversation; }
            set
            {
                conversation = value;
                RaisePropertyChanged("Conversation");
                value.UIMessages.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(UIMessages_CollectionChanged);
            }
        }

        private void UIMessages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (popupWindowController.ActiveTab != this)
                popupWindowController.Highlight(this);
            popupWindowController.FlashIfNotActive();
        }


        public string CurrentChatMessage
        {
            get { return currentChatMessage; }
            set
            {
                currentChatMessage = value;
                RaisePropertyChanged("CurrentChatMessage");
            }
        }

        public ICommand SendChatMessage
        {
            get { return sendChatMessage; }
            set
            {
                sendChatMessage = value;
                RaisePropertyChanged("SendChatMessage");
            }
        }

        public ICommand Close
        {
            get { return close; }
            set
            {
                close = value;
                RaisePropertyChanged("Close");
            }
        }
    }
}
