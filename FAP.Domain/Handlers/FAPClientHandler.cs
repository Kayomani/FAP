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
using Fap.Foundation;

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
                case "UPDATE":
                    return HandleUpdate(e,req);
                case "CHAT":
                    return HandleChat(e, req);
            }
            return false;
        }

        public void Start()
        {

        }

        private bool HandleChat(RequestEventArgs e, NetworkRequest req)
        {
            ChatVerb verb = new ChatVerb();
            verb.ReceiveResponse(req);

            model.Messages.Lock();
            model.Messages.AddRotate(verb.Nickname + ":" + verb.Message,50);

            model.Messages.Unlock();
            SendOk(e);
            SafeObservingCollectionManager.UpdateNowAsync();
            return true;
        }

        private bool HandleUpdate(RequestEventArgs e, NetworkRequest req)
        {
            UpdateVerb verb = new UpdateVerb();
            verb.ReceiveResponse(req);
            foreach (var node in verb.Nodes)
            {
                var search = model.Network.Nodes.Where(i => i.ID == node.ID).FirstOrDefault();
                if (search == null)
                {
                    //Dont allow partial updates to create clients.  Only full updates should contain the online flag.
                    if (node.ContainsKey("Online") && node.ContainsKey("Nickname") && node.ContainsKey("ID"))
                        model.Network.Nodes.Add(node);
                }
                else
                {
                    foreach (var param in node.Data)
                        search.SetData(param.Key, param.Value);
                }
            }
            SendOk(e);
            return true;
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
            SendOk(e);
            return true;
        }

        private bool HandleDisconnect(RequestEventArgs e)
        {
            SendOk(e);
            return true;
        }

        private void SendOk(RequestEventArgs e)
        {
            e.Response.Status = HttpStatusCode.OK;
            var generator = new ResponseWriter();
            generator.SendHeaders(e.Context, e.Response);
        }

    }
}
