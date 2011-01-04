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
