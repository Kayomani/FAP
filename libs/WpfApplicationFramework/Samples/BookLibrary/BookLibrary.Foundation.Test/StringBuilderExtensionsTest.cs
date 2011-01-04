using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Waf.UnitTesting;

namespace BookLibrary.Foundation.Test
{
    [TestClass]
    public class StringBuilderExtensionsTest
    {
        [TestMethod]
        public void AppendInNewLineTest()
        {
            AssertHelper.ExpectedException<ArgumentNullException>(() => 
                StringBuilderExtensions.AppendInNewLine(null, "text"));
            
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendInNewLine("First Line");
            Assert.AreEqual("First Line", stringBuilder.ToString());

            stringBuilder.AppendInNewLine("Second Line");
            Assert.AreEqual("First Line" + Environment.NewLine + "Second Line", stringBuilder.ToString());

            stringBuilder.AppendInNewLine("Third Line");
            Assert.AreEqual("First Line" + Environment.NewLine + "Second Line" + Environment.NewLine
                + "Third Line", stringBuilder.ToString());
        }
    }
}
