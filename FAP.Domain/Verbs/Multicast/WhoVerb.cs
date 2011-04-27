using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAP.Domain.Verbs.Multicast
{
    public class WhoVerb
    {
        public static string Message = "FAPWHO";

        public static string CreateRequest()
        {
            return Message;
        }
    }
}
