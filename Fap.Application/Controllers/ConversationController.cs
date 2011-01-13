using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Fap.Domain.Entity;
using Fap.Network.Entity;
using Fap.Application.ViewModels;
using System.Waf.Applications;
using Fap.Network;
using Fap.Domain.Verbs;
using System.Threading;
using Fap.Foundation;
using Fap.Domain.Services;

namespace Fap.Application.Controllers
{
    public class ConversationController
    {
        private readonly PopupWindowController windowController;
        private readonly Model model;
        private readonly LANPeerConnectionService peerController;
        private IContainer container;
        private SafeObservable<ConversationViewModel> viewModels = new SafeObservable<ConversationViewModel>();

        public ConversationController(IContainer container)
        {
            windowController = container.Resolve<PopupWindowController>();
            model = container.Resolve<Model>();
            peerController = container.Resolve<LANPeerConnectionService>();
            this.container = container;
        }

        public void Initalise()
        {
            model.Conversations.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Conversations_CollectionChanged);
            windowController.OnTabClosing += new PopupWindowController.TabClosing(chatPopupController_OnTabClosing);
            model.OnNewConverstation += new Model.NewConversation(model_OnNewConverstation);
        }

        

        private bool model_OnNewConverstation(string id, string message)
        {
            var peer = model.Peers.Where(p => p.ID == id).FirstOrDefault();
            if(null!=peer)
            {
                Conversation conv = model.Conversations.Where(c=>c.OtherParty==peer).FirstOrDefault();

                if (null == conv)
                {
                    conv = new Conversation();
                    conv.OtherParty = peer;
                    conv.Messages.Add(peer.Nickname + ": " + message);
                    model.Conversations.Add(conv);
                }
                else
                {
                    conv.Messages.Add(peer.Nickname + ": " + message);
                }


                SafeObservableStatic.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
          new Action(
           delegate()
           {
               var vm = viewModels.Where(v => v.Conversation == conv).FirstOrDefault();
               if (null != vm)
               {
                   if (windowController.ActiveTab != vm)
                   {
                       windowController.Highlight(vm);
                   }
               }
               windowController.FlashIfNotActive();
           }
          ));
              
               
                return true;
            }
            return false;
        }

        private void chatPopupController_OnTabClosing(object o)
        {
           ConversationViewModel vm = o as ConversationViewModel;
           if (null != vm)
           {
               model.Conversations.Remove(vm.Conversation);
               viewModels.Remove(vm);
           }
        }

        private void Conversations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (Conversation c in e.NewItems)
                {
                    SafeObservableStatic.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                 delegate()
                 {
                     var vm = container.Resolve<ConversationViewModel>();
                     vm.SendChatMessage = new DelegateCommand(SendChatMessage);
                     vm.Conversation = c;
                     viewModels.Add(vm);
                     windowController.AddWindow(vm.View, c.OtherParty.Nickname);
                 }
                ));
                }
            }
        }


        public void CreateConversation(Node n)
        {
            var search = viewModels.Where(c => c.Conversation.OtherParty == n).FirstOrDefault();
            if (null == search)
            {
                //New conversation
                Conversation c = new Conversation();
                c.OtherParty = n;
                model.Conversations.Lock();
                model.Conversations.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Conversations_CollectionChanged);
                model.Conversations.Add(c);
                model.Conversations.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Conversations_CollectionChanged);
                model.Conversations.Unlock();
                var vm = container.Resolve<ConversationViewModel>();
                vm.SendChatMessage = new DelegateCommand(SendChatMessage);
                vm.Conversation = c;
                viewModels.Add(vm);
                windowController.AddWindow(vm.View, n.Nickname);
            }
            else
            {
                //Converstation already open for this person so just switch to it
                windowController.SwitchToTab(search);
            }
        }


        private void SendChatMessage(object ivm)
        {
            ConversationViewModel vm = ivm as ConversationViewModel;
            if (null != vm && !string.IsNullOrEmpty(vm.CurrentChatMessage))
            {
                vm.Conversation.Messages.Add("You: " + vm.CurrentChatMessage);
                ThreadPool.QueueUserWorkItem(new WaitCallback(SendMessageAsync), ivm);
            }
        }

        private void SendMessageAsync(object ivm)
        {
            ConversationViewModel vm = ivm as ConversationViewModel;
            if (null != vm && !string.IsNullOrEmpty(vm.CurrentChatMessage))
            {
               
                Client c = container.Resolve<Client>();
                ConversationVerb verb = new ConversationVerb(model);
                verb.Nickname = model.Node.Nickname;
                verb.Message = vm.CurrentChatMessage;
                verb.SourceID = model.Node.ID;

                vm.CurrentChatMessage = string.Empty;

                if (!c.Execute(verb, vm.Conversation.OtherParty))
                {
                    vm.Conversation.Messages.Add("The other party failed to receive your message, please try again.");
                }
            }
        }

        public void Close()
        {
            model.Conversations.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Conversations_CollectionChanged);
            windowController.Close();
        }
    }
}
