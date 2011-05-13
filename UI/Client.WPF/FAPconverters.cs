using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using Fap.Foundation;

namespace Fap.Presentation
{
    public class Base64Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                try
                {
                    string data = (string)value;
                    if (!string.IsNullOrEmpty(data))
                    {
                        return System.Convert.FromBase64String(data);
                    }
                }
                catch
                {

                }
            }
            return new byte[0];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[])
            {
                byte[] data = (byte[])value;
                return System.Convert.ToBase64String(data);
            }
            return string.Empty;
        }
    }

    public class SizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long)
            {
                return Utility.FormatBytes((long)value);
            }
            if (value is int)
            {
                return Utility.FormatBytes(System.Convert.ToInt64(value));
            }
            return Utility.FormatBytes(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int || value is long)
            {
                return Utility.ConverNumberToText((long)value);

            }
            return Utility.FormatBytes(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SpeedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int || value is long)
            {
                return Utility.FormatBytes((long)value) + "/s";

            }
            return Utility.FormatBytes(0) + "/s";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TrueSpeedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int || value is long)
            {
                return Utility.FormatBytesTrue((long)value) + "/s";

            }
            return Utility.FormatBytesTrue(0) + "/s";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
