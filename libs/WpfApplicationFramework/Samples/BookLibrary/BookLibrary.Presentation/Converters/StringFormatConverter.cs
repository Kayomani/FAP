using System;
using System.Globalization;
using System.Windows.Data;

namespace BookLibrary.Presentation.Converters
{
    public class StringFormatConverter : IValueConverter
    {
        private static readonly StringFormatConverter defaultInstance = new StringFormatConverter();

        public static StringFormatConverter Default { get { return defaultInstance; } }
        

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string format = parameter as string ?? "{0}";

            return string.Format(culture, format, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
