using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Fap.Domain.Entity;
using Fap.Network.Entity;

namespace Fap.Presentation
{
    [ValueConversion(typeof(object), typeof(string))]
    public class SessionTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
                          System.Globalization.CultureInfo culture)
        {

            Session s = value as Session;
            if (null != s)
            {
                if (s.IsUpload)
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
