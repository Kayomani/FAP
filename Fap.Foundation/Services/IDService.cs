using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fap.Foundation.Services
{
    public class IDService
    {
        public static string CreateID()
        {
            return Guid.NewGuid().ToString().Replace("-", "").ToUpper();
        }
    }
}
