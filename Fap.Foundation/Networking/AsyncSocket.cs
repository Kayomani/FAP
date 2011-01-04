using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Fap.Foundation.Networking
{
   /* public class AsyncSocket
    {
        private Socket socket;
        private int connections;
        private NetworkBufferManager bufferManager;

        public delegate void ReceiveData(byte[] data, int length);
        public event ReceiveData OnReceiveData;

        public AsyncSocket(int connections, NetworkBufferManager manager)
        {
            this.connections = connections;
            bufferManager = manager;
        }

        public void Listen(int port, ProtocolType protocol)
        {
            switch (protocol)
            {
                case ProtocolType.Tcp:
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.DontFragment = false;
                    break;
                case ProtocolType.Udp:
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    break;
                default:
                    throw new Exception("Unsupported protocol");
            }

           
            socket.ReceiveBufferSize = bufferManager.Buffer;
            socket.SendBufferSize = bufferManager.Buffer;
           
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
            socket.Bind(ep);
            socket.Listen(connections);
        }

        public void Connect(IPAddress endpoint, int port)
        {
            socket.Connect(endpoint, port);
        }



        public void StartListen()
        {
            //Accept connections
            OnAcceptCompleted(null, null);
        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (null != e)
            {
                e.Completed -= new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
                if (e.AcceptSocket.Connected)
                {
                  //  e.UserToken = new Session(e);
                    e.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);
                    if (e.BytesTransferred != 0 || !e.AcceptSocket.ReceiveAsync(e))
                        OnReceiveCompleted(sender, e);
                }
                else
                {
                    bufferManager.FreeArg(e);
                }
            }

            var arg = bufferManager.GetArg();
            arg.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            if (!socket.AcceptAsync(arg))
                OnAcceptCompleted(null, arg);
        }

        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            // Check if the remote host closed the connection.
            if (e.BytesTransferred > 0)
            {
                if (e.SocketError == SocketError.Success)
                {
                   // Session token = e.UserToken as Session;
                    //token.SetData(e);

                    if (e.AcceptSocket.Available == 0)
                    {
                        e.Completed -= new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);
                       // if(null!=OnReceiveData)
                       //     OnReceiveData(
                        //mediator.ParseCommand(token, e);
                        return;
                    }
                    else if (!e.AcceptSocket.ReceiveAsync(e))
                    {
                        // Read the next block of data sent by client.
                        this.OnReceiveCompleted(sender, e);
                    }
                }
                else
                {
                    this.CloseClientSocket(e);
                }
            }
            else
            {
                this.CloseClientSocket(e);
            }
        }

        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            if (null != e.AcceptSocket && e.AcceptSocket.Connected)
                e.AcceptSocket.Close();
            e.Completed -= new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);
            e.Completed -= new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            bufferManager.FreeArg(e);
        }


        public class AsyncSessionToken
        {
         


        }

    }*/
}
