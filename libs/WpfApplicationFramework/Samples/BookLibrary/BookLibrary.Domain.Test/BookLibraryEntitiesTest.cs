using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace BookLibrary.Domain.Test
{
    [TestClass]
    public class BookLibraryEntitiesTest
    {
        [TestMethod]
        public void EntityToStringTest()
        {
            MethodInfo method = typeof(BookLibraryEntities).GetMethod("EntityToString", 
                BindingFlags.Static | BindingFlags.NonPublic);

            Entity entity = new Entity() { ToStringValue = "Test1" };
            Assert.AreEqual("Test1", method.Invoke(null, new object[] { entity }));

            FormattableEntity entity2 = new FormattableEntity() { ToStringValue = "Test2" };
            Assert.AreEqual("Test2", method.Invoke(null, new object[] { entity2 }));
        }


        private class Entity
        {
            public string ToStringValue;

            public override string ToString() { return ToStringValue; }
        }

        private class FormattableEntity : IFormattable
        {
            public string ToStringValue;
            
            public string ToString(string format, IFormatProvider formatProvider)
            {
                return ToStringValue;
            }
        }
    }
}
