using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Waf.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BookLibrary.Foundation.Test
{
    [TestClass]
    public class DataErrorInfoExtensionsTest
    {
        [TestMethod]
        public void ValidateTest()
        {
            AssertHelper.ExpectedException<ArgumentNullException>(() => DataErrorInfoExtensions.Validate(null));
            AssertHelper.ExpectedException<ArgumentNullException>(() => DataErrorInfoExtensions.Validate(null, "Name"));

            MockEntity entity = new MockEntity();
            entity.Error = "Test Error";
            Assert.AreEqual("Test Error", entity.Validate());

            entity.Errors.Add("Name", "Name Error");
            Assert.AreEqual("Name Error", entity.Validate("Name"));
        }



        private class MockEntity : IDataErrorInfo
        {
            public Dictionary<string, string> Errors = new Dictionary<string, string>();

            public string Error { get; set; }

            public string this[string columnName]
            {
                get { return Errors[columnName]; }
            }
        }
    }
}
