using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Fap.Domain.Entity
{
    public class NetworkInterface
    {
        private string name;
        private long speed;
        private string description;
        private IPAddress address;

        public string Name
        {
            set { name = value; }
            get { return name; }
        }

        public long Speed
        {
            set { speed = value; }
            get { return speed; }
        }

        public string Description
        {
            set { description = value; }
            get { return description; }
        }

        public IPAddress Address
        {
            set { address = value; }
            get { return address; }
        }
    }
}
