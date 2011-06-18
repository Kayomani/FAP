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

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NLog;

namespace FAP.Network.Services
{
    public class MulticastClientService : MulticastCommon
    {
        #region Delegates

        public delegate void MultiCastRX(string cmd);

        #endregion

        private readonly byte[] buffer = new byte[50000];

        private Socket listenSocket;
        private Logger logger;

        public MulticastClientService()
        {
            logger = LogManager.GetLogger("faplog");
        }

        public event MultiCastRX OnMultiCastRX;

        private void ConnectListen()
        {
            if (null == listenSocket)
            {
                listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                listenSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
                                             new MulticastOption(broadcastAddress, IPAddress.Any));
                listenSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
                //  listenSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface,);
                listenSocket.Bind(new IPEndPoint(IPAddress.Any, broadcastPort));

                listenSocket.ReceiveBufferSize = buffer.Length;
                listenSocket.SendBufferSize = buffer.Length;

                ThreadPool.QueueUserWorkItem(Process);
                //  listenSocket.Connect(broadcastAddress, broadcastPort);
            }
        }

        private void Process(object o)
        {
            while (true)
            {
                int length = listenSocket.Receive(buffer);
                if (null != OnMultiCastRX)
                    OnMultiCastRX(Encoding.UTF8.GetString(buffer, 0, length));
            }
        }

        public void StartListener()
        {
            ConnectListen();
        }
    }
}