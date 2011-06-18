using System;
using System.Threading;

namespace FAP.Domain.Entities
{
    /// <summary>
    /// A token representing an upload slot.  When the position is 0 then the upload can be sent.
    /// </summary>
    public class ServerUploadToken : IDisposable
    {
        private readonly AutoResetEvent sync = new AutoResetEvent(true);
        private int globalQueuePosition;
        private string remoteEndPoint;

        public int GlobalQueuePosition
        {
            get
            {
                lock (sync)
                    return globalQueuePosition;
            }
            set
            {
                lock (sync)
                    globalQueuePosition = value;
                sync.Set();
            }
        }

        public string RemoteEndPoint
        {
            set
            {
                lock (sync)
                    remoteEndPoint = value;
            }
            get
            {
                lock (sync)
                    return remoteEndPoint;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            sync.Close();
        }

        #endregion

        public void Wait()
        {
            sync.WaitOne();
        }

        public void WaitTimeout()
        {
            sync.WaitOne(5000);
        }
    }
}