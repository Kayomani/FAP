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
using Fap.Foundation;
using System.Threading;

namespace HttpServer
{
    public class HTTPContextManager
    {
        private static BackgroundSafeObservable<HttpContext> connections = new BackgroundSafeObservable<HttpContext>();
        private static bool running = false;
        public static readonly int MAX_KEEPALIVE = 150;//2.5 minutes

        public static void Register(HttpContext c)
        {
            if (!connections.Contains(c))
                connections.Add(c);
        }

        public static void Unregister(HttpContext c)
        {
            connections.Remove(c);
        }

        public static void Start()
        {
            if (!running)
            {
                lock (connections)
                {
                    running = true;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(checkConnections));
                }
            }
        }

        private static void checkConnections(object o)
        {
            while (true)
            {
                foreach (var connection in connections.ToList())
                {
                    try
                    {
                        if ((DateTime.Now - connection.LastAction).TotalSeconds > MAX_KEEPALIVE)
                        {
                            connection.Disconnect();
                            connections.Remove(connection);
                        }
                    }
                    catch { }

                }
                Thread.Sleep(1000);
            }
        }
    }
}
