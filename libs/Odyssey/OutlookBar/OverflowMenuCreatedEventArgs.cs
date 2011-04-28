using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Odyssey.Controls
{
    public class OverflowMenuCreatedEventArgs:EventArgs
    {
        public OverflowMenuCreatedEventArgs(Collection<object> menuItems)
            : base()
        {
            this.MenuItems = menuItems;
        }

        public Collection<object> MenuItems { get; private set; }
    }
}
