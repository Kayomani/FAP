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
using System.IO;
using Fap.Domain.Services;
using Fap.Domain.Entity;
using Fap.Foundation;
using Autofac;
using System.Threading;
using NLog;

namespace Fap.Application.Controllers
{
    public class SharesController : AsyncControllerBase
    {
        private  SharesViewModel viewModel;
        private  QueryViewModel browser;
        private readonly Logger logger;
        private readonly Model model;
        private LANPeerConnectionService peerController;
        private IContainer container;
        private ShareInfoService scanner;

        public SharesViewModel ViewModel { get { return viewModel; } }

        public SharesController(IContainer c,  Model m, LANPeerConnectionService p)
        {
            logger = LogManager.GetLogger("faplog");
            model = m;
            peerController = p;
            container = c;
            scanner = c.Resolve<ShareInfoService>();
        }
        public void Initalise()
        {
            viewModel = container.Resolve<SharesViewModel>();
            browser = container.Resolve<QueryViewModel>();
            viewModel.AddCommand = new DelegateCommand(AddCommand);
            viewModel.RefreshCommand = new DelegateCommand(RefreshCommand);
            viewModel.RemoveCommand = new DelegateCommand(RemoveCommand);
            viewModel.RenameCommand = new DelegateCommand(RenameCommand);
            viewModel.Shares = model.Shares;
            RefreshClientStats();
        }


        private void RefreshClientStats()
        {
            model.Node.ShareSize = model.Shares.Select(s => s.Size).Sum();
            model.Node.FileCount = model.Shares.Select(s => s.FileCount).Sum();
        }

        private void AddCommand()
        {
            string folder = string.Empty;
            if (browser.SelectFolder(out folder))
            {

                if (model.Shares.Where(os => os.Path == folder).Count() > 0)
                {
                    logger.Error("A share with this path already exists");
                }

                try
                {
                    //Check folder is accessible.
                    Directory.GetFiles(folder);

                    if (model.Shares.Where(sh => sh.Path == folder).Count() > 0)
                    {
                        System.Windows.MessageBox.Show("You have already shared this folder!");
                        return;
                    }

                    Share s = new Share();
                    string name = folder;
                    if (name.Contains(Path.DirectorySeparatorChar))
                    {
                        name = name.Substring(name.LastIndexOf(Path.DirectorySeparatorChar) + 1, (name.Length - name.LastIndexOf(Path.DirectorySeparatorChar)) - 1);

                    }
                    //Check name is valid and ok
                    bool retry = true;
                    do
                    {
                        MessageBoxViewModel messagebox = container.Resolve<MessageBoxViewModel>();
                        messagebox.Response = name;
                        messagebox.Message = "What do you want to name the share?";
                        if (messagebox.ShowDialog())
                        {
                            name = messagebox.Response;
                            retry = (model.Shares.Where(ss => ss.Name == name).Count() != 0 || name.Length==0);
                            if (retry)
                                System.Windows.MessageBox.Show("This name is already taken, please choose a different one!");
                        }
                        else
                        {
                            return;
                        }
                    }
                    while (retry);

                    if (name.Length > 0)
                    {
                        s.Name = name;
                        s.Path = folder;
                        model.Shares.Add(s);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(AsyncRefresh), s);
                    }
                }
                catch (Exception e)
                {
                    logger.ErrorException("Add share error",e);
                }
            }
        }

        private void AsyncRefresh(object o)
        {
            Share s = o as Share;
            if (null != s)
            {
                s.Status = "Scanning..";
                var info = scanner.RefreshPath(s);
                s.Size = info.Size;
                s.FileCount = info.FileCount;
                s.Status = string.Empty;
                s.LastRefresh = DateTime.Now;
                RefreshClientStats();
            }
        }

        public void RefreshShareInfo()
        {
            QueueWork(new DelegateCommand(AsyncRefreshShareInfo));
        }

        private void AsyncRefreshShareInfo()
        {
            foreach (var share in model.Shares.ToList().Where(s=>(DateTime.Now-s.LastRefresh).TotalMinutes>30).ToList())
                AsyncRefresh(share);
            RefreshClientStats();
        }

        private void RefreshCommand()
        {
            QueueWork(new DelegateCommand(AsyncRefreshCommand));
        }

        private void AsyncRefreshCommand()
        {
            foreach (var share in model.Shares.ToList())
                AsyncRefresh(share);
            RefreshClientStats();
        }

        private void RemoveCommand()
        {
            if (null != viewModel.SelectedShare)
            {
                scanner.RemoveShare(viewModel.SelectedShare.Name);
                model.Shares.Remove(viewModel.SelectedShare);
            }
        }

        private void RenameCommand()
        {
            if (null != viewModel.SelectedShare)
            {
                MessageBoxViewModel messagebox = container.Resolve<MessageBoxViewModel>();
                messagebox.Response = viewModel.SelectedShare.Name;
                messagebox.Message = "What do you want to rename it to?";
                if (messagebox.ShowDialog(ViewModel.View))
                {
                    if (model.Shares.Where(s => s.Name == messagebox.Response && s != viewModel.SelectedShare).Count() == 0 && messagebox.Response.Length != 0)
                    {
                        viewModel.SelectedShare.Name = messagebox.Response;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("This name is already taken, please choose a different one!");
                    }
                }
            }
        }

        protected class ScanInfo
        {
            public long Size { set; get; }
            public long FileCount { set; get; }
        }
    }

}
