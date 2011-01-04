using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Writer.Presentation.Converters;
using System.Windows;
using System.Waf.UnitTesting;

namespace Writer.Presentation.Test.Converters
{
    [TestClass]
    public class BoolToVisibilityConverterTest
    {
        [TestMethod]
        public void ConvertTest()
        {
            BoolToVisibilityConverter converter = BoolToVisibilityConverter.Default;
            
            Assert.AreEqual(Visibility.Visible, converter.Convert(true, typeof(Visibility), null, null));
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(false, typeof(Visibility), null, null));

            AssertHelper.ExpectedException<NotImplementedException>(() =>
                converter.ConvertBack(null, null, null, null));
        }
    }
}
