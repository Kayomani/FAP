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
using Fap.Foundation;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace Fap.Domain.Services
{
    public class IconService
    {
        private static Dictionary<string, System.Windows.Media.Imaging.BitmapFrame> cache = new Dictionary<string, System.Windows.Media.Imaging.BitmapFrame>();

        public static System.Windows.Media.Imaging.BitmapFrame GetIcon(string filename)
        {
            string ext = Path.GetExtension(filename);

            if (cache.ContainsKey(ext))
            {
                return cache[ext];
            }
            else
            {
                System.Drawing.Icon i = IconReader.GetFileIcon(filename, IconReader.IconSize.Large, false);
                
                MemoryStream iconStream = new MemoryStream();
                i.Save(iconStream);
                iconStream.Seek(0, SeekOrigin.Begin);
                var bf = System.Windows.Media.Imaging.BitmapFrame.Create(iconStream);
                bf = ResizeHelper(bf, 16, 16, BitmapScalingMode.Fant);
                cache.Add(ext, bf);
                return bf;
            }
        }


        public static BitmapFrame ResizeHelper(BitmapFrame photo, int width,
                                          int height, BitmapScalingMode scalingMode)
        {

            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(
                group, scalingMode);
            group.Children.Add(
                new ImageDrawing(photo,
                    new Rect(0, 0, width, height)));
            var targetVisual = new DrawingVisual();
            var targetContext = targetVisual.RenderOpen();
            targetContext.DrawDrawing(group);
            var target = new RenderTargetBitmap(
                width, height, 96, 96, PixelFormats.Default);
            targetContext.Close();
            target.Render(targetVisual);
            var targetFrame = BitmapFrame.Create(target);
            return targetFrame;
        }
    }
}
