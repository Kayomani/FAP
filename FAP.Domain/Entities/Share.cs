#region Copyright Kayomani 2011.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.

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
using Fap.Foundation.Services;

namespace FAP.Domain.Entities
{
    public class Share : BaseEntity
    {
        private long fileCount;
        private string id;
        private string name;
        private string path;
        private DateTime refresh;
        private long size;
        private string status;

        public Share()
        {
            id = IDService.CreateID();
        }

        public DateTime LastRefresh
        {
            set
            {
                refresh = value;
                NotifyChange("LastRefresh");
            }
            get { return refresh; }
        }

        public string Path
        {
            set
            {
                path = value;
                NotifyChange("Path");
            }
            get { return path; }
        }

        public string Name
        {
            set
            {
                name = value;
                NotifyChange("Name");
            }
            get { return name; }
        }

        public string ID
        {
            set
            {
                id = value;
                NotifyChange("ID");
            }
            get { return id; }
        }

        public long Size
        {
            set
            {
                size = value;
                NotifyChange("Size");
            }
            get { return size; }
        }

        public long FileCount
        {
            set
            {
                fileCount = value;
                NotifyChange("FileCount");
            }
            get { return fileCount; }
        }

        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                NotifyChange("Status");
            }
        }
    }
}