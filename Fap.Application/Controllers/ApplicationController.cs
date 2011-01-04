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
using Fap.Domain.Commands;
using Fap.Domain.Entity;
using Fap.Foundation;
using Fap.Foundation.Logging;
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

namespace Fap.Application.Controllers
{
    public class ApplicationController : AsyncControllerBase
    {
        private readonly IContainer container;
        private readonly MainWindowViewModel mainWindowModel;
        private readonly LoggerViewModel loggerModel;
        private readonly PopupWindowController popupController;
        private readonly QueryViewModel browser;

        private PeerController peerController;
        private SharesController shareController;
        private Model model;
        private Node node;
        private Logger logger;
        private DownloadService downloadService;
        private WatchdogController watchdog;
        private ServerService server;
        private TrayIconViewModel trayIcon;

        public ApplicationController(IContainer container)
        {
            if (container == null) { throw new ArgumentNullException("container"); }
            this.container = container;
            mainWindowModel = container.Resolve<MainWindowViewModel>();
            peerController = container.Resolve<PeerController>();
            shareController = container.Resolve<SharesController>();
            loggerModel = container.Resolve<LoggerViewModel>();
            logger = container.Resolve<Logger>();
            model = container.Resolve<Model>();
            popupController = container.Resolve<PopupWindowController>();
            browser = container.Resolve<QueryViewModel>();
            downloadService = container.Resolve<DownloadService>();
            watchdog = container.Resolve<WatchdogController>();
            trayIcon = container.Resolve<TrayIconViewModel>();
            logger.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(logger_CollectionChanged);
            QueueWork(new DelegateCommand(SetupAsync));
        }

        void logger_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (Fap.Foundation.Logging.Log newmsg in e.NewItems)
            {
#if DEBUG
                QueueWork(new DelegateCommand(AsyncAddLog), newmsg);
#else
                if (newmsg.Type == Log.LogType.Error)
                    QueueWork(new DelegateCommand(AsyncAddLog), newmsg);
#endif
            }
        }

        private void AsyncAddLog(object o)
        {
            Fap.Foundation.Logging.Log list = o as Fap.Foundation.Logging.Log;
            mainWindowModel.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
             new Action(
              delegate()
              {
#if DEBUG
                  mainWindowModel.ChatMessages.Add("LOG: " + list.DisplayString);
#endif
              }
             ));
        }


        private void SetupAsync()
        {
           // model.Server = new OldServer(20, container);
           // model.Server.Start(85);
        }


        public void Initalise()
        {
            try
            {
                model.Load();
            }
            catch (Exception e)
            {
                logger.LogException(e);
            }

            //If there is no avatar set then put in the default
            if (model.Avatar.Length == 0)
            {

                var stream = System.Windows.Application.GetResourceStream(new Uri("Images/Default_Avatar.png", UriKind.Relative)).Stream;
                byte[] img = new byte[stream.Length];
                stream.Read(img, 0, (int)stream.Length);
                model.Avatar = img;
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
            if (string.IsNullOrEmpty(model.DownloadFolder) || !Directory.Exists(model.DownloadFolder))
            {
                model.DownloadFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\FAP Downloads";
            }

            if (!Directory.Exists(model.DownloadFolder))
            {
                Directory.CreateDirectory(model.DownloadFolder);
            }

            //Load download queue
            try
            {
                model.DownloadQueue = new DownloadQueue();
                model.DownloadQueue.Load();
            }
            catch (Exception e)
            {
                logger.LogException(e);
            }

            mainWindowModel.WindowTitle = "FAP - 'Overkill' edition - Release " + Model.NetCodeVersion + "." + Model.ClientVersion ;
            mainWindowModel.SendChatMessage = new DelegateCommand(sendChatMessage);
            mainWindowModel.ViewShare = new DelegateCommand(viewShare);
            mainWindowModel.EditShares = new DelegateCommand(EditShares);
            mainWindowModel.ChangeAvatar = new DelegateCommand(ChangeAvatar);
            mainWindowModel.Settings = new DelegateCommand(Settings);
            mainWindowModel.ViewQueue = new DelegateCommand(ViewQueue);
            mainWindowModel.Closing = new DelegateCommand(Closing);
            //Logger
            loggerModel.Logs = logger.Logs;
            mainWindowModel.LogView = loggerModel.View;
            mainWindowModel.Avatar = model.Avatar;
            mainWindowModel.Nickname = model.Nickname;
            mainWindowModel.Description = model.Description;
            mainWindowModel.Sessions = model.Sessions;
            mainWindowModel.ChatMessages = model.Messages;
            model.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(model_PropertyChanged);
            shareController.Initalise();
            node = new Node();
            server = container.Resolve<ServerService>();
            server.Start();
            peerController.Start();
            mainWindowModel.Peers = model.Peers;
            mainWindowModel.Peers.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ClientList_CollectionChanged);
            mainWindowModel.Node = node;
            ClientList_CollectionChanged(null, null);

            //Tray icon
            trayIcon.Exit = new DelegateCommand(Closing);
            trayIcon.Model = model;
            trayIcon.Open = new DelegateCommand(ShowMainWindow);
            trayIcon.Queue = new DelegateCommand(ViewQueue);
            trayIcon.Settings = new DelegateCommand(Settings);
            trayIcon.Shares = new DelegateCommand(EditShares);
            trayIcon.ViewShare = new DelegateCommand(viewShare);
            trayIcon.ShowIcon = true;
        }


        private void ShowMainWindow()
        {

        }

        private IPAddress GetLocalAddress()
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            IPAddress a = localIPs[0];

            foreach (var ip in localIPs)
            {
                if (!IPAddress.IsLoopback(ip) && ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    a = ip;
                    break;
                }
            }
            return a;
        }

        private void Closing()
        {
            QueueWork(new DelegateCommand(ShutDown));
        }



        void ClientList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Status: Connected to ");
            sb.Append(mainWindowModel.Peers.Where(p=>p.NodeType!= ClientType.Overlord).Count());
            sb.Append(" clients sharing a total of ");

            long total =0;
            foreach(var client in mainWindowModel.Peers.ToList())
            {
                total+=client.ShareSize;
            }
            sb.Append(Utility.FormatBytes(total));
            mainWindowModel.NetworkStatus = sb.ToString();
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
            o.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(o_PropertyChanged);
            popupController.AddWindow(o.View, "Settings");
        }


        private void SettingsEditDownloadDir()
        {
             string folder = string.Empty;
             if (browser.SelectFolder(out folder))
             {
                 model.DownloadFolder = folder;
             }
        }

        /// <summary>
        /// Copy settings changes to the main gui vm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void o_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Fap.Application.ViewModels.SettingsViewModel s = sender as Fap.Application.ViewModels.SettingsViewModel;
            if (null != s)
            {
                switch (e.PropertyName)
                {
                    case "Nickname":
                        mainWindowModel.Nickname = model.Nickname;
                        break;
                    case "Description":
                        mainWindowModel.Description = model.Description;
                       // peerController.AnnounceUpdate();
                        break;
                }
            }
        }

        void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Avatar":
                    mainWindowModel.Avatar = model.Avatar;
                   // peerController.AnnounceUpdate();
                    break;
                case "Nickname":
                    mainWindowModel.Nickname = model.Nickname;
                  //  peerController.AnnounceUpdate();
                    break;
                case "Description":
                    mainWindowModel.Description = model.Description;
                  //  peerController.AnnounceUpdate();
                    break;
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
                    model.Avatar = ms.ToArray();
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
            peerController.SendChatMessage(mainWindowModel.CurrentChatMessage);
            mainWindowModel.CurrentChatMessage = string.Empty;
        }


        public void Run()
        {
            mainWindowModel.Show();
           // peerController.StartBroadcast();
           // peerController.StartBroadcastClient();
            watchdog.Run();
            downloadService.Run();
        }

        public void ShutDown()
        {
           // model.Server.Stop();
          //  peerController.StopBroadcast();
          //  peerController.AnnounceQuit();
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
            while (model.Sessions.ToList().Where(s=>!s.IsUpload  && s.InUse).Count() > 0)
            {
                //Wait for sessions to close
                Thread.Sleep(20);

                if (Environment.TickCount - start > 10000)
                    break;
            }
            model.Save();
            model.DownloadQueue.Save();
           Environment.FailFast(null);

            mainWindowModel.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
            new Action(
             delegate()
             {
                 mainWindowModel.Close();
             }
            ));
            
        }
    }
}
