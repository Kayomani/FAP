using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAP.Domain.Entities.FileSystem
{
    [Serializable]
    public class File
    {
        public string Name { set; get; }
        public long Size { set; get; }
        public long LastModified { set; get; }
    }
}
