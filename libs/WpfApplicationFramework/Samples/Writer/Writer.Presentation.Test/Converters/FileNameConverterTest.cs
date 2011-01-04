using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Writer.Presentation.Converters;
using System.Waf.UnitTesting;
using System.Windows;

namespace Writer.Presentation.Test.Converters
{
    [TestClass]
    public class FileNameConverterTest
    {
        [TestMethod]
        public void ConvertTest()
        {
            FileNameConverter converter = FileNameConverter.Default;

            Assert.AreEqual("Document 1.rtf", 
                converter.Convert(new object[] { "Document 1.rtf", false }, typeof(string), null, null));
            Assert.AreEqual("Document 1.rtf*",
                converter.Convert(new object[] { "Document 1.rtf", true }, typeof(string), null, null));

            Assert.AreEqual(DependencyProperty.UnsetValue, converter.Convert(new object[] { new object(), new object() }, 
                typeof(string), null, null));

            AssertHelper.ExpectedException<NotImplementedException>(() =>
                converter.ConvertBack(null, null, null, null));
        }
    }
}
