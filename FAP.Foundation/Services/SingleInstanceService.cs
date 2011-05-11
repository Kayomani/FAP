using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Fap.Foundation.Services
{
    public class SingleInstanceService
    {
        private Mutex mutex;
        private bool hasLock = false;

        public SingleInstanceService(string name)
        {
            mutex = new Mutex(false, name);
            mutex.WaitOne(0,false);
        }

        public void Dispose()
        {
            if (hasLock)
                mutex.ReleaseMutex();
            mutex.Close();
            mutex = null;
        }

        public bool GetLock()
        {
            if (hasLock)
                return hasLock;
            hasLock = mutex.WaitOne(0, false);
            return hasLock;
        }

        public bool HasLock
        {
            get { return hasLock; }
        }

        public void ReleaseLock()
        {
            if (!hasLock)
                throw new Exception("Cannot release a lock when we dont have one.");
            mutex.ReleaseMutex();
        }
    }
}
