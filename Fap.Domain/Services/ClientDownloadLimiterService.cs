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
using Fap.Domain.Entity;
using System.Threading;
using Fap.Network.Entity;

namespace Fap.Domain.Services
{
   /* public class ClientDownloadLimiterService
    {
        private List<DownloadTokenStore> downloadList = new List<DownloadTokenStore>();
        private Model model;


        public ClientDownloadLimiterService(Model model)
        {
            this.model = model;
        }

        public bool GetDownloadToken(RemoteClient rc)
        {
            lock (downloadList)
            {
                var token = downloadList.Where(t => t.Client == rc).FirstOrDefault();
                if (token == null)
                {
                    if (CheckGlobalDownloadLimit())
                    {
                        token = new DownloadTokenStore() { Client = rc, Downloads = 1 };
                        downloadList.Add(token);
                        return true;
                    }
                }
                else
                {
                    if (token.Downloads < model.MaxDownloadsPerUser)
                    {
                        if (CheckGlobalDownloadLimit())
                        {
                            token.Downloads++;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool CheckGlobalDownloadLimit()
        {
            return model.MaxDownloads > downloadList.Sum(i => i.Downloads);
        }

        public void FreeToken(RemoteClient rc)
        {
            lock (downloadList)
            {
                var token = downloadList.Where(t => t.Client == rc).FirstOrDefault();
                if (token != null)
                {
                    token.Downloads--;
                }
            }
        }
        
        
    }*/
}
