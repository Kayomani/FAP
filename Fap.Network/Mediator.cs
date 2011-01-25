#region Copyright Kayomani 2010.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
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
using Fap.Network.Entity;

/**
 * Request
 * 
 * FAPR/1.0           [Mandatory]
 * COMMAND PARAM     [Mandatory][Optional]
 * <RequestID> [Mandatory]
 * <ContentSize>     [Mandatory]
 * <AdditionalHeaders k:v> [Optional] e.g.
 * Close: Yes
 * Content: image/jpeg
 * 
 * Response
 * FAPR/1.0
 * 0               [Mandatory] Status - Non zero is error
 * <RequestID> [Mandatory]
 * <ContentSize>     [Mandatory]
 * <AdditionalHeaders k:v> [Optional] e.g.
 * Close: Yes
 * Content: image/jpeg
 * */

namespace Fap.Network
{
    public class Mediator
    {
        private static readonly string PROTOCOL = "FAPR/1.0";
        private static readonly char NEWLINE = '\n';

        public static byte[] Serialize(Request r)
        {
            StringBuilder sb = new StringBuilder();
            //Add protocol header
            sb.Append(PROTOCOL);
            sb.Append(NEWLINE);
            //Add command + param
            if (string.IsNullOrEmpty(r.Param))
            {
                sb.Append(MakeSafe(r.Command));
                sb.Append(NEWLINE);
            }
            else
            {
                sb.Append(MakeSafe(r.Command));
                sb.Append(" ");
                sb.Append(MakeSafe(r.Param));
                sb.Append(NEWLINE);
            }
            //Add request ID
            if (string.IsNullOrEmpty(r.RequestID))
                sb.Append("0");
            else
                sb.Append(r.RequestID);
            sb.Append(NEWLINE);
            //Add content size
            sb.Append(r.ContentSize.ToString());
            sb.Append(NEWLINE);
            //Add additional headers
            if (r.ConnectionClose)
            {
                sb.Append("Connection:close");
                sb.Append(NEWLINE);
            }
            if (!string.IsNullOrEmpty(r.ContentType))
            {
                sb.Append("Content-Type: " + r.ContentType);
                sb.Append(NEWLINE);
            }
            foreach (var s in r.AdditionalHeaders)
            {
                sb.Append(MakeSafe(s.Key));
                sb.Append(":");
                sb.Append(MakeSafe(s.Value));
                sb.Append(NEWLINE);
            }
            //Terminate header
            sb.Append(NEWLINE);
            return Encoding.Unicode.GetBytes(sb.ToString());
        }

        private static string MakeSafe(string s)
        {
            if (null == s)
                return string.Empty;
            s = s.Replace("\n", "\\n").Replace("\r", "\\r");
            return s;
        }


        public static bool Deserialize(string input, out Request r)
        {
            r = new Request();
            string[] split = input.Split('\n');
            if (split.Length < 5)
                return false;
            if (split[0] != PROTOCOL)
                return false;//Only accept the same protocol for now..
            if (string.IsNullOrEmpty(split[1]))
                return false;//No command present
            int paramsplit = split[1].IndexOf(' ');
            if (paramsplit!=-1)
            {
                r.Command = split[1].Substring(0,paramsplit);
                if (split[1].Length > paramsplit + 1)
                    r.Param = split[1].Substring(paramsplit + 1, split[1].Length - (paramsplit + 1));
            }
            else
            {
                r.Command = split[1];
            }
            //Get id
            r.RequestID = split[2];
            //Get content size
            long size = 0;
            long.TryParse(split[3], out size);
            r.ContentSize = size;
            //Parse additonal + option headers
            for (int i = 4; i < split.Length; i++)
            {
                int index = split[i].IndexOf(':');

                if (index!=-1)
                {
                    string key = split[i].Substring(0, index);
                    if (split[i].Length >= index + 1)
                    {
                        string value = split[i].Substring(index + 1, split[i].Length - (index + 1));
                        if (string.Equals(key, "Connection", StringComparison.InvariantCultureIgnoreCase))
                        {
                            r.ConnectionClose = string.Equals(value, "close", StringComparison.InvariantCultureIgnoreCase);
                        }
                        else if (string.Equals(key, "Content-Type", StringComparison.InvariantCultureIgnoreCase))
                        {
                            r.ContentType = value;
                        }
                        else
                        {
                            r.AdditionalHeaders.Add(key, value);
                        }
                    }
                }
            }
            return true;
        }
        public static bool Deserialize(string input, out Response r)
        {
            r = new Response();
            try
            {
                string[] split = input.Split('\n');
                if (split.Length < 3)
                    return false;
                if (split[0] != PROTOCOL)
                    return false;//Only accept the same protocol for now..
                //Parse status
                r.Status = int.Parse(split[1]);
                //Get id
                r.RequestID = split[2];
                //Get content size
                long size = 0;
                long.TryParse(split[3], out size);
                r.ContentSize = size;
                //Parse additonal + option headers
                for (int i = 4; i < split.Length; i++)
                {
                    int index = split[i].IndexOf(':');
                    if (index != -1)
                    {
                        string key = split[i].Substring(0, index);
                        if (split[i].Length >= index)
                        {
                            string value = split[i].Substring(index + 1, split[i].Length - (index + 1));
                            if (string.Equals(key, "Connection", StringComparison.InvariantCultureIgnoreCase))
                            {
                                r.ConnectionClose = string.Equals(value, "close", StringComparison.InvariantCultureIgnoreCase);
                            }
                            else if (string.Equals(key, "Content-Type", StringComparison.InvariantCultureIgnoreCase))
                            {
                                r.ContentType = value;
                            }
                            else
                            {
                                r.AdditionalHeaders.Add(key, value);
                            }
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static byte[] Serialize(Response r)
        {
            StringBuilder sb = new StringBuilder();
            //Protocol
            sb.Append(PROTOCOL);
            sb.Append(NEWLINE);
            //Response status
            sb.Append(r.Status.ToString());
            sb.Append(NEWLINE);
            //Request ID
            sb.Append(r.RequestID);
            sb.Append(NEWLINE);
            //Content size
            sb.Append(r.ContentSize);
            sb.Append(NEWLINE);
            //Add additional headers
            if (r.ConnectionClose)
            {
                sb.Append("Connection: close");
                sb.Append(NEWLINE);
            }
            if (!string.IsNullOrEmpty(r.ContentType))
            {
                sb.Append("Content-Type: " + r.ContentType);
                sb.Append(NEWLINE);
            }
            foreach (var s in r.AdditionalHeaders)
            {
                sb.Append(s.Key);
                sb.Append(":");
                sb.Append(s.Value);
                sb.Append(NEWLINE);
            }
            //Terminate header
            sb.Append(NEWLINE);
            return Encoding.Unicode.GetBytes(sb.ToString());
        }
    }
}
