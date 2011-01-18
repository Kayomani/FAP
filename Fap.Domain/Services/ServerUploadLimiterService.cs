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
using System.Threading;
using Fap.Domain.Entity;
using Fap.Network.Entity;

namespace Fap.Domain.Services
{
    /*public class ServerUploadLimiterService
    {
        List<ServerUploadToken> tokenlist = new List<ServerUploadToken>();
        Queue<ServerUploadToken> recycledList = new Queue<ServerUploadToken>();
        private Model model;

        public ServerUploadLimiterService(Model model)
        {
            this.model = model;
        }

        public bool RequestUploadToken(out ServerUploadToken token, RemoteClient rc)
        {
            bool startNow = true;
            lock (tokenlist)
            {
                if (recycledList.Count > 0)
                {
                    token = recycledList.Dequeue();
                }
                else
                {
                    token = new ServerUploadToken();
               }

                tokenlist.Add(token);
                token.Position = tokenlist.IndexOf(token);
                //If we have reached the global uploads then pause the download
                if (token.Position > model.MaxUploads)
                {
                 //   token.Wait();
                    startNow = false;
                }
                else if (tokenlist.Where(i => i.RemoteClient == rc).Count() >= model.MaxUploadsPerUser)
                {
                    //We have reached the max uploads for this perticular user
                 //   token.Wait();
                    startNow = false;
                }
            }
            return startNow;
        }

        public void FreeToken(ServerUploadToken token)
        {
            lock (tokenlist)
            {
                RemoteClient rc = token.RemoteClient;

                //Recycle token
                token.AllowedToUpload = false;
                token.RemoteClient = null;
                token.AllowedToLock = true;
               // while (token.HasLock)
                //    token.Release();
                recycledList.Enqueue(token);
                
                tokenlist.Remove(token);
                
                //Update positions
                for (int i = 0; i < tokenlist.Count; i++)
                {
                    tokenlist[i].Position = i;
                }

                // Signal updates
                for (int i = 0; i < tokenlist.Count; i++)
                {
                    tokenlist[i].AllowedToLock = false;
                    if (tokenlist[i].HasLock)
                    {
                        tokenlist[i].Release();
                    }
                    tokenlist[i].AllowedToLock = true;
                }

                //Trigger next download if there is one
                if (tokenlist.Count > 0)
                {
                    for (int i = 0; i < tokenlist.Count; i++)
                    {
                        //Make sure we don't exceed the per user limit
                        var count = tokenlist.Where(t => t.RemoteClient == rc).Count();
                        if ((count + 1) <= model.MaxUploadsPerUser)
                        {
                            if (!tokenlist[i].AllowedToUpload)
                            {
                                tokenlist[i].AllowedToUpload = true;
                                if(tokenlist[i].HasLock)
                                  tokenlist[i].Release();
                            }
                            return;
                        }
                    }
                }
            }
        }



        public class ServerUploadToken
        {
            private Semaphore semaphore;
            private int position;
            private int lockCount = 0;
            private bool allowedToLock =true;

            public ServerUploadToken()
            {
                semaphore = new Semaphore(0, 1);
            }

            public bool AllowedToUpload { set; get; }
            public RemoteClient RemoteClient { set; get; }

            public bool AllowedToLock
            {
                get
                {
                    lock (semaphore)
                        return allowedToLock;
                }
                set
                {
                    lock (semaphore)
                        allowedToLock = value;
                }
            }

            public int Position
            {
                set
                {
                    lock (semaphore)
                        position = value;
                }
                get
                {
                    lock (semaphore)
                        return position;
                }
            }

            public bool HasLock
            {
                get
                {
                    lock (semaphore)
                        return lockCount == 1;
                }
            }

            public void Dispose()
            {
                semaphore.Close();
            }

            public void Wait()
            {
                if (lockCount != 0)
                {

                }
               /* lock (semaphore)
                    lockCount++;
                semaphore.WaitOne();
               */
    /*
}

public void Release()
{
   if (lockCount != 1)
   {

   }
  /* lock (semaphore)
       lockCount--;
   semaphore.Release();
}
}
}*/
}
