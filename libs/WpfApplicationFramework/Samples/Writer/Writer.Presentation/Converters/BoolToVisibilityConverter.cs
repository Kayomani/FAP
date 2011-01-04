using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Writer.Presentation.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        private static readonly BoolToVisibilityConverter defaultInstance = new BoolToVisibilityConverter();

        public static BoolToVisibilityConverter Default { get { return defaultInstance; } }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
