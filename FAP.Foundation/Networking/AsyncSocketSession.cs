﻿#region Copyright Kayomani 2010.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
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

namespace Fap.Foundation.Networking
{
    public class AsyncSocketSession
    {

        private StringBuilder sb;
        private Int32 currentIndex;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="connection">Socket to accept incoming data.</param>
        /// <param name="bufferSize">Buffer size for accepted data.</param>
        public AsyncSocketSession()
        {
            this.sb = new StringBuilder();
        }

        public string Data { get { return sb.ToString(); } }

        /// <summary>
        /// Process data received from the client.
        /// </summary>
        /// <param name="args">SocketAsyncEventArgs used in the operation.</param>
        public void ProcessData(SocketAsyncEventArgs args)
        {
            // Get the message received from the client.
            String received = this.sb.ToString();

            Byte[] sendBuffer = Encoding.ASCII.GetBytes(received);
            args.SetBuffer(sendBuffer, 0, sendBuffer.Length);

            // Clear StringBuffer, so it can receive more data from a keep-alive connection client.
            sb.Length = 0;
            this.currentIndex = 0;
            sendBuffer = null;
        }

        /// <summary>
        /// Set data received from the client.
        /// </summary>
        /// <param name="args">SocketAsyncEventArgs used in the operation.</param>
        public void ReceiveData(SocketAsyncEventArgs args)
        {
            Int32 count = args.BytesTransferred;
            
            sb.Append(Encoding.ASCII.GetString(args.Buffer, args.Offset, args.BytesTransferred));
            this.currentIndex += count;
        }
    }
}
