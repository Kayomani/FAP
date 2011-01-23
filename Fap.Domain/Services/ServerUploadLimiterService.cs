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
    public class ServerUploadLimiterService
    {
        List<ServerUploadToken> tokenlist = new List<ServerUploadToken>();
        Queue<ServerUploadToken> recycledList = new Queue<ServerUploadToken>();
        private Model model;

        public ServerUploadLimiterService(Model model)
        {
            this.model = model;
        }

        public ServerUploadToken RequestUploadToken(Node node)
        {
            bool startNow = true;
            ServerUploadToken token;
            lock (tokenlist)
            {
                //Create token
                if (recycledList.Count > 0)
                    token = recycledList.Dequeue();
                else
                    token = new ServerUploadToken();

                tokenlist.Add(token);
                token.RemoteClient = node;

                int totalDownloads = tokenlist.Where(t => t.CanUpload).Count();


                //If we have reached the global uploads then pause the download
                if (totalDownloads > model.MaxUploads)
                    startNow = false;
                else if (tokenlist.Where(i => i.RemoteClient == node).Count() > model.MaxUploadsPerUser)
                {
                    //We have reached the max uploads for this perticular user
                    startNow = false;
                }

                token.CanUpload = startNow;
                if (startNow)
                    token.Position = 0;
                else
                    token.Position = tokenlist.Where(t => !t.CanUpload).Count() + 1;
            }
            return token;
        }

        public void FreeToken(ServerUploadToken token)
        {
            lock (tokenlist)
            {
                Node node = token.RemoteClient;

                //Recycle token
                token.CanUpload = false;
                token.RemoteClient = null;
                token.Position = 0;
                recycledList.Enqueue(token);

                tokenlist.Remove(token);

                //Update positions
                var queuedTokens = tokenlist.Where(t => !t.CanUpload).ToList();
                for (int i = 0; i < queuedTokens.Count(); i++)
                {
                    queuedTokens[i].Position = i;
                    int currentCLientDownloads = tokenlist.Where(t => t.RemoteClient == queuedTokens[i].RemoteClient).Count();
                    if (currentCLientDownloads < model.MaxDownloadsPerUser)
                    {
                        queuedTokens[i].CanUpload = true;
                        break;
                    }
                }
            }
        }
    }
}
