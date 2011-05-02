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
using Autofac;
using FAP.Domain.Services;
using FAP.Domain.Entities;
using FAP.Domain;
using FAP.Domain.Verbs;
using FAP.Application.ViewModels;
using System.Waf.Applications;
using System.Waf.Applications.Services;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Controls;
using System.Drawing;
using Fap.Foundation;
using Fap.Foundation.Services;
using System.Threading;
using System.Net;
using FAP.Application.Controllers;
using NLog;
using System.IO;

namespace FAP.Application
{
    public class ApplicationCore
    {
        private IContainer container;

        private SharesController shareController;
        private PopupWindowController popupController;
        private ConnectionController connectionController;
        private CompareController compareController;
        private SettingsController settingsController;
        private DownloadQueueController downloadQueueController;
        private SearchController searchController;
        private ConversationController conversationController;
        private InterfaceController interfaceController;
        private WatchdogController watchdogController;

        private ListenerService client;
        private ShareInfoService shareInfo;
        private ListenerService server;
       
        private Model model;
        private MainWindowViewModel mainWindowModel;
        private TrayIconViewModel trayIcon;

        public ApplicationCore(IContainer c)
        {
            container = c;
            connectionController = c.Resolve<ConnectionController>();
            //Don't send two request went doing a post..
            System.Net.ServicePointManager.Expect100Continue = false;
            //Don't limit connections to a single node - 100 I think is the upper limit.
            System.Net.ServicePointManager.DefaultConnectionLimit = 100;
            model = container.Resolve<Model>();
            interfaceController = container.Resolve<InterfaceController>();
        }

        public void Exit()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(ShutDownAsync));
        }

        public void ShutDownAsync(object param)
        {
            model.Save();
            model.DownloadQueue.Save();
            connectionController.Exit();

            if (null != server)
                server.Stop();
            if (null != client)
                client.Stop();

            //Kill UI
            SafeObservableStatic.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
            new Action(
             delegate()
             {
                 if (null != mainWindowModel)
                 {
                     popupController.Close();
                     mainWindowModel.Close();
                     trayIcon.Dispose();

                     while (model.BlockShutdown)
                         Thread.Sleep(10);
                     System.Windows.Application.Current.Shutdown(0);
                 }
             }
            ));
        }

        public void StartGUI()
        {
            trayIcon = container.Resolve<TrayIconViewModel>();
            //Tray icon
            trayIcon.Exit = new DelegateCommand(Exit);
            trayIcon.Model = model;
            trayIcon.Open = new DelegateCommand(ShowMainWindow);
            trayIcon.Queue = new DelegateCommand(ViewQueue);
            trayIcon.Settings = new DelegateCommand(Settings);
            trayIcon.Shares = new DelegateCommand(EditShares);
            trayIcon.ViewShare = new DelegateCommand(viewShare);
            trayIcon.Compare = new DelegateCommand(Compare);
            trayIcon.OpenExternal = new DelegateCommand(OpenExternal);
            trayIcon.ShowIcon = true;
            ShowMainWindow();
            ThreadPool.QueueUserWorkItem(new WaitCallback(MainWindowUpdater));
        }

        public bool Load()
        {
            model.Load();

            model.IPAddress = interfaceController.CheckAddress(model.IPAddress);
            //User chose to quit rather than select an interface =s
            if (string.IsNullOrEmpty(model.IPAddress))
                return false;
            if (string.IsNullOrEmpty(model.LocalNode.ID))
                model.LocalNode.ID = IDService.CreateID();

            //Set default download folder
            if (string.IsNullOrEmpty(model.DownloadFolder))
                model.DownloadFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\FAP Downloads";

            if (!Directory.Exists(model.DownloadFolder))
                Directory.CreateDirectory(model.DownloadFolder);

            //Set incomplete download folder
            if (string.IsNullOrEmpty(model.IncompleteFolder) || !Directory.Exists(model.IncompleteFolder))
                model.IncompleteFolder = model.DownloadFolder + "\\Incomplete";

            if (!Directory.Exists(model.DownloadFolder))
                Directory.CreateDirectory(model.DownloadFolder);

            if (!Directory.Exists(model.IncompleteFolder))
                Directory.CreateDirectory(model.IncompleteFolder);

            //Delete any empty folders in the incomplete folder
            RemoveEmptyFolders(model.IncompleteFolder);


            LogManager.GetLogger("faplog").Info("Client started with ID: {0}", model.LocalNode.ID);

            model.DownloadQueue.Load();

            shareInfo = container.Resolve<ShareInfoService>();
            shareInfo.Load();

            shareController = new SharesController(container, model);
            shareController.Initalise();
            popupController = container.Resolve<PopupWindowController>();
            conversationController = (ConversationController)container.Resolve<IConversationController>();
            watchdogController = container.Resolve<WatchdogController>();
            watchdogController.Start();
            return true;
        }

        public void StartClientServer()
        {
            client = new ListenerService(container, false);
            client.Start(30);
            connectionController.Start();
        }

        public void StartOverlordServer()
        {
            server = new ListenerService(container, true);
            server.Start(40);
        }

       

        private void ShowMainWindow()
        {
            if (null == mainWindowModel)
            {
                mainWindowModel = container.Resolve<MainWindowViewModel>();
              
                mainWindowModel.WindowTitle = Model.AppVersion;
#if SINGLE_SERVER
                mainWindowModel.WindowTitle += " Client only mode";
#endif
                mainWindowModel.SendChatMessage = new DelegateCommand(sendChatMessage);
                mainWindowModel.ViewShare = new DelegateCommand(viewShare);
                mainWindowModel.EditShares = new DelegateCommand(EditShares);
                mainWindowModel.Settings = new DelegateCommand(Settings);
                mainWindowModel.ViewQueue = new DelegateCommand(ViewQueue);
                mainWindowModel.Closing = new DelegateCommand(MainWindowClosing);
                mainWindowModel.OpenExternal = new DelegateCommand(OpenExternal);
                mainWindowModel.Compare = new DelegateCommand(Compare);
                mainWindowModel.Chat = new DelegateCommand(Chat);
                mainWindowModel.UserInfo = new DelegateCommand(showUserInfo);
                mainWindowModel.Avatar = model.Avatar;
                mainWindowModel.Nickname = model.Nickname;
                mainWindowModel.Description = model.Description;
                mainWindowModel.Sessions = model.TransferSessions;
                mainWindowModel.Node = model.LocalNode;
                mainWindowModel.Model = model;
                mainWindowModel.Search = new DelegateCommand(Search);

                SafeFilteredObservingCollection<Node> f = new SafeFilteredObservingCollection<Node>(new SafeObservingCollection<Node>(model.Network.Nodes));
                //f.Filter = s => s.NodeType != ClientType.Overlord;
                f.Filter = s => true;
                mainWindowModel.Peers = f;
                mainWindowModel.ChatMessages = new SafeObservingCollection<string>(model.Messages);
                mainWindowModel.Show();
            }
            else
            {
                if (mainWindowModel.Visible)
                    mainWindowModel.DoFlashWindow();
                else
                    mainWindowModel.Show();
            }
        }

        #region Main window Commands
        private void Search()
        {
            if (null == searchController)
            {
                searchController = container.Resolve<SearchController>();
                searchController.Initalize();
            }
            popupController.AddWindow(searchController.ViewModel.View, "Search");
        }

        private void showUserInfo(object obj)
        {
            Node n = obj as Node;
            if (null != n)
            {
                var o = container.Resolve<UserInfoViewModel>();
                o.Node = n;
               // popupController.AddWindow(o.View, "User info (" + n.Nickname + ")");
            }
        }

        private void Chat(object o)
        {
            Node peer = o as Node;
            if (null != peer)
                conversationController.CreateConversation(peer);
        }

        private void Compare()
        {
            if (null == compareController)
            {
                compareController = container.Resolve<CompareController>();
                var vm = compareController.Initalise();
            }
            popupController.AddWindow(compareController.ViewModel.View, "Compare");
        }

        private void OpenExternal(object o)
        {
            string url = o as string;
            if (!string.IsNullOrEmpty(url))
                Process.Start(url);
        }

        private void MainWindowClosing()
        {
            mainWindowModel = null;
        }

        private void ViewQueue()
        {
            if (null == downloadQueueController)
            {
                downloadQueueController = container.Resolve<DownloadQueueController>();
                downloadQueueController.Initalise();
            }
            popupController.AddWindow(downloadQueueController.ViewModel.View, "Download Queue");
        }

        private void sendChatMessage()
        {
            switch (mainWindowModel.CurrentChatMessage)
            {
                case "/debug":
                   // logReceiver.MoreDebug = !logReceiver.MoreDebug;
                  //  logger.Info("Debug mode is " + (logReceiver.MoreDebug ? "Activated" : "Deactivated"));
                    break;
                case "/disconnect":
                    //peerController.Disconnect();
                    break;
                default:
                    if (!string.IsNullOrEmpty(mainWindowModel.CurrentChatMessage))
                        connectionController.SendMessage(mainWindowModel.CurrentChatMessage);
                    break;
            }
            mainWindowModel.CurrentChatMessage = string.Empty;
        }

        private void viewShare(object o)
        {
            Node rc = o as Node;
            if (null != rc)
            {
               BrowserController bc = container.Resolve<BrowserController>(new NamedParameter("client", rc));
                bc.Initalise();
               popupController.AddWindow(bc.ViewModel.View, "View share of " + rc.Nickname);
            }
        }

        private void EditShares()
        {
            popupController.AddWindow(shareController.ViewModel.View, "Edit shares");
        }

        private void Settings()
        {
            if (null == settingsController)
            {
                settingsController = container.Resolve<SettingsController>();
                settingsController.Initaize();
            }
            popupController.AddWindow(settingsController.ViewModel.View, "Settings");
        }
        #endregion
        #region Main window UI updater
        /// <summary>
        /// Bulk update the main UI if updated
        /// </summary>
        private void MainWindowUpdater(object o)
        {
            while (true)
            {
                var window = mainWindowModel;
                if (null != window)
                {
                    window.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                       new Action(
                        delegate()
                        {
                            if (null != mainWindowModel)
                            {
                                //Update status line
                                {
                                    StringBuilder sbs = new StringBuilder();
                                    sbs.Append("Status: ");
                                    sbs.Append(model.Network.State);
                                    sbs.Append(" as ");
                                    sbs.Append(model.Nickname);

                                    /*var search = model.Peers.ToList().Where(p => p.ID == mainWindowModel.CurrentNetwork.OverlordID).FirstOrDefault();

                                    if (null != search)
                                    {
                                        sbs.Append(" on ");
                                        sbs.Append(search.Host);
                                        sbs.Append(" ");
                                    }

                                    if (peerController.IsOverlord)
                                        sbs.Append(" (Server host)");
                                    */

                                    window.NodeStatus = sbs.ToString();

                                    sbs.Length = 0;
                                    sbs = null;
                                }

                                //Update stats line
                                {
                                    StringBuilder sb = new StringBuilder();
                                    sb.Append("Stats: ");

                                    int count = model.Network.Nodes.Where(n => n.NodeType != ClientType.Overlord).Count();
                                    sb.Append(count);
                                    if (count == 1)
                                        sb.Append(" client sharing ");
                                    else
                                        sb.Append(" clients sharing ");
                                    sb.Append(Utility.FormatBytes(model.Network.Nodes.Select(p => p.ShareSize).Sum()));
                                    sb.Append(" in ");
                                    sb.Append(Utility.ConverNumberToText(model.Network.Nodes.Select(p => p.FileCount).Sum()));
                                    sb.Append(" files.");

                                    window.CurrentNetworkStatus = sb.ToString();

                                    sb.Length = 0;
                                    sb = null;
                                }

                                // Update transfers
                                foreach (var xfer in mainWindowModel.Sessions)
                                {
                                    if (xfer.Worker.Length == 0)
                                        xfer.Percent = 0;
                                    else
                                        xfer.Percent = (int)(((double)xfer.Worker.Position / xfer.Worker.Length) * 100);
                                    xfer.Size = xfer.Worker.Length;
                                    xfer.Speed = xfer.Worker.Speed;
                                    xfer.Status = xfer.Worker.Status;
                                }

                                //Local stats
                                {
                                    if (model.LocalNode.DownloadSpeed == 0)
                                    {
                                        string t = "Local: No transfers";
                                        if (mainWindowModel.LocalStats != t)
                                            mainWindowModel.LocalStats = t;
                                    }
                                    else
                                    {
                                        StringBuilder ls = new StringBuilder();
                                        ls.Append("Local RX/TX: ");
                                        ls.Append(Utility.ConvertNumberToTextSpeed(model.LocalNode.DownloadSpeed));
                                        ls.Append(" / ");
                                        ls.Append(Utility.ConvertNumberToTextSpeed(model.LocalNode.UploadSpeed));

                                        window.LocalStats = ls.ToString();

                                        ls.Length = 0;
                                        ls = null;
                                    }
                                }

                                //Global stats
                                {

                                    long upload = model.Network.Nodes.Select(s => s.DownloadSpeed).Sum();
                                    long download = model.Network.Nodes.Select(s => s.UploadSpeed).Sum();


                                    if (upload == 0 && download == 0)
                                    {
                                        string t = "Network: No transfers";
                                        if (mainWindowModel.GlobalStats != t)
                                            mainWindowModel.GlobalStats = t;
                                    }
                                    else
                                    {
                                        StringBuilder gs = new StringBuilder();
                                        gs.Append("Global RX/TX: ");
                                        gs.Append(Utility.ConvertNumberToTextSpeed(download));
                                        gs.Append(" / ");
                                        gs.Append(Utility.ConvertNumberToTextSpeed(upload));

                                        window.GlobalStats = gs.ToString();
                                        gs.Length = 0;
                                        gs = null;
                                    }
                                }
                            }
                        }
                       ));
                }
                window = null;
                Thread.Sleep(333);
            }
        }
        #endregion

        private void RemoveEmptyFolders(string path)
        {
            string[] folders = Directory.GetDirectories(path);
            foreach (var folder in folders)
                iRemoveEmptyFolders(folder);

        }
        private void iRemoveEmptyFolders(string path)
        {
            string[] folders = Directory.GetDirectories(path);
            foreach (var folder in folders)
                iRemoveEmptyFolders(folder);
            folders = Directory.GetDirectories(path);

            if (folders.Length == 0)
            {
                if (Directory.GetFiles(path).Length == 0)
                {
                    try
                    {
                        Directory.Delete(path);
                    }
                    catch { }
                }
            }
        }
    }
}
