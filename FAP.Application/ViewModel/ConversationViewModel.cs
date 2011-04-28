using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications;
using FAP.Application.Views;
using System.Windows.Input;
using Fap.Foundation;
using FAP.Domain.Entities;

namespace FAP.Application.ViewModels
{
    public class ConversationViewModel: ViewModel<IConverstationView>
    {
        private string currentChatMessage;
        private ICommand sendChatMessage;
        private Conversation conversation;
        private ICommand close;


        public ConversationViewModel(IConverstationView view)
            : base(view)
        {
        }


        public Conversation Conversation
        {
            get { return conversation; }
            set
            {
                conversation = value;
                RaisePropertyChanged("Conversation");
            }
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
