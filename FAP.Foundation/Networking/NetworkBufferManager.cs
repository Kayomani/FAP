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

namespace Fap.Foundation.Networking
{
  /*  public class NetworkBufferManager
    {
        Stack<SocketAsyncEventArgs> pool = new Stack<SocketAsyncEventArgs>();

        public int Buffer { set; get; }

        public NetworkBufferManager(int count, int buffer)
        {
            Buffer = buffer;//5mb
            for(int i=0;i<count;i++)
            {
                SocketAsyncEventArgs a = new SocketAsyncEventArgs();
                a.SetBuffer(new byte[Buffer], 0, Buffer);
                pool.Push(a);

            }

        }

        public SocketAsyncEventArgs GetArg()
        {
            lock (pool)
            {
                if (pool.Count > 0)
                {
                    return pool.Pop();
                }
            }
            SocketAsyncEventArgs a = new SocketAsyncEventArgs();
            a.SetBuffer(new byte[Buffer], 0, Buffer);
            return a;
        }

        public void FreeArg(SocketAsyncEventArgs a)
        {
            lock (pool)
            {
                a.AcceptSocket = null;
                pool.Push(a);
            }
        }
    }*/
}
