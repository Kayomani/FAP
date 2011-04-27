using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HttpServer;

namespace FAP.Domain.Handlers
{
    public interface IFAPHandler
    {
        bool Handle(RequestEventArgs e);
    }
}
