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
                System.Drawing.Icon i = IconReader.GetFileIcon(filename, IconReader.IconSize.Small, false);
                
                MemoryStream iconStream = new MemoryStream();
                i.Save(iconStream);
                iconStream.Seek(0, SeekOrigin.Begin);
                var bf = System.Windows.Media.Imaging.BitmapFrame.Create(iconStream);
                cache.Add(ext, bf);
                return bf;
            }
        }
    }
}
