using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Odyssey.Ribbon.EventArgs
{
    public class SelectedTabIndexChangedEvent : RoutedPropertyChangedEventArgs<int>
    {
        public SelectedTabIndexChangedEvent(int oldValue, int newValue, RoutedEvent routedEvent)
            : base(oldValue, newValue, routedEvent)
        {
        }
    }

   // public delegate void SelectedTabIndexChangedHandler(object sender, SelectedTabIndexChangedEvent e);
}
