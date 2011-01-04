using System;
using System.Globalization;
using System.Windows.Data;
using BookLibrary.Domain;
using BookLibrary.Presentation.Properties;

namespace BookLibrary.Presentation.Converters
{
    public class PersonToStringConverter : IValueConverter
    {
        private static readonly PersonToStringConverter defaultInstance = new PersonToStringConverter();

        public static PersonToStringConverter Default { get { return defaultInstance; } }

        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Person person = value as Person;
            if (person == null) { return null; }

            return string.Format(culture, Resources.PersonFormat, person.Firstname, person.Lastname);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
