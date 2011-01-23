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
using Fap.Foundation.Logging;

namespace Fap.Network.Services
{
    public class FAPListener
    {
        private TcpListener listener;
        public delegate bool ReceiveRequest(Request r, Socket s);
        public event ReceiveRequest OnReceiveRequest;

        BufferService bufferManager;
        Logger logger;
        ConnectionService connectionService;

        public FAPListener(BufferService b, Logger l, ConnectionService c)
        {
            bufferManager = b;
            logger = l;
            connectionService = c;
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
            Socket socket = null;
            MemoryBuffer arg = null;
            ConnectionToken token = new ConnectionToken();
            bool disposed = false;
            try
            {
                listener.BeginAcceptSocket(new AsyncCallback(handleClient), null);
                socket = listener.EndAcceptSocket(result);
                socket.SendBufferSize = BufferService.SmallBuffer;
                socket.ReceiveBufferSize = BufferService.SmallBuffer*2;
                socket.ReceiveTimeout = 300 * 1000;
                socket.Blocking = true;

                while (socket.Connected)
                {
                    try
                    {
                        arg = bufferManager.GetSmallArg();
                        int rx = socket.Receive(arg.Data);
                        arg.SetDataLocation(0, rx); 
                        token.ReceiveData(arg);
                        while (token.ContainsCommand())
                        {
                            disposed = ProcessRequest(token.GetCommand(), socket);
                        }
                    }
                    catch { }
                }
            }
            catch (Exception e)
            {
                logger.LogException(e);
            }
            try
            {
                if (!disposed)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    //Free the associated session
                    token.Dispose();
                    //connectionService.RemoveServerSession(arg);
                    bufferManager.FreeArg(arg);
                }
                else
                {
                   // connectionService.RemoveServerSession(arg);
                    bufferManager.FreeArg(arg);
                }
            }
            catch (Exception e)
            {
                logger.LogException(e);
            }
        }

        public bool ProcessRequest(string i, Socket s)
        {
            try
            {
                Request x = new Request();
                if (Mediator.Deserialize(i, out x))
                {
                    if (null != OnReceiveRequest)
                        return OnReceiveRequest(x,s);
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
