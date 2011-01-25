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
        private object sync = new object();

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


        public List<Session> GetAndClearStaleSessions()
        {
            lock (sync)
            {
                var staleSessions = sessions.Where(s => !s.InUse && s.Stale).ToList();
                foreach (var session in staleSessions)
                {
                    session.InUse = true;
                    sessions.Remove(session);
                }
                return staleSessions;
            }
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
            lock (sync)
            {
                if (sessions.Contains(s))
                    sessions.Remove(s);
            }
        }

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
