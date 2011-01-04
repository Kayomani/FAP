using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using BookLibrary.Domain;
using BookLibrary.Presentation.Properties;

namespace BookLibrary.Presentation.Converters
{
    public class LanguageToStringConverter : IValueConverter
    {
        private static readonly LanguageToStringConverter defaultInstance = new LanguageToStringConverter();

        public static LanguageToStringConverter Default { get { return defaultInstance; } }

        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Language)) { return null; }
            
            Language language = (Language)value;
            switch (language)
            {
                case Language.Undefined:
                    return Resources.Undefined;
                case Language.English:
                    return Resources.English;
                case Language.German:
                    return Resources.German;
                case Language.French:
                    return Resources.French;
                case Language.Spanish:
                    return Resources.Spanish;
                case Language.Chinese:
                    return Resources.Chinese;
                case Language.Japanese:
                    return Resources.Japanese;
            }
            throw new InvalidOperationException("Enum value is unknown.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
