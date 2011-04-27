using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Network.Entities;
using HttpServer;
using System.IO;

namespace FAP.Network
{
    public class Multiplexor
    {
        private readonly static string preample = "/Fap.app/";

        public static string Encode(string url, string verb, string param)
        {
            StringBuilder sb = new StringBuilder();
            if (!url.StartsWith("http://"))
                sb.Append("http://");
            sb.Append(url);
            if (url.EndsWith("/"))
                sb.Append(preample.Substring(1));
            else
                sb.Append(preample);
            sb.Append(verb);
            if (!string.IsNullOrEmpty(param))
            {
                sb.Append("?p=");
                sb.Append(Convert.ToBase64String(Encoding.Unicode.GetBytes(param)));
            }
            string result = sb.ToString();
            sb.Length = 0;
            sb = null;
            return result;
        }

        public static NetworkRequest Decode(IRequest r)
        {
            NetworkRequest req = new NetworkRequest();
            if (!r.Uri.AbsolutePath.StartsWith(preample))
                throw new Exception("Malformed url");
            req.Verb = r.Uri.AbsolutePath.Substring(preample.Length);

            var param = r.Parameters.Where(p => p.Name == "p").FirstOrDefault();
            if (null != param)
                req.Param = Encoding.Unicode.GetString(Convert.FromBase64String(param.Value));
            if (r.Method == "POST")
            {
                req.Data = GetPostString(r);
            }
            return req;
        }


        public static string GetPostString(IRequest e)
        {
            using (StreamReader reader = new StreamReader(e.Body))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
