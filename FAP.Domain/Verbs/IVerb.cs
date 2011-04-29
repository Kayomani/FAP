using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Domain.Entities;
using FAP.Network.Entities;

namespace FAP.Domain.Verbs
{
    public interface IVerb
    {
        NetworkRequest CreateRequest();
        NetworkRequest ProcessRequest(NetworkRequest r);
        bool ReceiveResponse(NetworkRequest r);
    }
}
