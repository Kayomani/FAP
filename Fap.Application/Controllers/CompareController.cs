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
            foreach (var peer in model.Peers)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(RunAsync), peer);
            }
        }

        private void RunAsync(object o)
        {
            Node node = o as Node;
            if (null != node)
            {
                Client client = new Client(bs, cs);
                CompareVerb verb = new CompareVerb();

                if (client.Execute(verb, node))
                {
                    verb.Node.Nickname = node.Nickname;
                    data.Add(verb.Node);
                }
            }
        }
    }
}
