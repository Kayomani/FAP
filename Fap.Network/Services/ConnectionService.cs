using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fap.Network.Entity;
using System.Net.Sockets;
using System.Net;
using Fap.Foundation;
using System.Threading;

namespace Fap.Network.Services
{
    public class ConnectionService
    {
        private SafeObservable<Session> sessions = new SafeObservable<Session>();
        private ReaderWriterLockSlim sync = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public SafeObservable<Session> Sessions
        {
            get { return sessions; }
        }

        public Session GetClientSession(Node rc)
        {
            lock (sync)
            {
                var session = sessions.Where(s => s.Host == rc && !s.InUse && !s.IsUpload).FirstOrDefault();
                if (null != session)
                {
                    session.InUse = true;
                    return session;
                }
            }
            //This might take a while so do it outside the lock
            var newSession = CreateSession(rc);
            if (null != newSession)
            {
                lock (sync)
                {
                    sessions.Add(newSession);
                }
            }
            return newSession;
        }

        public void FreeClientSession(Session s)
        {
            lock (sync)
            {
                if (!s.Socket.Connected)
                {
                    sessions.Remove(s);
                    s.Socket.Close();
                }
                else
                {
                    s.InUse = false;
                }
            }
        }

        public void RemoveClientSession(Session s)
        {
            if (sessions.Contains(s))
                sessions.Remove(s);
        }

     

      /*  public void RemoveServerSession(MemoryBuffer arg)
        {
            lock (sync)
            {
                //Check for existing session
                foreach (var session in sessions.ToList())
                {
                    if (session.Socket == arg.Socket)
                    {
                        sessions.Remove(session);
                        break;
                    }
                }
            }
        }

        public Session GetServerSession(MemoryBuffer arg)
        {
            lock (sync)
            {
                //Check for existing session
                foreach (var session in sessions.ToList())
                {
                    if (session.Socket == arg.Socket)
                        return session;
                }
                //Create new
                Session s = new Session();
                s.Socket = arg.Socket;
                s.IsUpload = true;
                s.InUse = true;
                //Try to find username
               */ /* IPEndPoint host = arg.Socket.RemoteEndPoint as IPEndPoint;
                 if (null != host)
                 {
                     string address = host.Address.ToString();
                     foreach (var client in model.Clients.ToList())
                     {
                         if (string.Equals(client.Host, address))
                         {
                             s.User = client.Nickname;
                             s.Host = client;
                             break;
                         }
                     }
                     if (string.IsNullOrEmpty(s.User))
                         s.User = address;
                 }
                 else if (null != arg.Socket.RemoteEndPoint)
                     s.User = arg.Socket.RemoteEndPoint.ToString();**//*
                sessions.Add(s);
                return s;
            }
        }*/

        private Session CreateSession(Node rc)
        {
            Socket s;
            try
            {
                s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                s.Connect(rc.Host, rc.Port);
                s.Blocking = true;
            }
            catch
            {
                return null;
            }
            Session sess = new Session();
            sess.Host = rc;
            sess.InUse = true;
            sess.Socket = s;
            sess.IsUpload = false;
            return sess;
        }
    }
}
