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
using Fap.Network.Entity;
using Fap.Foundation;
using NLog;

namespace Fap.Network.Services
{
    /// <summary>
    /// Manages converting the incoming datastream into requests
    /// </summary>
    public class FapConnectionHandler
    {
        private BufferService bufferManager;
        private Logger logger;

        public delegate FAPListenerRequestReturnStatus ReceiveRequest(Request r, Socket s);
        public event ReceiveRequest OnReceiveRequest;
        public delegate void Disconnect();
        public event Disconnect OnDisconnect;

        public FapConnectionHandler(BufferService b)
        {
            bufferManager = b;
            logger = LogManager.GetLogger("faplog");
        }

        public void Dispose()
        {


        }

        public void HandleConnection(Socket socket)
        {
            MemoryBuffer arg = null;
            ConnectionToken token = new ConnectionToken();
            FAPListenerRequestReturnStatus status = FAPListenerRequestReturnStatus.None;
            try
            {
                socket.SendBufferSize = BufferService.SmallBuffer;
                socket.ReceiveBufferSize = BufferService.SmallBuffer * 2;
                while (socket.Connected)
                {
                    arg = bufferManager.GetSmallArg();

                    int rx = socket.Receive(arg.Data);
                    if (rx == 0)
                    {
                        bufferManager.FreeArg(arg);
                        throw new Exception("Disconnected?");
                    }
                    else
                    {
                        arg.SetDataLocation(0, rx);
                        token.ReceiveData(arg);
                        while (token.ContainsCommand())
                        {
                            status = ProcessRequest(token.GetCommand(), socket);
                            if (status == FAPListenerRequestReturnStatus.ExternalHandler)
                            {
                                //Socket is now being handled externally to the listener
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.ErrorException("Connection handler exception", e);
            }
            if (null != OnDisconnect)
                OnDisconnect();
            try
            {
                if (status != FAPListenerRequestReturnStatus.Disposed)
                {
                    socket.ReceiveTimeout = 1000;
                    socket.SendTimeout = 1000;
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                bufferManager.FreeArg(arg);
                token.Dispose();
                socket = null;
            }
            catch (Exception e)
            {
                logger.ErrorException("Connection handler exception", e);
            }
        }

        public FAPListenerRequestReturnStatus ProcessRequest(string i, Socket s)
        {
            try
            {
                Request x = new Request();
                if (Mediator.Deserialize(i, out x))
                {
                    FAPListenerRequestReturnStatus state = FAPListenerRequestReturnStatus.None;
                    if (null != OnReceiveRequest)
                        state = OnReceiveRequest(x, s);
                    if (state == FAPListenerRequestReturnStatus.ExternalHandler)
                        return state;
                    if (state != FAPListenerRequestReturnStatus.Disposed && x.ConnectionClose)
                    {
                        s.Shutdown(SocketShutdown.Both);
                        s.Close();
                        return FAPListenerRequestReturnStatus.Disposed;
                    }
                }
                return FAPListenerRequestReturnStatus.None;
            }
            catch
            {
                return FAPListenerRequestReturnStatus.None;
            }
        }
    }
}
