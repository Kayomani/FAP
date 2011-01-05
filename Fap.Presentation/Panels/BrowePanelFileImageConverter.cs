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
using System.Windows.Data;
using System.Globalization;
using System.IO;
using Fap.Domain.Services;

namespace Fap.Presentation.Panels
{


    [ValueConversion(typeof(object), typeof(string))]
    public class BrowePanelFileImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
                          System.Globalization.CultureInfo culture)
        {

           

            string filename = string.Empty;

            if (value is Fap.Domain.Entity.FileSystemEntity)
            {

                Fap.Domain.Entity.FileSystemEntity fse = value as Fap.Domain.Entity.FileSystemEntity;
                filename = fse.Name;
                if (fse.IsFolder)
                    return "/Fap.Presentation;component/Images/folder.png";
            }

            else if (value is Fap.Domain.Entity.DownloadRequest)
            {
                Fap.Domain.Entity.DownloadRequest dl = value as Fap.Domain.Entity.DownloadRequest;
                filename = dl.FileName;
                if (dl.IsFolder)
                    return "/Fap.Presentation;component/Images/folder.png";
            }
            if (string.IsNullOrEmpty(filename))
                return string.Empty;
            return (object)IconService.GetIcon(filename);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                        System.Globalization.CultureInfo culture)
        {
            return null;
        }

    }

}
