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

namespace FAP.Application
{
    public class ApplicationCore
    {
        private IContainer container;

        private SharesController shareController;
        private PopupWindowController popupController;

        private Listener client;
        private ShareInfoService shareInfo;
        private Listener server;
        private ConnectionController connectionController;
        private CompareController compareController;
        private SettingsController settingsController;
        private DownloadQueueController downloadQueueController;
        private SearchController searchController;
        private ConversationController conversationController;

        private Model model;

        private MainWindowViewModel mainWindowModel;


        public ApplicationCore(IContainer c)
        {
            container = c;
            connectionController = c.Resolve<ConnectionController>();
            //Don't send two request went doing a post..
            System.Net.ServicePointManager.Expect100Continue = false;
            //Don't limit connections to a single node - 100 I think is the upper limit.
            System.Net.ServicePointManager.DefaultConnectionLimit = 100;
            
            model = container.Resolve<Model>();
            
        }

        public void Exit()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(ShutDownAsync));
        }

        public void ShutDownAsync(object param)
        {
        }

        public void StartGUI()
        {
            ShowMainWindow();
            ThreadPool.QueueUserWorkItem(new WaitCallback(MainWindowUpdater));
        }

        public void Load()
        {
            model.Load();
            model.DownloadQueue.Load();

            model.LocalNode.ID=  IDService.CreateID();
            shareInfo = container.Resolve<ShareInfoService>();
            shareInfo.Load();

            shareController = new SharesController(container, model);
            shareController.Initalise();
            popupController = container.Resolve<PopupWindowController>();
            conversationController = (ConversationController)container.Resolve<IConversationController>();
        }
        
        public void StartClientServer()
        {
            client = new Listener(container, false);
            client.Start(30);
            connectionController.Start();
        }

        public void StartOverlordServer()
        {
            server = new Listener(container, true);
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
                //mainWindowModel.PeerSortType = model.PeerSortType;
                  //mainWindowModel.CurrentNetwork = model.Networks.Where(n => n.ID == "LOCAL").First();

               // FilteredObservableCollection<Node> f = new FilteredObservableCollection<Node>(model.Network.Nodes);
              //  f.Filter = s => s.NodeType != ClientType.Overlord;

                SafeFilteredObservingCollection<Node> f = new SafeFilteredObservingCollection<Node>(new SafeObservingCollection<Node>(model.Network.Nodes));
                f.Filter = s => s.NodeType != ClientType.Overlord;
                mainWindowModel.Peers = f;

                mainWindowModel.ChatMessages = new SafeObservingCollection<string>(model.Messages);

              // // ClientList_CollectionChanged(null, null);
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
               // BrowserController bc = container.Resolve<BrowserController>(new NamedParameter("client", rc));
              //  bc.Initalise();
              //  popupController.AddWindow(bc.ViewModel.View, "View share of " + rc.Nickname);
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
        #region Settings window commands

        private void ResetInterface()
        {
            model.IPAddress = null;
            model.Save();
            container.Resolve<IMessageService>().ShowWarning("Interface selection reset.  FAP will now restart.");
            Process notePad = new Process();

            notePad.StartInfo.FileName = Assembly.GetEntryAssembly().CodeBase;
            notePad.StartInfo.Arguments = "WAIT";
            notePad.Start();
           // Exit();
        }

        private void SettingsEditDownloadDir()
        {
            /* string folder = string.Empty;
             if (browser.SelectFolder(out folder))
             {
                 model.DownloadFolder = folder;
                 model.IncompleteFolder = folder + "\\Incomplete";
             }*/
        }

        private void ChangeAvatar()
        {
            string path = string.Empty;

           /* if (browser.SelectFile(out path))
            {
                try
                {
                    MemoryStream ms = new MemoryStream();
                    FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                    ms.SetLength(stream.Length);
                    stream.Read(ms.GetBuffer(), 0, (int)stream.Length);
                    ms.Flush();
                    stream.Close();
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
            }*/
        }

        private System.Drawing.Image ResizeImage(Bitmap FullsizeImage, int NewWidth, int MaxHeight)
        {
            // Prevent using images internal thumbnail
            FullsizeImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
            FullsizeImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);

            if (FullsizeImage.Width <= NewWidth)
                NewWidth = FullsizeImage.Width;

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
        #endregion


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
    }
}
