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
using Fap.Network.Entity;
using System.Threading;

namespace Fap.Domain.Entity
{
    public class ServerUploadToken: IDisposable
    {
        private int position = 0;
        private bool canUpload = false;
        private Node remoteClient;

        private AutoResetEvent sync = new AutoResetEvent(true);

        public int Position
        {
            set
            {
                lock (sync)
                    position = value;
                sync.Set();
            }
            get
            {
                lock (sync)
                    return position;
            }
        }

        public bool CanUpload
        {
            set
            {
                lock (sync)
                    canUpload = value;
                sync.Set();
            }
            get
            {
                lock (sync)
                    return canUpload;
            }
        }

        public Node RemoteClient
        {
            set
            {
                lock (sync)
                    remoteClient = value;
            }
            get
            {
                lock (sync)
                    return remoteClient;
            }
        }

        public void Wait()
        {
            sync.WaitOne();
        }

        public void Dispose()
        {
            sync.Close();
        }
    }
}
