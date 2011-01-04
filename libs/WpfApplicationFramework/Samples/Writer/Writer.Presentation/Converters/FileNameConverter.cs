using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows;

namespace Writer.Presentation.Converters
{
    public class FileNameConverter : IMultiValueConverter
    {
        private static FileNameConverter defaultInstance = new FileNameConverter();

        public static FileNameConverter Default { get { return defaultInstance; } }

        
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(values[0] is string) || !(values[1] is bool))
            {
                return DependencyProperty.UnsetValue;
            }
            
            string fileName = (string)values[0];
            bool modified = (bool)values[1];
            return Path.GetFileName(fileName) + (modified ? "*" : "");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
