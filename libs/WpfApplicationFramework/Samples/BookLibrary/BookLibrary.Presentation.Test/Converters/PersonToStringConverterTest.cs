using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BookLibrary.Presentation.Converters;
using BookLibrary.Domain;
using BookLibrary.Presentation.Properties;
using System.Waf.UnitTesting;

namespace BookLibrary.Presentation.Test.Converters
{
    [TestClass]
    public class PersonToStringConverterTest
    {
        [TestMethod]
        public void PersonToStringConverterBasicTest()
        {
            Person person = new Person() { Firstname = "Harry", Lastname = "Potter" };
            
            PersonToStringConverter converter = PersonToStringConverter.Default;
            Assert.AreEqual(string.Format(null, Resources.PersonFormat, person.Firstname, person.Lastname), 
                converter.Convert(person, null, null, null));
            Assert.IsNull(converter.Convert(null, null, null, null));

            AssertHelper.ExpectedException<NotSupportedException>(() => converter.ConvertBack(null, null, null, null));
        }
    }
}
