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
using System.Net.Sockets;
using System.Net;
using Fap.Network.Entity;
using System.Threading;
using Fap.Foundation;
using NLog;

namespace Fap.Network.Services
{
    public class FAPListener
    {
        private TcpListener listener;
        public delegate FAPListenerRequestReturnStatus ReceiveRequest(Request r, Socket s);
        public event ReceiveRequest OnReceiveRequest;
        //Services
        private BufferService bufferManager;
        private Logger logger;
        private ConnectionService connectionService;
        //Handler
        private FapConnectionHandler handler;

        public FAPListener(BufferService b, ConnectionService c)
        {
            bufferManager = b;
            logger = LogManager.GetLogger("faplog");
            connectionService = c;
            handler = new FapConnectionHandler(b);
            handler.OnReceiveRequest += new FapConnectionHandler.ReceiveRequest(handler_OnReceiveRequest);
        }

        FAPListenerRequestReturnStatus handler_OnReceiveRequest(Request r, Socket s)
        {
            if (null != OnReceiveRequest)
                return OnReceiveRequest(r, s);
            return FAPListenerRequestReturnStatus.None;
        }

        public int Start(IPAddress address, int port)
        {
            if (null != listener)
                throw new Exception("Already listening");

            bool trybind = true;
            int inport = port;
            do
            {
                try
                {
                    //Try to bind
                    listener = new TcpListener(address, port);
                    listener.Start();
                    trybind = false;
                }
                catch
                {
                    //Try again
                    port++;
                    if (inport + 100 < port)
                    {
                        throw new Exception("Could to bind to " + address.ToString());
                    }
                }
            }
            while (trybind);
            listener.BeginAcceptSocket(new AsyncCallback(handleClient), null);
            return port;
        }

        public void Stop()
        {
            listener.Stop();
        }

        private void handleClient(IAsyncResult result)
        {
            try
            {
                listener.BeginAcceptSocket(new AsyncCallback(handleClient), null);
                Socket socket = listener.EndAcceptSocket(result);
                socket.SendBufferSize = BufferService.SmallBuffer;
                socket.ReceiveBufferSize = BufferService.SmallBuffer * 2;
                socket.ReceiveTimeout = 300 * 1000;
                socket.Blocking = true;

                handler.HandleConnection(socket);
            }
            catch (Exception e)
            {
                logger.ErrorException("Listener exception",e);
            }
        }
    }
}
