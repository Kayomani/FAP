using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BookLibrary.Presentation.Converters;
using System.Waf.UnitTesting;

namespace BookLibrary.Presentation.Test.Converters
{
    [TestClass]
    public class StringFormatConverterTest
    {
        [TestMethod]
        public void StringFormatConverterBasicTest()
        {
            string book = "Star Wars - Heir to the Empire";
            string format = "Book: {0}";
            
            StringFormatConverter converter = StringFormatConverter.Default;
            Assert.AreEqual(string.Format(null, format, book), converter.Convert(book, null, format, null));
            Assert.AreEqual(string.Format(null, "{0}", book), converter.Convert(book, null, null, null));

            AssertHelper.ExpectedException<NotSupportedException>(() => converter.ConvertBack(null, null, null, null));
        }
    }
}
