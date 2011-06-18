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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Waf.Applications;
using System.Waf.Applications.Services;
using Autofac;
using FAP.Application.ViewModels;
using FAP.Domain.Entities;

namespace FAP.Application.Controllers
{
    internal class SettingsController
    {
        private readonly IContainer container;
        private readonly ApplicationCore core;
        private readonly Model model;
        private QueryViewModel browser;
        private SettingsViewModel viewModel;

        public SettingsController(IContainer c, Model m, ApplicationCore ac)
        {
            container = c;
            model = m;
            core = ac;
        }

        public SettingsViewModel ViewModel
        {
            get { return viewModel; }
        }

        public void Initaize()
        {
            if (null == viewModel)
            {
                browser = container.Resolve<QueryViewModel>();
                viewModel = container.Resolve<SettingsViewModel>();
                viewModel.Model = model;
                viewModel.EditDownloadDir = new DelegateCommand(SettingsEditDownloadDir);
                viewModel.ChangeAvatar = new DelegateCommand(ChangeAvatar);
                viewModel.ResetInterface = new DelegateCommand(ResetInterface);
                viewModel.DisplayQuickStart = new DelegateCommand(DisplayQuickStart);
            }
        }

        private void DisplayQuickStart()
        {
            core.ShowQuickStart();
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
                    var ms = new MemoryStream();
                    var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                    ms.SetLength(stream.Length);
                    stream.Read(ms.GetBuffer(), 0, (int) stream.Length);
                    ms.Flush();
                    stream.Close();
                    //Resize
                    var bitmap = new Bitmap(ms);
                    Image thumbnail = ResizeImage(bitmap, 100, 100);
                    ms = new MemoryStream();
                    thumbnail.Save(ms, ImageFormat.Png);
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
            FullsizeImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
            FullsizeImage.RotateFlip(RotateFlipType.Rotate180FlipNone);

            if (FullsizeImage.Width <= NewWidth)
                NewWidth = FullsizeImage.Width;

            int NewHeight = FullsizeImage.Height*NewWidth/FullsizeImage.Width;
            if (NewHeight > MaxHeight)
            {
                // Resize with height instead
                NewWidth = FullsizeImage.Width*MaxHeight/FullsizeImage.Height;
                NewHeight = MaxHeight;
            }

            Image NewImage = FullsizeImage.GetThumbnailImage(NewWidth, NewHeight, null, IntPtr.Zero);
            // Clear handle to original file so that we can overwrite it if necessary
            FullsizeImage.Dispose();
            // Save resized picture
            return NewImage;
        }

        private void ResetInterface()
        {
            model.LocalNode.Host = null;
            model.Save();
            container.Resolve<IMessageService>().ShowWarning("Interface selection reset.  FAP will now restart.");
            var notePad = new Process();

            notePad.StartInfo.FileName = Assembly.GetEntryAssembly().CodeBase;
            notePad.StartInfo.Arguments = "WAIT";
            notePad.Start();
            core.Exit();
        }
    }
}