using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fap.Foundation;

namespace FAP.Domain.Entities
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
