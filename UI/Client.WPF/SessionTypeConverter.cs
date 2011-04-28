using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using FAP.Domain.Entities;

namespace Fap.Presentation
{
    [ValueConversion(typeof(object), typeof(string))]
    public class SessionTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
                          System.Globalization.CultureInfo culture)
        {
            TransferSession s = value as TransferSession;
            if (null != s)
            {
                if (s.IsDownload)
                    return "/Fap.Presentation;component/Images/Download.png";
                else
                    return "/Fap.Presentation;component/Images/Upload.png";
           }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                        System.Globalization.CultureInfo culture)
        {
            return null;
        }

    }
}
