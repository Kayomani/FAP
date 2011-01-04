using System;
using System.ComponentModel;
using System.Globalization;
using BookLibrary.Domain.Properties;
using BookLibrary.Foundation;

namespace BookLibrary.Domain
{
    public partial class Book : IDataErrorInfo, IFormattable
    {
        [NonSerialized]
        private readonly DataErrorSupport dataErrorSupport;
        

        public Book()
        {
            // SQL Server Compact does not support entities with server-generated keys or values when it is used 
            // with the Entity Framework. Therefore, we need to create the keys ourselves.
            // See also: http://technet.microsoft.com/en-us/library/cc835494.aspx
            Id = Guid.NewGuid();

            dataErrorSupport = new DataErrorSupport(this)
                .AddValidationRule("Title", ValidateTitle)
                .AddValidationRule("Author", ValidateAuthor)
                .AddValidationRule("Publisher", ValidatePublisher)
                .AddValidationRule("Isbn", ValidateIsbn)
                .AddValidationRule("Pages", ValidatePages);

            LendToReference.AssociationChanged += LendToReferenceAssociationChanged;
        }


        public Language Language
        {
            // Entity Framework doesn't support Enums. We use an Int32 value internal and cast it to our enum.
            get { return (Language)LanguageInternal; }
            set { LanguageInternal = (int)value; }
        }

        string IDataErrorInfo.Error { get { return dataErrorSupport.Error; } }

        string IDataErrorInfo.this[string memberName] { get { return dataErrorSupport[memberName]; } }

        protected DataErrorSupport DataErrorSupport { get { return dataErrorSupport; } }


        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, Resources.BookToString, Title, Author);
        }

        protected override void OnPropertyChanged(string property)
        {
            base.OnPropertyChanged(property);

            if (property == "LanguageInternal")
            {
                OnPropertyChanged("Language");
            }
        }

        private string ValidateTitle(object objectInstance, string memberName)
        {
            if (string.IsNullOrEmpty(Title)) { return Resources.TitleMandatory; }
            if (Title.Length > 100) 
            { 
                return string.Format(CultureInfo.CurrentCulture, Resources.TitleMaxLength, 100); 
            }
            return "";
        }

        private string ValidateAuthor(object objectInstance, string memberName)
        {
            if (string.IsNullOrEmpty(Author)) { return Resources.AuthorMandatory; }
            if (Author.Length > 100) 
            {
                return string.Format(CultureInfo.CurrentCulture, Resources.AuthorMaxLength, 100);
            }
            return "";
        }

        private string ValidatePublisher(object objectInstance, string memberName)
        {
            if (Publisher != null && Publisher.Length > 100) 
            {
                return string.Format(CultureInfo.CurrentCulture, Resources.PublisherMaxLength, 100);
            }
            return "";
        }

        private string ValidateIsbn(object objectInstance, string memberName)
        {
            if (Isbn != null && Isbn.Length > 14) 
            {
                return string.Format(CultureInfo.CurrentCulture, Resources.IsbnMaxLength, 14);
            }
            return "";
        }

        private string ValidatePages(object objectInstance, string memberName)
        {
            if (Pages < 0) 
            { 
                return string.Format(CultureInfo.CurrentCulture, Resources.PagesEqualOrLarger, 0); 
            }
            return "";
        }

        private void LendToReferenceAssociationChanged(object sender, CollectionChangeEventArgs e)
        {
            // The navigation property LendTo doesn't support the PropertyChanged event. We have to raise it ourselves.
            OnPropertyChanged("LendTo");
        }
    }
}
