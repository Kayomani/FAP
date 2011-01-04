using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using System.Waf.UnitTesting;

namespace BookLibrary.Foundation.Test
{
    [TestClass]
    public class DataErrorSupportTest
    {
        [TestMethod]
        public void DataErrorSupportWithoutRulesTest()
        {
            AssertHelper.ExpectedException<ArgumentNullException>(() => new DataErrorSupport(null));

            object entity = new object();
            DataErrorSupport dataErrorSupport = new DataErrorSupport(entity);
            Assert.AreEqual("", dataErrorSupport.Error);
            Assert.AreEqual("", dataErrorSupport[null]);
            Assert.AreEqual("", dataErrorSupport[""]);
            Assert.AreEqual("", dataErrorSupport["Name"]);
        }

        [TestMethod]
        public void DataErrorSupportWithRulesTest()
        {
            object entity = new object();
            DataErrorSupport dataErrorSupport = new DataErrorSupport(entity);

            string nameError = "Name is mandatory.";
            dataErrorSupport.AddValidationRule("Name", (instance, memberName) =>
            {
                return nameError;
            });
            
            Assert.AreEqual(nameError, dataErrorSupport.Error);
            Assert.AreEqual(nameError, dataErrorSupport[""]);
            Assert.AreEqual(nameError, dataErrorSupport[null]);
            Assert.AreEqual(nameError, dataErrorSupport["Name"]);
            Assert.AreEqual("", dataErrorSupport["Email"]);

            AssertHelper.ExpectedException<ArgumentException>(() => 
                dataErrorSupport.AddValidationRule("Name", (instance, memberName) => { return ""; }));

            string newLine = Environment.NewLine;
            string emailError = "The Email address is invalid.";
            dataErrorSupport.AddValidationRule("Email", (instance, memberName) =>
            {
                return emailError;
            });
            Assert.AreEqual(nameError + newLine + emailError, dataErrorSupport.Error);
            Assert.AreEqual(nameError + newLine + emailError, dataErrorSupport[""]);
            Assert.AreEqual(nameError + newLine + emailError, dataErrorSupport[null]);
            Assert.AreEqual(nameError, dataErrorSupport["Name"]);
            Assert.AreEqual(emailError, dataErrorSupport["Email"]);
            Assert.AreEqual("", dataErrorSupport["Lastname"]);

            string objectError = "Object Error";
            dataErrorSupport.AddValidationRule("", (instance, memberName) =>
            {
                return objectError;
            });
            Assert.AreEqual(nameError + newLine + emailError + newLine + objectError, dataErrorSupport.Error);
            Assert.AreEqual(nameError + newLine + emailError + newLine + objectError, dataErrorSupport[""]);
            Assert.AreEqual(nameError + newLine + emailError + newLine + objectError, dataErrorSupport[null]);
            Assert.AreEqual(nameError + newLine + objectError, dataErrorSupport["Name"]);
            Assert.AreEqual(emailError + newLine + objectError, dataErrorSupport["Email"]);
            Assert.AreEqual(objectError, dataErrorSupport["Lastname"]);

            AssertHelper.ExpectedException<ArgumentException>(() =>
                dataErrorSupport.AddValidationRule(null, (instance, memberName) => { return ""; }));
        }

        [TestMethod]
        public void ValidationRuleDelegateTest()
        {
            object entity = new object();
            DataErrorSupport dataErrorSupport = new DataErrorSupport(entity);
            
            string nameError = "Name is mandatory.";
            dataErrorSupport.AddValidationRule("Name", (instance, memberName) =>
            {
                Assert.AreEqual(entity, instance);
                Assert.AreEqual("Name", memberName);
                return nameError;
            });

            Assert.AreEqual(nameError, dataErrorSupport.Validate("Name"));
        }
    }
}
