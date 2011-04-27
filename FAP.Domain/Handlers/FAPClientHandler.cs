using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HttpServer;
using System.Net;
using HttpServer.Messages;
using FAP.Domain.Entities;
using FAP.Domain.Verbs;
using FAP.Network.Entities;
using FAP.Network;
using NLog;

namespace FAP.Domain.Handlers
{
    public class FAPClientHandler : IFAPHandler
    {
        private Model model;

        public FAPClientHandler(Model m)
        {
            model = m;
        }

        public bool Handle(RequestEventArgs e)
        {
            NetworkRequest req = Multiplexor.Decode(e.Request);
            LogManager.GetLogger("faplog").Info("Client rx: {0} {1} ", req.Verb, req.Param);
            switch (req.Verb)
            {
                case "INFO":
                    return HandleInfo(e);
                case "NOOP":
                    return HandleNOOP(e);
                case "DISCONNECT":
                    return HandleDisconnect(e);
            }
            return false;
        }

        public void Start()
        {

        }

        private bool HandleInfo(RequestEventArgs e)
        {
            e.Response.Status = HttpStatusCode.OK;
            InfoVerb verb = new InfoVerb();
            verb.Node = model.LocalNode;
            var result = verb.CreateRequest();
            byte[] data = Encoding.ASCII.GetBytes(result.Data);
            var generator = new ResponseWriter();
            e.Response.ContentLength.Value = data.Length;
            generator.SendHeaders(e.Context, e.Response);
            e.Context.Stream.Write(data, 0, data.Length);
            e.Context.Stream.Flush();
            return true;
        }

        private bool HandleNOOP(RequestEventArgs e)
        {
            e.Response.Status = HttpStatusCode.OK;
            var generator = new ResponseWriter();
            generator.SendHeaders(e.Context, e.Response);
            return true;
        }

        private bool HandleDisconnect(RequestEventArgs e)
        {

            return false;
        }

    }
}
