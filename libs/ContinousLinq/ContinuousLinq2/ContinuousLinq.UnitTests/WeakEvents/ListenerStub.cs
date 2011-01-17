using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ContinuousLinq.WeakEvents;
using System.ComponentModel;
using System.Threading;

namespace ContinuousLinq.UnitTests.WeakEvents
{
    public class ListenerStub
    {
        public int CallCount { get; set; }
        public object Sender { get; set; }
        public PropertyChangedEventArgs Args { get; set; }

        public void OnPropertyChanged(object sender, PropertyChangedEventArgs ea)
        {
            this.CallCount++;
            this.Sender = sender;
            this.Args = ea;
        }
    }
}
