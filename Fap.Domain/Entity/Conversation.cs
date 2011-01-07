using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fap.Network.Entity;
using Fap.Foundation;

namespace Fap.Domain.Entity
{
    public class Conversation
    {
        public Node OtherParty { set; get; }
        public SafeObservable<string> Messages { set; get; }

        public Conversation()
        {
            Messages = new SafeObservable<string>();
        }
    }
}
