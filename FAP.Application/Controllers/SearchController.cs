using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Waf.Applications;
using Autofac;
using FAP.Application.ViewModel;
using FAP.Domain;
using FAP.Domain.Entities;
using FAP.Domain.Net;
using FAP.Domain.Verbs;
using Fap.Foundation;

namespace FAP.Application.Controllers
{
    public class SearchController
    {
        private readonly IContainer container;
        private readonly Model model;
        private readonly object sync = new object();
        private SafeObservedCollection<SearchResult> currentResults = new SafeObservedCollection<SearchResult>();
        private int outstandingrequests;
        private long startTime;
        private SearchViewModel viewModel;

        public SearchController(IContainer c, Model m)
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
                viewModel.Download = new DelegateCommand(Download);
                viewModel.ViewShare = new DelegateCommand(ViewShare);
                viewModel.Reset = new DelegateCommand(Reset);
            }
        }

        private void Reset()
        {
            if (null != viewModel.Results)
                viewModel.Results.Dispose();
            currentResults.Clear();
            currentResults = new SafeObservedCollection<SearchResult>();
            viewModel.Results = new SafeObservingCollection<SearchResult>(currentResults);
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
            viewModel.AllowSearch = false;
            var i = o as ObservableCollection<object>;
            var items = new List<SearchResult>();
            if (null != i)
            {
                foreach (SearchResult item in i)
                    items.Add(item);
            }
            foreach (SearchResult item in items)
            {
                string fullpath = item.Path;

                if (!fullpath.EndsWith("/"))
                    fullpath += "/";
                model.DownloadQueue.List.Add(new DownloadRequest
                                                 {
                                                     Added = DateTime.Now,
                                                     FullPath = fullpath + item.FileName,
                                                     LocalPath = item.Path,
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
            viewModel.AllowSearch = false;
            ThreadPool.QueueUserWorkItem(EnableSearch);
            currentResults.Clear();
            currentResults = new SafeObservedCollection<SearchResult>();
            if (null != viewModel.Results)
                viewModel.Results.Dispose();
            viewModel.Results = new SafeObservingCollection<SearchResult>(currentResults);

            List<Node> peerlist = model.Network.Nodes.ToList();


            outstandingrequests = 0;
            viewModel.LowerStatusMessage = string.Empty;


            if (peerlist.Count == 0)
            {
                viewModel.UpperStatusMessage = "Please wait until your connected";
                viewModel.LowerStatusMessage = "to a network prior to searching.";
            }
            else
            {
                viewModel.UpperStatusMessage = "Search running..";
                viewModel.LowerStatusMessage = model.Network.Nodes.Count + " peers remaining..";
                outstandingrequests = peerlist.Count;
                startTime = Environment.TickCount;
                foreach (Node peer in peerlist)
                    ThreadPool.QueueUserWorkItem(RunAsync, new AsyncSearchParam {Node = peer, Results = currentResults});
            }
        }

        private void EnableSearch(object b)
        {
            Thread.Sleep(8000);
            viewModel.AllowSearch = true;
        }

        private double GetSearchSize()
        {
            switch (viewModel.SizeModifier)
            {
                case "KB":
                    return (double) viewModel.SizeText*1024;
                case "MB":
                    return (double) viewModel.SizeText*1048576;
                case "GB":
                    return (double) viewModel.SizeText*1073741824;
                case "TB":
                    return (double) viewModel.SizeText*1099511627776;
            }
            return 0;
        }

        private void RunAsync(object o)
        {
            var param = o as AsyncSearchParam;
            if (null != param && null != param.Node)
            {
                var client = new Client(model.LocalNode);
                var verb = new SearchVerb(null);
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
                        verb.ModifiedBefore = (DateTime) viewModel.ModifiedDate;
                        break;
                    case "After":
                        verb.ModifiedAfter = (DateTime) viewModel.ModifiedDate;
                        break;
                }

                if (client.Execute(verb, param.Node))
                {
                    if (null != verb.Results)
                    {
                        //Set name
                        foreach (SearchResult result in verb.Results)
                        {
                            result.User = param.Node.Nickname;
                            result.ClientID = param.Node.ID;
                        }
                        param.Results.AddRange(verb.Results);
                    }
                }
            }
            lock (sync)
            {
                //If we still on the same search then update the UI.
                if (param.Results == currentResults)
                {
                    outstandingrequests--;
                    if (outstandingrequests < 1)
                    {
                        viewModel.UpperStatusMessage = "Search complete in " + (Environment.TickCount - startTime) +
                                                       " ms";
                        viewModel.LowerStatusMessage = currentResults.Count + " results.";
                    }
                    else
                    {
                        viewModel.LowerStatusMessage = outstandingrequests + " peers remaining..";
                    }
                }
            }
        }

        #region Nested type: AsyncSearchParam

        private class AsyncSearchParam
        {
            public SafeObservedCollection<SearchResult> Results { set; get; }
            public Node Node { set; get; }
        }

        #endregion
    }
}