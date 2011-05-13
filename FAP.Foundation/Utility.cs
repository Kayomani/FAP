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
using System.Web;

namespace Fap.Foundation
{
    public class Utility
    {
        /// <summary>
        ///  HttpUtility.UrlEncode does this wrong :E  Is there a better way than this??
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string EncodeURL(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            url = url.Replace("!", "%21");
            url = url.Replace("*", "%2A");
            url = url.Replace("'", "%27");
            url = url.Replace("(", "%28");
            url = url.Replace(")", "%29");
            url = url.Replace(";", "%3B");
            url = url.Replace(":", "%3A");
            url = url.Replace("@", "%40");
            url = url.Replace("&", "%26");
            url = url.Replace("=", "%3D");
            url = url.Replace("+", "%2B");
            url = url.Replace("$", "%24");
            url = url.Replace(",", "%2C");
            url = url.Replace("/", "'%2F");
            url = url.Replace("?", "%3F");
            url = url.Replace("%", "%25");
            url = url.Replace("#", "%23");
            url = url.Replace("[", "%5B");
            url = url.Replace("]", "%5D");
            return url;
        }

        public static string DecodeURL(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            url = url.Replace("%21", "!");
            url = url.Replace("%2A", "*");
            url = url.Replace("%27", "'");
            url = url.Replace("%28", "(");
            url = url.Replace("%29", ")");
            url = url.Replace("%3B", ";");
            url = url.Replace("%3A", ":");
            url = url.Replace("%40", "@");
            url = url.Replace("%26", "&");
            url = url.Replace("%3D", "=");
            url = url.Replace("%2B", "+");
            url = url.Replace("%24", "$");
            url = url.Replace("%2C", ",");
            url = url.Replace("%2F", "/");
            url = url.Replace("%3F", "?");
            url = url.Replace("%25", "%");
            url = url.Replace("%23", "#");
            url = url.Replace("%5B", "[");
            url = url.Replace("%5D", "]");
            return HttpUtility.UrlDecode(url);
        }

        public static string FormatBytes(long bytes)
        {
            const int scale = 1024;
            string[] orders = new string[] { "TB", "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (bytes > max)
                    return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);

                max /= scale;
            }
            return "0 Bytes";
        }

        public static string FormatBytesTrue(long bytes)
        {
            const int scale = 1000;
            string[] orders = new string[] { "TB", "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (bytes > max)
                    return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);

                max /= scale;
            }
            return "0 Bytes";
        }

        public static string ConverNumberToText(long count)
        {
            const int scale = 1000;
            string[] orders = new string[] { "trillion", "billion", "million", "k", "" };
            double max = (double)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (count > max)
                    return string.Format("{0:##.#} {1}", count/ max, order);

                max /= scale;
            }
            return "0";
        }

        public static string ConvertNumberToTextSpeed(long count)
        {
            return FormatBytesTrue(count) + "/s";
        }
    }
}
