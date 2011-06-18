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
using System.IO;
using Fap.Foundation;
using Newtonsoft.Json;
using NLog;

namespace FAP.Domain.Entities
{
    [Serializable]
    public class DownloadQueue : BaseEntity
    {
        private readonly SafeObservedCollection<DownloadRequest> queue = new SafeObservedCollection<DownloadRequest>();

        private readonly string saveLocation = "Queue.cfg";
        private readonly object sync = new object();

        public SafeObservedCollection<DownloadRequest> List
        {
            get { return queue; }
        }

        public void Save()
        {
            lock (sync)
                SafeSave(this, saveLocation, Formatting.None);
        }

        public void Load()
        {
            lock (sync)
            {
                try
                {
                    queue.Clear();
                    if (File.Exists(DATA_FOLDER + saveLocation))
                    {
                        var saved = SafeLoad<DownloadQueue>(saveLocation);
                        queue.AddRange(saved.List.ToList());
                    }
                }
                catch (Exception e)
                {
                    LogManager.GetLogger("faplog").WarnException("Failed to read download queue", e);
                }
            }
        }
    }
}