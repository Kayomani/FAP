using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BookLibrary.Foundation;
using System.Globalization;
using BookLibrary.Domain.Properties;
using System.Reflection;

namespace BookLibrary.Domain.Test
{
    [TestClass]
    public class PersonTest
    {
        [TestMethod]
        public void GeneralPersonTest()
        {
            Person person = new Person();
            Assert.IsNotNull(person.Id);

            person.Firstname = "Harry";
            person.Lastname = "Potter";
            person.Email = "harry.potter@hogwarts.edu";

            Assert.AreEqual("", person.Validate());

            Assert.AreEqual("Harry Potter", person.ToString(null, CultureInfo.InvariantCulture));

            // Read the protected DataErrorSupport property via reflection
            DataErrorSupport dataErrorSupport = typeof(Person).GetProperty("DataErrorSupport", 
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(person, null) as DataErrorSupport;
            Assert.IsNotNull(dataErrorSupport);
        }

        [TestMethod]
        public void PersonFirstnameValidationTest()
        {
            Person person = new Person();

            Assert.IsNull(person.Firstname);
            Assert.AreEqual(Resources.FirstnameMandatory, person.Validate("Firstname"));
            
            person.Firstname = TestHelper.CreateString('A', 31);
            Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, Resources.FirstnameMaxLength, 30), 
                person.Validate("Firstname"));
            
            person.Firstname = TestHelper.CreateString('A', 30);
            Assert.AreEqual("", person.Validate("Firstname"));
        }

        [TestMethod]
        public void PersonLastnameValidationTest()
        {
            Person person = new Person();

            Assert.IsNull(person.Lastname);
            Assert.AreEqual(Resources.LastnameMandatory, person.Validate("Lastname"));
            
            person.Lastname = TestHelper.CreateString('A', 31);
            Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, Resources.LastnameMaxLength, 30),
                person.Validate("Lastname"));
            
            person.Lastname = TestHelper.CreateString('A', 30);
            Assert.AreEqual("", person.Validate("Lastname"));
        }

        [TestMethod]
        public void PersonEmailValidationTest()
        {
            Person person = new Person();

            Assert.IsNull(person.Email);
            Assert.AreEqual("", person.Validate("Email"));
            
            person.Email = TestHelper.CreateString('A', 101);
            Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, Resources.EmailMaxLength, 100),
                person.Validate("Email"));
            
            person.Email = "my." + TestHelper.CreateString('A', 88) + "@mail.com";
            Assert.AreEqual("", person.Validate("Email"));

            person.Email = "harry.potter";
            Assert.AreEqual(Resources.EmailInvalid, person.Validate("Email"));
            
            person.Email = "harry.potter@hogwarts";
            Assert.AreEqual(Resources.EmailInvalid, person.Validate("Email"));
            
            person.Email = "harry@hogwarts.edu";
            Assert.AreEqual("", person.Validate("Email"));
        }
    }
}
