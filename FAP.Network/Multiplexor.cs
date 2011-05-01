#region Copyright Kayomani 2011.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
/**
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or any 
    later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Network.Entities;
using HttpServer;
using System.IO;
using System.Net;
using HttpServer.Headers;

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

            HeaderCollection headers = r.Headers as HeaderCollection;
            if(null!=headers)
            {
                foreach (var  h in headers)
                {
                    StringHeader header = h as StringHeader;
                    if (null != header)
                    {
                        switch (header.Name.ToUpper())
                        {
                            case "FAP-AUTH":
                                req.AuthKey = header.Value;
                                break;
                            case "FAP-SOURCE":
                                req.SourceID = header.Value;
                                break;
                            case "FAP-OVERLORD":
                                req.OverlordID = header.Value;
                                break;
                        }
                    }
                }
            }
            return req;
        }


        public static string GetPostString(IRequest e)
        {
            using (StreamReader reader = new StreamReader(e.Body,Encoding.Unicode))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
