using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BookLibrary.Presentation.Converters;
using System.Waf.UnitTesting;
using BookLibrary.Domain;
using BookLibrary.Presentation.Properties;

namespace BookLibrary.Presentation.Test.Converters
{
    [TestClass]
    public class LanguageToStringConverterTest
    {
        [TestMethod]
        public void LanguageToStringConverterBasicTest()
        {
            LanguageToStringConverter converter = LanguageToStringConverter.Default;

            Assert.AreEqual(Resources.Undefined, converter.Convert(Language.Undefined, null, null, null));
            Assert.AreEqual(Resources.English, converter.Convert(Language.English, null, null, null));
            Assert.AreEqual(Resources.German, converter.Convert(Language.German, null, null, null));
            Assert.AreEqual(Resources.French, converter.Convert(Language.French, null, null, null));
            Assert.AreEqual(Resources.Spanish, converter.Convert(Language.Spanish, null, null, null));
            Assert.AreEqual(Resources.Chinese, converter.Convert(Language.Chinese, null, null, null));
            Assert.AreEqual(Resources.Japanese, converter.Convert(Language.Japanese, null, null, null));
            AssertHelper.ExpectedException<InvalidOperationException>(() => 
                converter.Convert((Language)(-1), null, null, null));

            Assert.IsNull(converter.Convert(12, null, null, null));
            
            AssertHelper.ExpectedException<NotSupportedException>(() => converter.ConvertBack(null, null, null, null));
        }
    }
}
