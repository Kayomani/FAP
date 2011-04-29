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
using FAP.Application.ViewModels;
using Autofac;
using System.Waf.Applications;
using FAP.Domain.Entities;
using System.IO;
using System.Drawing;
using System.Waf.Applications.Services;
using System.Diagnostics;
using System.Reflection;

namespace FAP.Application.Controllers
{
    class SettingsController
    {
        private SettingsViewModel viewModel;
        private QueryViewModel browser;
        private ApplicationCore core;

        private IContainer container;
        private Model model;

        public SettingsViewModel ViewModel { get { return viewModel; } }

        public SettingsController(IContainer c,Model m, ApplicationCore ac)
        {
            container = c;
            model = m;
            core = ac;
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
            }
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

        private void ResetInterface()
        {
            model.IPAddress = null;
            model.Save();
            container.Resolve<IMessageService>().ShowWarning("Interface selection reset.  FAP will now restart.");
            Process notePad = new Process();

            notePad.StartInfo.FileName = Assembly.GetEntryAssembly().CodeBase;
            notePad.StartInfo.Arguments = "WAIT";
            notePad.Start();
            core.Exit();
        }
    }
}
