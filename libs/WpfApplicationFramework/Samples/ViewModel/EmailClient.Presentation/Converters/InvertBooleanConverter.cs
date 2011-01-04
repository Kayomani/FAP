using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace EmailClient.Presentation.Converters
{
    public class InvertBooleanConverter : IValueConverter
    {
        private static readonly InvertBooleanConverter defaultInstance = new InvertBooleanConverter();

        public static InvertBooleanConverter Default { get { return defaultInstance; } }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }
}
