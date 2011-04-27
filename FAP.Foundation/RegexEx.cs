using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Fap.Foundation
{
    public class RegexEx
    {
        public static List<string> FindMatches(string regex, string data)
        {
            List<string> ret = new List<string>();
            Regex regx = new Regex(regex, RegexOptions.IgnoreCase);

            MatchCollection result = regx.Matches(data);

            foreach (Group group in result)
            {
                if (!ret.Contains(group.Value))
                    ret.Add(group.Value);
            }
            return ret;
        }
    }
}
