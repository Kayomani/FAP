using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using System.Linq;

namespace LinqToWmi.ProtoGenerator
{
    class WmiMetaInformation
    {
        public ManagementObject GetMetaInformation(string name)
        {
            ManagementObject value = null;
            ManagementScope scope = new ManagementScope(@"\\localhost");

            ManagementPath path = new ManagementPath(name);
            ManagementClass management = new ManagementClass(scope, path, null);
   
            foreach (ManagementObject child in management.GetInstances())
            {            
                value = child;
                break;
            }
            management.Dispose();
            return value;
        }
    }
}
