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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Waf.Applications;
using System.Windows.Threading;
using Autofac;
using FAP.Application.Controllers;
using FAP.Application.ViewModel;
using FAP.Application.ViewModels;
using FAP.Domain;
using FAP.Domain.Entities;
using FAP.Domain.Services;
using FAP.Domain.Verbs;
using Fap.Foundation;
using Fap.Foundation.RegistryServices;
using Fap.Foundation.Services;
using NLog;
using IContainer = Autofac.IContainer;

namespace FAP.Application
{
    public class ApplicationCore
    {
        private readonly ConnectionController connectionController;
        private readonly IContainer container;
        private readonly InterfaceController interfaceController;
        private readonly LogService logService;

        private readonly Model model;
        private readonly OverlordManagerService overlordManagerService;
        private readonly RegisterProtocolService registerProtocolService;
        private readonly SingleInstanceService singleInstanceService;
        private readonly UpdateCheckerService updateChecker;
        private ListenerService client;
        private CompareController compareController;
        private ConversationController conversationController;
        private DownloadQueueController downloadQueueController;
        private MainWindowViewModel mainWindowModel;
        private PopupWindowController popupController;
        private SearchController searchController;
        private SettingsController settingsController;
        private SharesController shareController;
        private ShareInfoService shareInfo;
        private TrayIconViewModel trayIcon;
        private WatchdogController watchdogController;

        public ApplicationCore(IContainer c)
        {
            container = c;
            model = container.Resolve<Model>();
            logService = container.Resolve<LogService>();

            connectionController = c.Resolve<ConnectionController>();
            //Don't send two request went doing a post..
            ServicePointManager.Expect100Continue = false;
            //Don't limit connections to a single node - 100 I think is the upper limit.
            ServicePointManager.DefaultConnectionLimit = 100;
            //System.Net.ServicePointManager.MaxServicePointIdleTime = 20000000;
            updateChecker = container.Resolve<UpdateCheckerService>();
            interfaceController = container.Resolve<InterfaceController>();
            overlordManagerService = container.Resolve<OverlordManagerService>();
            singleInstanceService = new SingleInstanceService("FAP");
            registerProtocolService = new RegisterProtocolService();
        }

        public bool CheckSingleInstance()
        {
            return singleInstanceService.GetLock();
        }

        public void Exit()
        {
            ThreadPool.QueueUserWorkItem(ShutDownAsync);
        }

        public void ShutDownAsync(object param)
        {
            model.Save();
            model.DownloadQueue.Save();
            connectionController.Exit();
            watchdogController.Stop();
            //Kill local overlord if running
            overlordManagerService.Stop();
            if (null != client)
                client.Stop();

            //Kill UI
            SafeObservableStatic.Dispatcher.Invoke(DispatcherPriority.Normal,
                                                   new Action(
                                                       delegate
                                                           {
                                                               if (null != mainWindowModel)
                                                               {
                                                                   popupController.Close();
                                                                   mainWindowModel.Close();
                                                                   trayIcon.Dispose();

                                                                   model.GetShutdownLock();
                                                                   singleInstanceService.Dispose();
                                                                   System.Windows.Application.Current.Shutdown(0);
                                                               }
                                                           }
                                                       ));
        }

        public void StartGUI(bool showWindow)
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
            if (showWindow)
                ShowMainWindow();
            ThreadPool.QueueUserWorkItem(MainWindowUpdater);
        }

        public bool Load(bool server)
        {
            model.Load();
            model.LocalNode.Host = interfaceController.CheckAddress(model.LocalNode.Host);
            //User chose to quit rather than select an interface =s
            if (string.IsNullOrEmpty(model.LocalNode.Host))
                return false;

            model.CheckSetDefaults();

            updateChecker.Run();

            //Immediatly send model upates
            model.LocalNode.PropertyChanged += LocalNode_PropertyChanged;

            if (!server)
            {
                //Register FAP protocol
                string location = Assembly.GetCallingAssembly().Location;
                registerProtocolService.Register("fap", location, "-url \"%1\"");

                //Delete any empty folders in the incomplete folder
                RemoveEmptyFolders(model.IncompleteFolder);
                LogManager.GetLogger("faplog").Debug("Client started with ID: {0}", model.LocalNode.ID);

                model.DownloadQueue.Load();

                shareInfo = container.Resolve<ShareInfoService>();
                shareInfo.Load();

                shareController = new SharesController(container, model);
                shareController.Initalise();
                popupController = container.Resolve<PopupWindowController>();
                conversationController = (ConversationController) container.Resolve<IConversationController>();
                watchdogController = container.Resolve<WatchdogController>();
                watchdogController.Start();

                if (!model.DisplayedHelp)
                    ShowQuickStart();
            }
            return true;
        }

        private void LocalNode_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //Update immeadiatly on user input to give the app a nicer feel
            if (e.PropertyName == "Nickname" || e.PropertyName == "Description" || e.PropertyName == "Avatar")
                ThreadPool.QueueUserWorkItem(updateModelAsync);
        }

        private void updateModelAsync(object o)
        {
            connectionController.CheckModelChanges();
        }

        public void ShowQuickStart()
        {
            model.DisplayedHelp = true;
            var helpWindow = container.Resolve<WebViewModel>();

            if (null != helpWindow)
            {
                string path = Path.GetDirectoryName(Assembly.GetCallingAssembly().CodeBase);
                helpWindow.Location = path + "\\Web.Help\\help.html";
                popupController.AddWindow(helpWindow.View, "Quick Start");
            }
        }

        public void AddDownloadUrlWhenConnected(string url)
        {
            ThreadPool.QueueUserWorkItem(AddDownloadAsync, url);
        }

        private void AddDownloadAsync(object url)
        {
            while (model.Network.State != ConnectionState.Connected)
                Thread.Sleep(250);
            Thread.Sleep(2000);
            model.AddDownloadURL(url as string);
        }

        public void StartClient()
        {
            client = new ListenerService(container, false);
            client.Start(model.LocalNode.Port);
            connectionController.Start();
        }

        public void StartOverlordServer()
        {
            model.IsDedicated = true;
            overlordManagerService.Start();
        }

        private void ShowMainWindow()
        {
            if (null == mainWindowModel)
            {
                mainWindowModel = container.Resolve<MainWindowViewModel>();

                mainWindowModel.WindowTitle = Model.AppVersion;
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
                mainWindowModel.Sessions = model.UITransferSessions;
                mainWindowModel.Node = model.LocalNode;
                mainWindowModel.Model = model;
                mainWindowModel.Search = new DelegateCommand(Search);

                var f = new SafeFilteredObservingCollection<Node>(new SafeObservingCollection<Node>(model.Network.Nodes));
                f.Filter = s => s.NodeType != ClientType.Overlord;
                mainWindowModel.Peers = f;
                mainWindowModel.ChatMessages = new SafeObservingCollection<string>(model.Messages);
            }
            else
            {
                if (mainWindowModel.Visible)
                    mainWindowModel.DoFlashWindow();
                else
                    mainWindowModel.Show();
            }

            mainWindowModel.Show();
        }

        private void RemoveEmptyFolders(string path)
        {
            string[] folders = Directory.GetDirectories(path);
            foreach (string folder in folders)
                iRemoveEmptyFolders(folder);
        }

        private void iRemoveEmptyFolders(string path)
        {
            string[] folders = Directory.GetDirectories(path);
            foreach (string folder in folders)
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
                    catch
                    {
                    }
                }
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
            var n = obj as Node;
            if (null != n)
            {
                var o = container.Resolve<UserInfoViewModel>();
                o.Node = n;
                // popupController.AddWindow(o.View, "User info (" + n.Nickname + ")");
            }
        }

        private void Chat(object o)
        {
            var peer = o as Node;
            if (null != peer)
                conversationController.CreateConversation(peer);
        }

        private void Compare()
        {
            if (null == compareController)
            {
                compareController = container.Resolve<CompareController>();
                CompareViewModel vm = compareController.Initalise();
            }
            popupController.AddWindow(compareController.ViewModel.View, "Compare");
        }

        private void OpenExternal(object o)
        {
            var url = o as string;
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
            popupController.AddWindow(downloadQueueController.ViewModel.View, "Transfer Info");
        }

        private void sendChatMessage()
        {
            switch (mainWindowModel.CurrentChatMessage)
            {
                case "/trace":
                    logService.Filter = LogLevel.Trace;
                    model.Messages.Add("Debug level set to: Trace");
                    break;
                case "/debug":
                    logService.Filter = LogLevel.Debug;
                    model.Messages.Add("Debug level set to: Debug");
                    break;
                case "/info":
                    logService.Filter = LogLevel.Debug;
                    model.Messages.Add("Debug level set to: Info");
                    break;
                case "/warn":
                    logService.Filter = LogLevel.Debug;
                    model.Messages.Add("Debug level set to: Warning");
                    break;
                case "/error":
                    logService.Filter = LogLevel.Debug;
                    model.Messages.Add("Debug level set to: Error");
                    break;
                case "/fatal":
                    logService.Filter = LogLevel.Debug;
                    model.Messages.Add("Debug level set to: Fatal");
                    break;
                case "/off":
                    logService.Filter = LogLevel.Off;
                    model.Messages.Add("Debug level set to: Fatal");
                    break;
                case "/disconnect":
                    model.Messages.Add("Disconnecting from current overlord..");
                    connectionController.Disconnect();
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
            var rc = o as Node;
            if (null != rc)
            {
                var bc = container.Resolve<BrowserController>(new NamedParameter("client", rc));
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
                MainWindowViewModel window = mainWindowModel;
                if (null != window)
                {
                    window.Dispatcher.Invoke(DispatcherPriority.Background,
                                             new Action(
                                                 delegate
                                                     {
                                                         if (null != mainWindowModel)
                                                         {
                                                             //Update status line
                                                             {
                                                                 var sbs = new StringBuilder();
                                                                 sbs.Append("Status: ");
                                                                 sbs.Append(model.Network.State);
                                                                 sbs.Append(" as ");
                                                                 sbs.Append(model.Nickname);

                                                                 if (overlordManagerService.IsOverlordActive)
                                                                     sbs.Append(" (Overlord host)");

                                                                 if (model.Network.State == ConnectionState.Connected)
                                                                 {
                                                                     if (model.Network.Overlord.Host ==
                                                                         model.LocalNode.Host)
                                                                     {
                                                                         sbs.Append(" on yourself.");
                                                                     }
                                                                     else
                                                                     {
                                                                         sbs.Append(" on ");
                                                                         Node search =
                                                                             model.Network.Nodes.ToList().Where(
                                                                                 n =>
                                                                                 n.Host == model.Network.Overlord.Host &&
                                                                                 n.NodeType == ClientType.Client).
                                                                                 FirstOrDefault();
                                                                         if (null == search)
                                                                             sbs.Append(model.Network.Overlord.Host);
                                                                         else
                                                                             sbs.Append(search.Nickname);
                                                                     }
                                                                 }

                                                                 window.NodeStatus = sbs.ToString();
                                                                 sbs.Length = 0;
                                                                 sbs = null;
                                                             }

                                                             //Update stats line
                                                             {
                                                                 var sb = new StringBuilder();
                                                                 sb.Append("Stats: ");

                                                                 int count =
                                                                     model.Network.Nodes.Where(
                                                                         n => n.NodeType != ClientType.Overlord).Count();
                                                                 sb.Append(count);
                                                                 if (count == 1)
                                                                     sb.Append(" client sharing ");
                                                                 else
                                                                     sb.Append(" clients sharing ");
                                                                 sb.Append(
                                                                     Utility.FormatBytes(
                                                                         model.Network.Nodes.Select(p => p.ShareSize).
                                                                             Sum()));
                                                                 sb.Append(" in ");
                                                                 sb.Append(
                                                                     Utility.ConverNumberToText(
                                                                         model.Network.Nodes.Select(p => p.FileCount).
                                                                             Sum()));
                                                                 sb.Append(" files.");

                                                                 window.CurrentNetworkStatus = sb.ToString();

                                                                 sb.Length = 0;
                                                                 sb = null;
                                                             }

                                                             // Update transfers
                                                             foreach (TransferSession xfer in mainWindowModel.Sessions)
                                                             {
                                                                 if (xfer.Worker.Length == 0)
                                                                     xfer.Percent = 0;
                                                                 else
                                                                     xfer.Percent =
                                                                         (int)
                                                                         (((double) xfer.Worker.Position/
                                                                           xfer.Worker.Length)*100);
                                                                 xfer.Size = xfer.Worker.Length;
                                                                 if (!xfer.Worker.IsComplete)
                                                                     xfer.Speed = xfer.Worker.Speed;
                                                                 xfer.Status = xfer.Worker.Status;
                                                             }

                                                             //Local stats
                                                             {
                                                                 if (model.LocalNode.DownloadSpeed == 0 &&
                                                                     model.LocalNode.UploadSpeed == 0)
                                                                 {
                                                                     string t = "Local: No transfers";
                                                                     if (mainWindowModel.LocalStats != t)
                                                                         mainWindowModel.LocalStats = t;
                                                                 }
                                                                 else
                                                                 {
                                                                     var ls = new StringBuilder();
                                                                     ls.Append("Local RX/TX: ");
                                                                     ls.Append(
                                                                         Utility.ConvertNumberToTextSpeed(
                                                                             model.LocalNode.DownloadSpeed));
                                                                     ls.Append(" / ");
                                                                     ls.Append(
                                                                         Utility.ConvertNumberToTextSpeed(
                                                                             model.LocalNode.UploadSpeed));

                                                                     window.LocalStats = ls.ToString();

                                                                     ls.Length = 0;
                                                                     ls = null;
                                                                 }
                                                             }

                                                             //Global stats
                                                             {
                                                                 long upload =
                                                                     model.Network.Nodes.ToList().Select(
                                                                         s => s.DownloadSpeed).Sum();
                                                                 long download =
                                                                     model.Network.Nodes.ToList().Select(
                                                                         s => s.UploadSpeed).Sum();


                                                                 if (upload == 0 && download == 0)
                                                                 {
                                                                     string t = "Network: No transfers";
                                                                     if (mainWindowModel.GlobalStats != t)
                                                                         mainWindowModel.GlobalStats = t;
                                                                 }
                                                                 else
                                                                 {
                                                                     var gs = new StringBuilder();
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
    }
}