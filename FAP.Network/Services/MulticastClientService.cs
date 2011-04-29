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
using System.Net.Sockets;
using System.Net;
using NLog;
using System.Threading;

namespace FAP.Network.Services
{
    public class MulticastClientService : MulticastCommon
    {
        private Socket listenSocket;
        private Logger logger;

        private byte[] buffer = new byte[50000];

        public delegate void MultiCastRX(string cmd);
        public event MultiCastRX OnMultiCastRX;

        public MulticastClientService()
        {
            logger = LogManager.GetLogger("faplog");
        }

        private void ConnectListen()
        {
            if (null == listenSocket)
            {
                listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                listenSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(broadcastAddress, IPAddress.Any));
                listenSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
                //  listenSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface,);
                listenSocket.Bind(new IPEndPoint(IPAddress.Any, broadcastPort));

                listenSocket.ReceiveBufferSize = buffer.Length;
                listenSocket.SendBufferSize = buffer.Length;

                ThreadPool.QueueUserWorkItem(new WaitCallback(Process));
                //  listenSocket.Connect(broadcastAddress, broadcastPort);
            }
        }

        private void Process(object o)
        {
            while (true)
            {
                int length = listenSocket.Receive(buffer);
                if (null != OnMultiCastRX)
                    OnMultiCastRX(Encoding.Unicode.GetString(buffer, 0, length));
            }
        }

        public void StartListener()
        {
            ConnectListen();
        }
    }
}
