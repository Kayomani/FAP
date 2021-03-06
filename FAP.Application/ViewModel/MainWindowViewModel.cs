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

using System.Waf.Applications;
using System.Windows.Input;
using System.Windows.Threading;
using FAP.Application.Views;
using FAP.Domain;
using FAP.Domain.Entities;
using Fap.Foundation;

namespace FAP.Application.ViewModels
{
    public class MainWindowViewModel : ViewModel<IMainWindow>
    {
        private bool allowClose;
        private string avatar;
        private ICommand chat;
        private SafeObservingCollection<string> chatList;
        private ICommand closing;
        private ICommand compare;
        private string currentChatMessage;
        private string description;
        private ICommand editShares;
        private string globalStats;
        private string localStats;
        private Model model;
        private string networkInfo;
        private string networkStats;
        private string nickname;
        private Node node;
        private string nodeStatus;
        private ICommand openExternal;
        private SafeFilteredObservingCollection<Node> peers;
        private ICommand search;
        private object selectedClient;
        private ICommand sendChatMessage;
        private SafeObservingCollection<TransferSession> sessions;
        private ICommand settings;
        private PeerSortType sortType;
        private ICommand userinfo;
        private ICommand viewQueue;
        private ICommand viewShare;
        private bool visible;
        private string windowTitle;

        public MainWindowViewModel(IMainWindow view)
            : base(view)
        {
        }

        public PeerSortType PeerSortType
        {
            get { return sortType; }
            set
            {
                sortType = value;
                RaisePropertyChanged("PeerSortType");
            }
        }

        public Model Model
        {
            get { return model; }
            set
            {
                model = value;
                RaisePropertyChanged("Model");
            }
        }

        public string NodeStatus
        {
            get { return nodeStatus; }
            set
            {
                if (nodeStatus != value)
                {
                    nodeStatus = value;
                    RaisePropertyChanged("NodeStatus");
                }
            }
        }


        public string CurrentNetworkStatus
        {
            get { return networkStats; }
            set
            {
                if (networkStats != value)
                {
                    networkStats = value;
                    RaisePropertyChanged("CurrentNetworkStatus");
                }
            }
        }

        public string LocalStats
        {
            get { return localStats; }
            set
            {
                if (localStats != value)
                {
                    localStats = value;
                    RaisePropertyChanged("LocalStats");
                }
            }
        }

        public string GlobalStats
        {
            get { return globalStats; }
            set
            {
                if (globalStats != value)
                {
                    globalStats = value;
                    RaisePropertyChanged("GlobalStats");
                }
            }
        }

        public bool Visible
        {
            get { return visible; }
            set
            {
                visible = value;
                RaisePropertyChanged("Visible");
            }
        }

        public bool AllowClose
        {
            get { return allowClose; }
            protected set
            {
                allowClose = value;
                RaisePropertyChanged("AllowClose");
            }
        }

        public SafeFilteredObservingCollection<Node> Peers
        {
            get { return peers; }
            set
            {
                peers = value;
                RaisePropertyChanged("Peers");
            }
        }

        public Node Node
        {
            get { return node; }
            set
            {
                node = value;
                RaisePropertyChanged("Node");
            }
        }

        public string Avatar
        {
            get { return avatar; }
            set
            {
                avatar = value;
                RaisePropertyChanged("Avatar");
            }
        }

        public string WindowTitle
        {
            get { return windowTitle; }
            set
            {
                windowTitle = value;
                RaisePropertyChanged("WindowTitle");
            }
        }

        public string NetworkStatus
        {
            get { return networkInfo; }
            set
            {
                networkInfo = value;
                RaisePropertyChanged("NetworkStatus");
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

        public string Nickname
        {
            get { return nickname; }
            set
            {
                nickname = value;
                RaisePropertyChanged("Nickname");
            }
        }

        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                RaisePropertyChanged("Description");
            }
        }

        public SafeObservingCollection<TransferSession> Sessions
        {
            get { return sessions; }
            set
            {
                sessions = value;
                RaisePropertyChanged("Sessions");
            }
        }

        public SafeObservingCollection<string> ChatMessages
        {
            get { return chatList; }
            set
            {
                chatList = value;
                RaisePropertyChanged("ChatMessages");
            }
        }

        public ICommand Chat
        {
            get { return chat; }
            set
            {
                chat = value;
                RaisePropertyChanged("Chat");
            }
        }

        public ICommand Search
        {
            get { return search; }
            set
            {
                search = value;
                RaisePropertyChanged("Search");
            }
        }

        public ICommand OpenExternal
        {
            get { return openExternal; }
            set
            {
                openExternal = value;
                RaisePropertyChanged("OpenExternal");
            }
        }

        public ICommand Compare
        {
            get { return compare; }
            set
            {
                compare = value;
                RaisePropertyChanged("Compare");
            }
        }

        public ICommand UserInfo
        {
            get { return userinfo; }
            set
            {
                userinfo = value;
                RaisePropertyChanged("UserInfo");
            }
        }

        public ICommand Closing
        {
            get { return closing; }
            set
            {
                closing = value;
                RaisePropertyChanged("Closing");
            }
        }

        public ICommand ViewQueue
        {
            get { return viewQueue; }
            set
            {
                viewQueue = value;
                RaisePropertyChanged("ViewQueue");
            }
        }

        public ICommand EditShares
        {
            get { return editShares; }
            set
            {
                editShares = value;
                RaisePropertyChanged("EditShares");
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

        public ICommand Settings
        {
            get { return settings; }
            set
            {
                settings = value;
                RaisePropertyChanged("Settings");
            }
        }

        public ICommand ViewShare
        {
            get { return viewShare; }
            set
            {
                viewShare = value;
                RaisePropertyChanged("ViewShare");
            }
        }

        public object SelectedClient
        {
            get { return selectedClient; }
            set
            {
                selectedClient = value;
                RaisePropertyChanged("SelectedClient");
            }
        }

        public Dispatcher Dispatcher
        {
            get { return ViewCore.Dispatcher; }
        }

        public void DoFlashWindow()
        {
            ViewCore.Flash();
        }


        public void Show()
        {
            visible = true;
            ViewCore.Show();
        }

        public void Close()
        {
            visible = false;
            AllowClose = true;
            ViewCore.Close();
        }
    }
}