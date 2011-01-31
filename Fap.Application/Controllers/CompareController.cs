using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Fap.Foundation;
using Fap.Domain.Entity;
using Fap.Application.ViewModels;
using System.Waf.Applications;
using System.Threading;
using Fap.Network.Entity;
using Fap.Network;
using Fap.Network.Services;
using Fap.Domain.Verbs;

namespace Fap.Application.Controllers
{
    public class CompareController
    {
        private SafeObservable<CompareNode> data = new SafeObservable<CompareNode>();
        private CompareViewModel viewModel;
        private Model model;
        private BufferService bs;
        private ConnectionService cs;

        private object sync = new object();
        private int requests = 0;

        public CompareController(IContainer c)
        {
            viewModel = c.Resolve<CompareViewModel>();
            model = c.Resolve<Model>();
            bs = c.Resolve<BufferService>();
            cs = c.Resolve<ConnectionService>();
        }

        public CompareViewModel Initalise()
        {
            viewModel.Run = new DelegateCommand(Run);
            viewModel.Data = data;
            viewModel.Status = "Status: click start to retrieve information.";
            return viewModel;
        }

        private void Run()
        {
            data.Clear();
            viewModel.EnableRun = false;
            viewModel.Status = "Status: Waiting for a response from " + model.Peers.Count + " clients..";
            foreach (var peer in model.Peers)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(RunAsync), peer);
            }
        }

        private void RunAsync(object o)
        {
            lock (sync)
            {
                requests++;
            }

            Node node = o as Node;
            if (null != node)
            {
                Client client = new Client(bs, cs);
                CompareVerb verb = new CompareVerb(model);

                if (client.Execute(verb, node))
                {
                    verb.Node.Nickname = node.Nickname;
                    verb.Node.Address = node.Location;
                    verb.Node.Status = "OK";
                    data.Add(verb.Node);
                }
                else if (verb.Status == 10)
                {
                    verb.Node.Nickname = node.Nickname;
                    verb.Node.Address = node.Location;
                    verb.Node.Status = "Denied";
                    data.Add(verb.Node);
                }
                else
                {
                    if (null != verb.Node)
                    {
                        verb.Node.Nickname = node.Nickname;
                        verb.Node.Address = node.Location;
                        verb.Node.Status = "Error";
                        data.Add(verb.Node);
                    }
                }
            }

            lock (sync)
            {
                requests--;
                viewModel.Status = "Status: Waiting for a response from " + requests + " clients..";
                if (requests == 0)
                {
                    viewModel.EnableRun = true;
                    viewModel.Status = "Status: All Information recieved, click start to refresh info (Note clients will cache information for 5 minutes).";
                }
            }
        }
    }
}
