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
using System.Linq;
using System.Net;
using HttpServer;
using HttpServer.Headers;
using HttpServer.Messages;
using HttpListener = HttpServer.HttpListener;

namespace FAP.Network.Server
{
    public enum RequestType
    {
        FAP,
        HTTP
    } ;

    public class NodeServer
    {
        #region Delegates

        public delegate bool Request(RequestType type, RequestEventArgs arg);

        #endregion

        private HttpListener listener;

        public event Request OnRequest;


        public void Start(IPAddress a, int port)
        {
            listener = HttpListener.Create(a, port);
            listener.RequestReceived += listener_RequestReceived;
            listener.Start(1000);
        }

        public void Stop()
        {
            listener.Stop();
        }

        private void listener_RequestReceived(object sender, RequestEventArgs e)
        {
            e.IsHandled = true;
            e.Response.Reason = string.Empty;
            string userAgent = string.Empty;
            IHeader uahead =
                e.Request.Headers.Where(h => string.Equals("User-Agent", h.Name, StringComparison.OrdinalIgnoreCase)).
                    FirstOrDefault();
            if (null != uahead)
                userAgent = uahead.HeaderValue;

            //Send to the correct handler
            if (userAgent.StartsWith("FAP"))
            {
                if (OnRequest(RequestType.FAP, e))
                    return;
            }
            if (OnRequest(RequestType.HTTP, e))
                return;
            e.Response.Reason = "Handler error";
            e.Response.Status = HttpStatusCode.InternalServerError;
            var generator = new ResponseWriter();
            generator.SendHeaders(e.Context, e.Response);
        }
    }
}