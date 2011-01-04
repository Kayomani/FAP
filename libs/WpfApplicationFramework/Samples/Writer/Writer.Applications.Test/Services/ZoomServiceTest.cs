using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Writer.Applications.Services;
using System.Waf.UnitTesting;

namespace Writer.Applications.Test.Services
{
    [TestClass]
    public class ZoomServiceTest
    {
        [TestMethod]
        public void DefaultAndActiveZoomTest()
        {
            ZoomService service = new ZoomService();
            
            Assert.IsTrue(service.DefaultZooms.SequenceEqual(new double[] { 2, 1.5, 1.25, 1, 0.75, 0.5 }));
            Assert.AreEqual(1, service.ActiveZoom);

            AssertHelper.PropertyChangedEvent(service, x => x.ActiveZoom, () => service.ActiveZoom = 1.5);
            Assert.AreEqual(1.5, service.ActiveZoom);
        }
    }
}
