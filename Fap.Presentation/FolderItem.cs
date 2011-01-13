using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Fap.Presentation
{
    public class FolderItem
    {
        public string Folder { get; set; }
        public ImageSource Image { get; set; }

        public FolderItem()
            : base()
        {
            ImageSourceConverter isc = new ImageSourceConverter();
            Image = isc.ConvertFrom("openfolderHS.png") as ImageSource;
        }
    }
}
