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
using Fap.Application.ViewModels;
using System.Waf.Applications;
using Fap.Domain;
using Fap.Domain.Entity;
using Fap.Foundation;
using Fap.Domain.Services;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Resources;
using System.Net;
using System.Threading;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using Autofac;
using Fap.Network.Entity;
using System.Net.Sockets;
using Fap.Domain.Controllers;
using System.Diagnostics;
using Fap.Foundation.Services;
using ContinuousLinq;
using NLog;
using Fap.Application.Views;
using System.Waf.Applications.Services;
using System.Reflection;

namespace Fap.Application.Controllers
{
    public class ApplicationController : AsyncControllerBase
    {
        private readonly IContainer container;
        private readonly PopupWindowController popupController;
        private readonly QueryViewModel browser;
        private readonly LANPeerConnectionService peerController;
        private readonly SharesController shareController;
        private readonly Logger logger;
        private readonly WatchdogController watchdog;
        private readonly ClientListenerService server;
        private readonly ConversationController chatController;
        private readonly DownloadController downloadController;
        private readonly LogService logReceiver;
        private readonly ShareInfoService shareInfo;
        private readonly InterfaceController interfaceController;

        private MainWindowViewModel mainWindowModel;
        private Model model;
        private TrayIconViewModel trayIcon;

        public ApplicationController(IContainer container)
        {
            if (container == null) { throw new ArgumentNullException("container"); }
            this.container = container;
            peerController = container.Resolve<LANPeerConnectionService>();
            shareController = container.Resolve<SharesController>();
            logger = LogManager.GetLogger("faplog");
            model = container.Resolve<Model>();
            popupController = container.Resolve<PopupWindowController>();
            browser = container.Resolve<QueryViewModel>();
            watchdog = container.Resolve<WatchdogController>();
            trayIcon = container.Resolve<TrayIconViewModel>();
            server = container.Resolve<ClientListenerService>();
            chatController = container.Resolve<ConversationController>();
            downloadController = container.Resolve<DownloadController>();
            logReceiver = container.Resolve<LogService>();
            shareInfo = container.Resolve<ShareInfoService>();
            interfaceController = container.Resolve<InterfaceController>();
        }

        public bool Initalise()
        {
            try
            {
                model.Load();
            }
            catch
            {
                logger.Warn("Failed to read config file, using defaults");
            }

            model.IPAddress = interfaceController.CheckAddress(model.IPAddress);
            //User chose to quit rather than select an interface =s
            if (string.IsNullOrEmpty(model.IPAddress))
                return false;

            logger.Info("Using local address: {0}", model.IPAddress);
            //If there is no avatar set then put in the default
            if (string.IsNullOrEmpty(model.Avatar))
            {

                var stream = System.Windows.Application.GetResourceStream(new Uri("Images/Default_Avatar.png", UriKind.Relative)).Stream;
                byte[] img = new byte[stream.Length];
                stream.Read(img, 0, (int)stream.Length);
                model.Avatar = Convert.ToBase64String(img);
                model.Save();
            }
            //Set default nick
            if (string.IsNullOrEmpty(model.Nickname))
            {
                model.Nickname = Dns.GetHostName();
            }

            //Set default limits
            if (model.MaxDownloads == 0)
                model.MaxDownloads = 4;
            if (model.MaxDownloadsPerUser == 0)
                model.MaxDownloadsPerUser = 2;
            if (model.MaxUploads == 0)
                model.MaxUploads = 5;
            if (model.MaxUploadsPerUser == 0)
                model.MaxUploadsPerUser = 2;

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

            if (string.IsNullOrEmpty(model.LocalNodeID))
            {
                model.LocalNodeID = IDService.CreateID();
            }

            shareInfo.Load();

            //Load download queue
            try
            {
                model.DownloadQueue = new DownloadQueue();
                model.DownloadQueue.Load();
            }
            catch
            {
                logger.Warn("Failed to read download queue");
            }

            if (model.MaxOverlordPeers == 0)
                model.MaxOverlordPeers = 50;

            //Add local network manually
            Fap.Network.Entity.Network network = new Network.Entity.Network();
            network.ID = "LOCAL";
            network.Name = "Local";
            network.State = Network.ConnectionState.Disconnected;
            model.Networks.Add(network);

            shareController.Initalise();
            chatController.Initalise();
            server.Start();
            peerController.Start(network);
            model.Peers.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Peers_CollectionChanged);
            ThreadPool.QueueUserWorkItem(new WaitCallback(MainWindowUpdater));
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
            model.Node.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Node_PropertyChanged);

            logger.Info("Local node ID is {0}", model.Node.ID);
            return true;
        }


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
                if (Directory.GetFiles(path).Length==0)
                {
                    try
                    {
                        Directory.Delete(path);
                    }
                    catch { }
                }
            }
        }

        void Node_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(null!=mainWindowModel)
            {
            switch (e.PropertyName)
            {
                case "Nickname":
                mainWindowModel.Nickname = model.Nickname;
                break;
                case "Description":
                mainWindowModel.Description = model.Description;
                break;
                case "Avatar":
                mainWindowModel.Avatar = model.Avatar;
                break;
            }
        }
        }

        private void Peers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
           
        }

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
                    mainWindowModel.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                       new Action(
                        delegate()
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append("Stats: ");

                            int count = model.Peers.Where(n => n.NodeType != ClientType.Overlord).Count();
                            sb.Append(count);
                            if (count == 1)
                                sb.Append(" client sharing ");
                            else
                                sb.Append(" clients sharing ");
                            sb.Append(Utility.FormatBytes(model.Peers.Select(p => p.ShareSize).Sum()));
                            sb.Append(" in ");
                            sb.Append(Utility.ConverNumberToText(model.Peers.Select(p => p.FileCount).Sum()));
                            sb.Append(" files.");
                            string text = sb.ToString();

                            if (null != mainWindowModel)
                            {
                                mainWindowModel.CurrentNetworkStatus = text;

                                StringBuilder sbs = new StringBuilder();
                                sbs.Append("Status: ");
                                sbs.Append(mainWindowModel.CurrentNetwork.State);
                                sbs.Append(" as ");
                                sbs.Append(model.Nickname);
                                if (peerController.IsOverlord)
                                    sbs.Append(" (Server host)");
                                mainWindowModel.NodeStatus = sbs.ToString();
                            }

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
                            if (model.Node.DownloadSpeed == 0)
                            {
                                mainWindowModel.LocalStats = "Local: No transfers";
                            }
                            else
                            {
                                StringBuilder ls = new StringBuilder();
                                ls.Append("Local RX/TX: ");
                                ls.Append(Utility.ConvertNumberToTextSpeed(model.Node.DownloadSpeed));
                                ls.Append(" / ");
                                ls.Append(Utility.ConvertNumberToTextSpeed(model.Node.UploadSpeed));
                                mainWindowModel.LocalStats = ls.ToString();
                            }
                            //Global stats

                            long upload = model.Peers.Select(s => s.DownloadSpeed).Sum();
                            long download = model.Peers.Select(s => s.UploadSpeed).Sum();


                            if (upload == 0 && download == 0)
                            {
                                mainWindowModel.GlobalStats = "Network: No transfers";
                            }
                            else
                            {
                                StringBuilder gs = new StringBuilder();
                                gs.Append("Global RX/TX: ");
                                gs.Append(Utility.ConvertNumberToTextSpeed(download));
                                gs.Append(" / ");
                                gs.Append(Utility.ConvertNumberToTextSpeed(upload));
                                mainWindowModel.GlobalStats = gs.ToString();

                            }

                            foreach (var line in logReceiver.GetLines())
                                model.Messages.Add(line);
                        }
                       ));
                }
                Thread.Sleep(333);
            }
        }

        private void showUserInfo(object obj)
        {
            Node n = obj as Node;
            if (null != n)
            {
                var o = container.Resolve<UserInfoViewModel>();
                o.Node = n;
                popupController.AddWindow(o.View, "User info (" + n.Nickname + ")");
            }
        }


        private void ShowMainWindow()
        {
            if (null == mainWindowModel)
            {
                mainWindowModel = container.Resolve<MainWindowViewModel>();
                mainWindowModel.CurrentNetwork = model.Networks.Where(n=>n.ID == "LOCAL").First();
                mainWindowModel.WindowTitle = "FAP Alpha 4";
                mainWindowModel.SendChatMessage = new DelegateCommand(sendChatMessage);
                mainWindowModel.ViewShare = new DelegateCommand(viewShare);
                mainWindowModel.EditShares = new DelegateCommand(EditShares);
                mainWindowModel.ChangeAvatar = new DelegateCommand(ChangeAvatar);
                mainWindowModel.Settings = new DelegateCommand(Settings);
                mainWindowModel.ViewQueue = new DelegateCommand(ViewQueue);
                mainWindowModel.Closing = new DelegateCommand(MainWindowClosing);
                mainWindowModel.OpenExternal = new DelegateCommand(OpenExternal);
                mainWindowModel.Compare = new DelegateCommand(Compare);
                mainWindowModel.Chat = new DelegateCommand(Chat);
                mainWindowModel.UserInfo = new DelegateCommand(showUserInfo);
                mainWindowModel.ChangePeerSort = new DelegateCommand(ChangePeerSort);
                mainWindowModel.Avatar = model.Avatar;
                mainWindowModel.Nickname = model.Nickname;
                mainWindowModel.Description = model.Description;
                mainWindowModel.Sessions = model.TransferSessions;
                mainWindowModel.ChatMessages = model.Messages;
                mainWindowModel.Node = model.Node;
                mainWindowModel.Model = model;
                mainWindowModel.PeerSortType = model.PeerSortType;
                BindPeerList();
                ClientList_CollectionChanged(null, null);
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

        private void ChangePeerSort(object o)
        {
            mainWindowModel.PeerSortType = (PeerSortType)o;
            model.PeerSortType = mainWindowModel.PeerSortType;
            BindPeerList();
        }

        private void BindPeerList()
        {
            if (null != mainWindowModel)
            {
                switch (mainWindowModel.PeerSortType)
                {
                    case PeerSortType.Address:
                        mainWindowModel.Peers = model.Peers.Where(s=>s.NodeType!=ClientType.Overlord).Select(s => s).OrderBy(s=>s.Host);
                        break;
                    case PeerSortType.Name:
                        mainWindowModel.Peers = model.Peers.Where(s => s.NodeType != ClientType.Overlord).Select(s => s).OrderBy(s => s.Nickname);
                        break;
                    case PeerSortType.Size:
                        mainWindowModel.Peers = model.Peers.Where(s => s.NodeType != ClientType.Overlord).Select(s => s).OrderByDescending(s => s.ShareSize);
                        break;
                    case PeerSortType.Type:
                        mainWindowModel.Peers = model.Peers.Select(s => s).OrderBy(s => s.NodeType);
                        break;
                }
            }
        }


        private void Chat(object o)
        {
            Node peer = o as Node;
            if (null != peer)
            {
                chatController.CreateConversation(peer);
            }
        }

        private void Compare()
        {
            CompareController cc = container.Resolve<CompareController>();
            var vm = cc.Initalise();
            popupController.AddWindow(vm.View, "Compare");
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

        private void Exit()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(ShutDownAsync));
        }

        void ClientList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (null != mainWindowModel)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Status: Connected to ");
                sb.Append(model.Peers.Where(p => p.NodeType != ClientType.Overlord).Count());
                sb.Append(" clients sharing a total of ");

                long total = 0;
                foreach (var client in model.Peers.ToList())
                {
                    total += client.ShareSize;
                }
                sb.Append(Utility.FormatBytes(total));
                mainWindowModel.NetworkStatus = sb.ToString();
            }
        }


        private void ViewQueue()
        {
            DownloadQueueController dqc = container.Resolve<DownloadQueueController>();
            dqc.Initalise();
            popupController.AddWindow(dqc.ViewModel.View, "Download Queue");
        }

        private void Settings()
        {

            var o = container.Resolve<SettingsViewModel>();
            o.Model = model;
            o.EditDownloadDir = new DelegateCommand(SettingsEditDownloadDir);
            o.ChangeAvatar = new DelegateCommand(ChangeAvatar);
            o.ResetInterface = new DelegateCommand(ResetInterface);
            popupController.AddWindow(o.View, "Settings");
        }

        private void ResetInterface()
        {
            model.IPAddress = null;
            model.Save();
            container.Resolve<IMessageService>().ShowWarning("Interface selection reset.  FAP will now restart.");
            Process notePad = new Process();

            notePad.StartInfo.FileName = Assembly.GetEntryAssembly().CodeBase;
            notePad.StartInfo.Arguments = "WAIT";
            notePad.Start();
            Exit();

        }

        private void SettingsEditDownloadDir()
        {
             string folder = string.Empty;
             if (browser.SelectFolder(out folder))
             {
                 model.DownloadFolder = folder;
                 model.IncompleteFolder = folder + "\\Incomplete";
             }
        }



        private void ChangeAvatar()
        {
            string path = string.Empty;

            if (browser.SelectFile(out path))
            {
                try
                {
                    MemoryStream ms = new MemoryStream();
                    FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                    ms.SetLength(stream.Length);
                    stream.Read(ms.GetBuffer(), 0, (int)stream.Length);

                    ms.Flush();
                    stream.Close();

                    //byte[] img = new byte[ms.Length];
                    //  ms.Read(img,0,(int)ms.Length);

                    //Resize
                    Bitmap bitmap = new Bitmap(ms);

                    Image thumbnail = ResizeImage(bitmap, 100, 100);

                    ms = new MemoryStream();
                    thumbnail.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    model.Avatar = Convert.ToBase64String(ms.ToArray());
                }
                catch
                {

                }
            }
        }


        private Image ResizeImage(Bitmap FullsizeImage, int NewWidth, int MaxHeight)
        {
            // Prevent using images internal thumbnail
            FullsizeImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
            FullsizeImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);

            if (FullsizeImage.Width <= NewWidth)
            {
                NewWidth = FullsizeImage.Width;
            }

            int NewHeight = FullsizeImage.Height * NewWidth / FullsizeImage.Width;
            if (NewHeight > MaxHeight)
            {
                // Resize with height instead
                NewWidth = FullsizeImage.Width * MaxHeight / FullsizeImage.Height;
                NewHeight = MaxHeight;
            }

            System.Drawing.Image NewImage = FullsizeImage.GetThumbnailImage(NewWidth, NewHeight, null, IntPtr.Zero);

            // Clear handle to original file so that we can overwrite it if necessary
            FullsizeImage.Dispose();

            // Save resized picture
            return NewImage;
        }

        private void EditShares()
        {
            popupController.AddWindow(shareController.ViewModel.View,"Edit shares");
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


        private void sendChatMessage()
        {
            if (string.Equals(mainWindowModel.CurrentChatMessage, "/debug", StringComparison.InvariantCultureIgnoreCase))
            {
                logReceiver.MoreDebug = !logReceiver.MoreDebug;
                logger.Info("Debug mode is " + (logReceiver.MoreDebug ? "Activated" : "Deactivated"));
            }
            else
            {
                peerController.SendChatMessage(mainWindowModel.CurrentChatMessage);
               
            }
            mainWindowModel.CurrentChatMessage = string.Empty;
        }


        public void Run()
        {
            watchdog.Run();
            ShowMainWindow();
            downloadController.Start();
        }

        public void ShutDownAsync(object param)
        {

            model.Save();
            model.DownloadQueue.Save();

            //Do not do this on the dispatcher thread!
            peerController.Stop();

            //Try to kill off existing connections (Dangerous)
            foreach (var session in model.Sessions.ToList())
            {
                try
                {
                    if (session.Socket != null && session.Socket.Connected)
                    {
                        session.Socket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                        session.Socket.Close();
                    }
                }
                catch { }
            }
            long start = Environment.TickCount;
            while (model.Sessions.ToList().Where(s => !s.IsUpload && s.InUse).Count() > 0)
            {
                //Wait for sessions to close
                Thread.Sleep(20);

                if (Environment.TickCount - start > 10000)
                    break;
            }

            //Kill the other foreground thread
            watchdog.Stop();

            //Kill UI
            SafeObservableStatic.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
            new Action(
             delegate()
             {
                 if (null != mainWindowModel)
                 {
                     popupController.Close();
                     chatController.Close();
                     mainWindowModel.Close();
                     trayIcon.Dispose();
                     System.Windows.Application.Current.Shutdown(0);
                 }
             }
            ));
        }
    }
}
