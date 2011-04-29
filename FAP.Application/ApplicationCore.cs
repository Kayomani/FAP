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

namespace FAP.Application
{
    public class ApplicationCore
    {
        private IContainer container;

        private Listener client;
        private ShareInfoService shareInfo;
        private Listener server;
        private ConnectionController connectionController;

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

        

        public void Load()
        {
            model.Load();
            model.LocalNode.ID=  IDService.CreateID();
            shareInfo = container.Resolve<ShareInfoService>();
            shareInfo.Load();
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


        public void StartGUI()
        {
            ShowMainWindow();
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
            {
               // chatController.CreateConversation(peer);
            }
        }

        private void Compare()
        {
          //  CompareController cc = container.Resolve<CompareController>();
           // var vm = cc.Initalise();
           // popupController.AddWindow(vm.View, "Compare");
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
           // DownloadQueueController dqc = container.Resolve<DownloadQueueController>();
           // dqc.Initalise();
           // popupController.AddWindow(dqc.ViewModel.View, "Download Queue");
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
           // popupController.AddWindow(shareController.ViewModel.View, "Edit shares");
        }

        private void Settings()
        {
            var o = container.Resolve<SettingsViewModel>();
            o.Model = model;
            o.EditDownloadDir = new DelegateCommand(SettingsEditDownloadDir);
            o.ChangeAvatar = new DelegateCommand(ChangeAvatar);
            o.ResetInterface = new DelegateCommand(ResetInterface);
            //popupController.AddWindow(o.View, "Settings");
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
    }
}
