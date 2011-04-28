using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Domain.Net;

namespace FAP.Domain.Verbs
{
    public class HelloVerb
    {
        public static readonly string Preamble = "FAPHELLO";

        public string CreateRequest(string address, string name, string id, int priority)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Preamble);
            sb.Append("\n");
            sb.Append(Convert.ToBase64String(Encoding.Unicode.GetBytes(address)));
            sb.Append("\n");
            sb.Append(Convert.ToBase64String(Encoding.Unicode.GetBytes(name)));
            sb.Append("\n");
            sb.Append(Convert.ToBase64String(Encoding.Unicode.GetBytes(id)));
            sb.Append("\n");
            sb.Append(priority);
            return sb.ToString();
        }

        public DetectedNode ParseRequest(string input)
        {
            try
            {
                string[] split = input.Split('\n');
                if (split.Length == 5)
                    return new DetectedNode() { Address = DecodeString(split[1]), NetworkName = DecodeString(split[2]), ID = DecodeString(split[3]), Priority = int.Parse(split[4]) };
            }
            catch { }
            return null;
        }

        private string DecodeString(string s)
        {
            return Encoding.Unicode.GetString(Convert.FromBase64String(s));
        }
    }
}
