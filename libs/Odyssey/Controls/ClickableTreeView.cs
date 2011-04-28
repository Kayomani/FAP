using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Odyssey.Controls
{
    public class ClickableTreeView:TreeView
    {

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is Separator) || (item is TreeViewItem);
        }

        protected override System.Windows.DependencyObject GetContainerForItemOverride()
        {
            return new ClickableTreeViewItem();
        }
    }
}
