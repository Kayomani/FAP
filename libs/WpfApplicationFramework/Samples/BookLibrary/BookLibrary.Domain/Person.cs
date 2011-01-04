using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using BookLibrary.Domain.Properties;
using BookLibrary.Foundation;

namespace BookLibrary.Domain
{
    public partial class Person : IDataErrorInfo, IFormattable
    {
        private static readonly Regex emailValidationRegex = new Regex(@"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$", 
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        [NonSerialized]
        private readonly DataErrorSupport dataErrorSupport;

        
        public Person()
        {
            // SQL Server Compact does not support entities with server-generated keys or values when it is used 
            // with the Entity Framework. Therefore, we need to create the keys ourselves.
            // See also: http://technet.microsoft.com/en-us/library/cc835494.aspx
            Id = Guid.NewGuid();

            dataErrorSupport = new DataErrorSupport(this)
                .AddValidationRule("Firstname", ValidateFirstname)
                .AddValidationRule("Lastname", ValidateLastname)
                .AddValidationRule("Email", ValidateEmail);
        }


        string IDataErrorInfo.Error { get { return dataErrorSupport.Error; } }

        string IDataErrorInfo.this[string memberName] { get { return dataErrorSupport[memberName]; } }

        protected DataErrorSupport DataErrorSupport { get { return dataErrorSupport; } }


        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, Resources.PersonToString, Firstname, Lastname);
        }

        private string ValidateFirstname(object objectInstance, string memberName)
        {
            if (string.IsNullOrEmpty(Firstname)) { return Resources.FirstnameMandatory; }
            if (Firstname.Length > 30) 
            { 
                return string.Format(CultureInfo.CurrentCulture, Resources.FirstnameMaxLength, 30); 
            }
            return "";
        }

        private string ValidateLastname(object objectInstance, string memberName)
        {
            if (string.IsNullOrEmpty(Lastname)) { return Resources.LastnameMandatory; }
            if (Lastname.Length > 30) 
            {
                return string.Format(CultureInfo.CurrentCulture, Resources.LastnameMaxLength, 30); 
            }
            return "";
        }

        private string ValidateEmail(object objectInstance, string memberName)
        {
            if (!string.IsNullOrEmpty(Email))
            {
                if (Email.Length > 100)
                {
                    return string.Format(CultureInfo.CurrentCulture, Resources.EmailMaxLength, 100);
                }
                if (!emailValidationRegex.IsMatch(Email))
                {
                    return Resources.EmailInvalid;
                }
            }
            return "";
        }
    }
}
