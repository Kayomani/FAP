using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAP.Domain.Services
{
    public class BonjourAnnouncerService
    {
        private Bonjour.DNSSDEventManager eventManager = null;

        public BonjourAnnouncerService()
        {
            eventManager = new Bonjour.DNSSDEventManager();
        }
    }
}
