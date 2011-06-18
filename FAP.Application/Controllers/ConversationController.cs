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

using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Waf.Applications;
using Autofac;
using FAP.Application.ViewModels;
using FAP.Domain.Entities;
using FAP.Domain.Net;
using FAP.Domain.Verbs;
using Fap.Foundation;

namespace FAP.Application.Controllers
{
    public class ConversationController : IConversationController
    {
        private readonly IContainer container;
        private readonly SafeObservedCollection<Conversation> conversations = new SafeObservedCollection<Conversation>();
        private readonly Model model;
        private readonly SafeObservingCollection<Conversation> uiConversations;
        private readonly SafeObservable<ConversationViewModel> viewModels = new SafeObservable<ConversationViewModel>();
        private readonly PopupWindowController windowController;

        public ConversationController(IContainer container, Model m)
        {
            windowController = container.Resolve<PopupWindowController>();
            model = m;
            this.container = container;
            uiConversations = new SafeObservingCollection<Conversation>(conversations);
            uiConversations.CollectionChanged += uiConversations_CollectionChanged;
            windowController.OnTabClosing += chatPopupController_OnTabClosing;
        }

        #region IConversationController Members

        public bool HandleMessage(string id, string nickname, string message)
        {
            Node peer = model.Network.Nodes.Where(p => p.ID == id).FirstOrDefault();

            if (null != peer)
            {
                Conversation conv = conversations.Where(c => c.OtherParty == peer).FirstOrDefault();
                if (null == conv)
                {
                    conv = new Conversation();
                    conv.OtherParty = peer;
                    conv.Messages.Add(peer.Nickname + ": " + message);
                    conversations.Add(conv);
                }
                else
                {
                    conv.Messages.Add(peer.Nickname + ": " + message);
                }
                return true;
            }
            return false;
        }

        #endregion

        /// <summary>
        /// New conversion has been added - add a window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiConversations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Conversation c in e.NewItems)
                {
                    var vm = container.Resolve<ConversationViewModel>();
                    vm.SendChatMessage = new DelegateCommand(SendChatMessage);
                    vm.Conversation = c;
                    viewModels.Add(vm);
                    windowController.AddWindow(vm.View, c.OtherParty.Nickname);
                }
            }
        }

        private void chatPopupController_OnTabClosing(object o)
        {
            var vm = o as ConversationViewModel;
            if (null != vm)
            {
                conversations.Remove(vm.Conversation);
                vm.Conversation.Dispose();
                viewModels.Remove(vm);
            }
        }

        public void CreateConversation(Node n)
        {
            ConversationViewModel search = viewModels.Where(c => c.Conversation.OtherParty == n).FirstOrDefault();
            if (null == search)
            {
                //New conversation
                var c = new Conversation();
                c.OtherParty = n;
                conversations.Add(c);
            }
            else
            {
                //Converstation already open for this person so just switch to it
                windowController.SwitchToTab(search);
            }
        }

        private void SendChatMessage(object ivm)
        {
            var vm = ivm as ConversationViewModel;
            if (null != vm && !string.IsNullOrEmpty(vm.CurrentChatMessage))
            {
                vm.Conversation.Messages.Add("You: " + vm.CurrentChatMessage);
                ThreadPool.QueueUserWorkItem(SendMessageAsync, ivm);
            }
        }

        private void SendMessageAsync(object ivm)
        {
            var vm = ivm as ConversationViewModel;
            if (null != vm && !string.IsNullOrEmpty(vm.CurrentChatMessage))
            {
                string message = vm.CurrentChatMessage;

                var c = new Client(model.LocalNode);
                var verb = new ConversationVerb();
                verb.Nickname = model.LocalNode.Nickname;
                verb.Message = message;
                verb.SourceID = model.LocalNode.ID;
                vm.CurrentChatMessage = string.Empty;

                if (!c.Execute(verb, vm.Conversation.OtherParty))
                    vm.Conversation.Messages.Add("The other party failed to receive your message, please try again.");
            }
        }
    }
}