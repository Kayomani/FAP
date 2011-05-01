﻿#region Copyright Kayomani 2011.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
/**
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or any 
    later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * */
#endregion
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
using FAP.Domain.Services;

namespace FAP.Domain.Handlers
{
    public class FAPClientHandler : IFAPHandler
    {
        private Model model;
        private ShareInfoService shareInfoService;
        private IConversationController chatController;

        public FAPClientHandler(Model m, ShareInfoService s, IConversationController c)
        {
            model = m;
            shareInfoService = s;
            chatController = c;
        }

        public bool Handle(RequestEventArgs e)
        {
            NetworkRequest req = Multiplexor.Decode(e.Request);
            LogManager.GetLogger("faplog").Info("Client rx: {0} p: {1} source: {2} overlord: {3}", req.Verb, req.Param,req.SourceID,req.OverlordID);
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
                case "COMPARE":
                    return HandleCompare(e, req);
                case "SEARCH":
                    return HandleSearch(e, req);
                case "CONVERSTATION":
                    return HandleConversation(e,req);
                case "BROWSE":
                    return HandleBrowse(e, req);
            }
            return false;
        }

        public void Start()
        {

        }

        private bool HandleBrowse(RequestEventArgs e, NetworkRequest req)
        {
            BrowseVerb verb = new BrowseVerb(model, shareInfoService);
            var result = verb.ProcessRequest(req);
            byte[] data = Encoding.Unicode.GetBytes(result.Data);
            var generator = new ResponseWriter();
            e.Response.ContentLength.Value = data.Length;
            generator.SendHeaders(e.Context, e.Response);
            e.Context.Stream.Write(data, 0, data.Length);
            e.Context.Stream.Flush();
            data = null;
            return true;

        }

        private bool HandleConversation(RequestEventArgs e, NetworkRequest req)
        {
            try
            {
                ConversationVerb verb = new ConversationVerb();
                verb.ProcessRequest(req);
                if (chatController.HandleMessage(verb.SourceID, verb.Nickname, verb.Message))
                {
                    SendOk(e);
                    return true;
                }
            }
            catch { }
            return false;
        }
        
        private bool HandleSearch(RequestEventArgs e, NetworkRequest req)
        {
            //We dont do this on a server..
            SearchVerb verb = new SearchVerb(shareInfoService);
            var result = verb.ProcessRequest(req);
            byte[] data = Encoding.Unicode.GetBytes(result.Data);
            var generator = new ResponseWriter();
            e.Response.ContentLength.Value = data.Length;
            generator.SendHeaders(e.Context, e.Response);
            e.Context.Stream.Write(data, 0, data.Length);
            e.Context.Stream.Flush();
            data = null;
            return true;
        }

        private bool HandleCompare(RequestEventArgs e, NetworkRequest req)
        {
            CompareVerb verb = new CompareVerb(model);

            var result = verb.ProcessRequest(req);
            byte[] data = Encoding.Unicode.GetBytes(result.Data);
            var generator = new ResponseWriter();
            e.Response.ContentLength.Value = data.Length;
            generator.SendHeaders(e.Context, e.Response);
            e.Context.Stream.Write(data, 0, data.Length);
            e.Context.Stream.Flush();
            data = null;

            return true;
        }

        private bool HandleChat(RequestEventArgs e, NetworkRequest req)
        {
            ChatVerb verb = new ChatVerb();
            verb.ReceiveResponse(req);
            model.Messages.AddRotate(verb.Nickname + ":" + verb.Message,50);
            SendOk(e);
            SafeObservingCollectionManager.UpdateNowAsync();
            return true;
        }

        private bool HandleUpdate(RequestEventArgs e, NetworkRequest req)
        {
            if (req.AuthKey == model.Network.Overlord.Secret)
            {

                UpdateVerb verb = new UpdateVerb();
                verb.ProcessRequest(req);
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
            return false;
        }

        private bool HandleInfo(RequestEventArgs e)
        {
            e.Response.Status = HttpStatusCode.OK;
            InfoVerb verb = new InfoVerb();
            verb.Node = model.LocalNode;
            var result = verb.CreateRequest();
            byte[] data = Encoding.Unicode.GetBytes(result.Data);
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
