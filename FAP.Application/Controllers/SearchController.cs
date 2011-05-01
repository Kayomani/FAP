using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Application.ViewModels;
using Autofac;
using FAP.Application.ViewModel;
using System.Waf.Applications;
using FAP.Domain.Verbs;
using System.Threading;
using FAP.Domain.Entities;
using FAP.Domain;
using Fap.Foundation;
using FAP.Domain.Net;

namespace FAP.Application.Controllers
{
    public class SearchController
    {
        private SearchViewModel viewModel;
        private IContainer container;
        private Model model;
        private SafeObservedCollection<SearchResult> results = new SafeObservedCollection<SearchResult>();

        private int outstandingrequests = 0;
        private object sync = new object();
        private long startTime=0;

        public SearchController(IContainer c,Model m)
        {
            container = c;
            model = m;
        }

        public SearchViewModel ViewModel
        {
            get { return viewModel; }
        }

        public void Initalize()
        {
            if (null == viewModel)
            {
                viewModel = container.Resolve<SearchViewModel>();
                viewModel.Search = new DelegateCommand(Search);
                viewModel.Results = new SafeObservingCollection<SearchResult>(results);
                viewModel.Download = new DelegateCommand(Download);
                viewModel.ViewShare = new DelegateCommand(ViewShare);
                viewModel.Reset = new DelegateCommand(Reset);
            }
        }

        private void Reset()
        {
            results.Clear();
            viewModel.LowerStatusMessage = string.Empty;
            viewModel.UpperStatusMessage = string.Empty;
            viewModel.SearchString = string.Empty;
            viewModel.ModifiedDate = null;
            viewModel.ModifiedSearchType = "Any";
            viewModel.SizeSearchType = "Any Size";
            viewModel.SizeText = null;
        }

        private void ViewShare(object o)
        {

        }

        private void Download(object o)
        {
            System.Collections.ObjectModel.ObservableCollection<object> i = o as System.Collections.ObjectModel.ObservableCollection<object>;
            List<SearchResult> items = new List<SearchResult>();
            if (null != i)
            {
                foreach (SearchResult item in i)
                    items.Add(item);
            }
            foreach (var item in items)
            {
                model.DownloadQueue.List.Add(new DownloadRequest()
                {
                    Added = DateTime.Now,
                    FullPath = item.Path + item.FileName,
                    LocalPath = model.DownloadFolder,
                    Nickname = item.User,
                    Size = item.Size,
                    State = DownloadRequestState.None,
                    ClientID = item.ClientID,
                    IsFolder = item.IsFolder
                });
            }
            if (items.Count > 0)
                viewModel.UpperStatusMessage = items.Count + " added to download queue.";
        }

        private void Search()
        {
            results.Clear();
            var peerlist = model.Network.Nodes.ToList();


          
            

            outstandingrequests = 0;
            viewModel.LowerStatusMessage = string.Empty;
            

            if (peerlist.Count == 0)
            {
              viewModel.UpperStatusMessage =  "Please wait until your connected";
              viewModel.LowerStatusMessage = "to a network prior to searching.";
            }
            else
            {
                viewModel.UpperStatusMessage = "Search running..";
                viewModel.LowerStatusMessage = model.Network.Nodes.Count + " peers remaining..";
                outstandingrequests = peerlist.Count;
                startTime = Environment.TickCount;
                foreach (var peer in peerlist)
                    ThreadPool.QueueUserWorkItem(new WaitCallback(RunAsync), peer);
            }
        }

        private double GetSearchSize()
        {
            switch (viewModel.SizeModifier)
            {
                case "KB":
                    return (double)viewModel.SizeText * 1024;
                case "MB":
                    return (double)viewModel.SizeText * 1048576;
                case "GB":
                    return (double)viewModel.SizeText * 1073741824;
                case "TB":
                    return (double)viewModel.SizeText * 1099511627776;
            }
            return 0;
        }

        private void RunAsync(object o)
        {
            Node peer = o as Node;
            if (null != peer)
            {
                Client client = new Client(model.LocalNode);
                SearchVerb verb = new SearchVerb(null);
                verb.SearchString = viewModel.SearchString;

                switch (viewModel.SizeSearchType)
                {
                    case "Any Size":
                        break;
                    case "Less than":
                        verb.SmallerThan = GetSearchSize();
                        break;
                    case "Greater than":
                        verb.LargerThan = GetSearchSize();
                        break;
                }

                switch (viewModel.ModifiedSearchType)
                {
                    case "Any":
                        break;
                    case "Before":
                        verb.ModifiedBefore = (DateTime)viewModel.ModifiedDate;
                        break;
                    case "After":
                        verb.ModifiedAfter = (DateTime)viewModel.ModifiedDate;
                        break;
                }

                if (client.Execute(verb, peer))
                {
                    if (null != verb.Results)
                    {
                        //Set name
                        foreach (var result in verb.Results)
                        {
                            result.User = peer.Nickname;
                            result.ClientID = peer.ID;
                        }
                        results.AddRange(verb.Results);
                    }
                }
            }
            lock (sync)
            {
                outstandingrequests--;
                if (outstandingrequests < 1)
                {
                    viewModel.UpperStatusMessage = "Search complete in " + (Environment.TickCount - startTime) + " ms";
                    viewModel.LowerStatusMessage = results.Count + " results.";
                }
                else
                {
                    viewModel.LowerStatusMessage = outstandingrequests + " peers remaining..";
                }
            }
        }
    }
}
