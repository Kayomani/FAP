using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Waf.Applications.Services;
using System.Waf.UnitTesting;

namespace Test.Waf.Applications.Services
{
    [TestClass]
    public class FileTypeTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            FileType fileType = new FileType("RichText Documents (*.rtf)", ".rtf");
            Assert.AreEqual("RichText Documents (*.rtf)", fileType.Description);
            Assert.AreEqual(".rtf", fileType.FileExtension);

            AssertHelper.ExpectedException<ArgumentException>(() => new FileType(null, ".rtf"));
            AssertHelper.ExpectedException<ArgumentException>(() => new FileType("", ".rtf"));
            AssertHelper.ExpectedException<ArgumentException>(() => new FileType("RichText Documents", null));
            AssertHelper.ExpectedException<ArgumentException>(() => new FileType("RichText Documents", ""));
            AssertHelper.ExpectedException<ArgumentException>(() => new FileType("RichText Documents", "rtf"));
        }
    }
}
