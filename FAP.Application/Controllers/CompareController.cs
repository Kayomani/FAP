#region Copyright Kayomani 2011.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
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
using Fap.Foundation;
using FAP.Application.ViewModels;
using FAP.Domain.Entities;
using Autofac;
using System.Waf.Applications;
using FAP.Domain;
using System.Threading;
using FAP.Domain.Verbs;

namespace FAP.Application.Controllers
{
    public class CompareController
    {
        private SafeObservable<CompareNode> data = new SafeObservable<CompareNode>();
        private CompareViewModel viewModel;
        private Model model;

        private object sync = new object();
        private int requests = 0;

        public CompareController(IContainer c)
        {
            viewModel = c.Resolve<CompareViewModel>();
            model = c.Resolve<Model>();
        }

        public CompareViewModel ViewModel
        {
            get { return viewModel; }
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
            var peerlist = model.Network.Nodes.ToList();

            if (peerlist.Count == 0)
            {
                viewModel.Status = "Please wait until your connected to a network prior to running the compare tool";
            }
            else
            {
                viewModel.Status = "Status: Waiting for a response from " + model.Network.Nodes.Count + " peers..";
                foreach (var peer in peerlist)
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
                Client client = new Client(model.LocalNode);
                CompareVerb verb = new CompareVerb(model);

                if (client.Execute(verb, node))
                {
                    if (!verb.Allowed)
                    {
                        verb.Node.Nickname = node.Nickname;
                        verb.Node.Location = node.Location;
                        verb.Node.Status = "Denied";
                        data.Add(verb.Node);
                    }
                    else
                    {
                        verb.Node.Nickname = node.Nickname;
                        verb.Node.Location = node.Location;
                        verb.Node.Status = "OK";
                        data.Add(verb.Node);
                    }
                }
                else
                {
                    verb.Node = new CompareNode();
                    verb.Node.Nickname = node.Nickname;
                    verb.Node.Location = node.Location;
                    verb.Node.Status = "Error";
                    data.Add(verb.Node);
                }
            }

            lock (sync)
            {
                requests--;
                viewModel.Status = "Status: Waiting for a response from " + requests + " peers..";
                if (requests == 0)
                {
                    viewModel.EnableRun = true;
                    viewModel.Status = "Status: All Information recieved, click start to refresh info (Note clients will cache information for 5 minutes).";
                }
            }
        }
    }
}
