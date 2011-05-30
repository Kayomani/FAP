using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Network.Entities;
using FAP.Domain.Entities;
using FAP.Domain.Services;

namespace FAP.Domain.Verbs
{
    public class SearchVerb : BaseVerb, IVerb
    {
        private List<SearchResult> results = new List<SearchResult>();
        private ShareInfoService shareInfoService;

        public SearchVerb(ShareInfoService s)
        {
            shareInfoService = s;
        }

        public NetworkRequest CreateRequest()
        {
            NetworkRequest r = new NetworkRequest();
            r.Verb = "SEARCH";
            r.Data = Serialize<SearchVerb>(this);
            return r;
        }

        public NetworkRequest ProcessRequest(NetworkRequest r)
        {
            SearchVerb verb = Deserialise<SearchVerb>(r.Data);
            SearchString = verb.SearchString;

            long modifiedBefore = 0;
            long modifiedAfter = 0;

            if (DateTime.MinValue != verb.ModifiedBefore)
                modifiedBefore = verb.ModifiedBefore.ToFileTime();
            if (DateTime.MinValue != verb.ModifiedAfter)
                modifiedAfter = verb.ModifiedAfter.ToFileTime();
            if (null != shareInfoService)
                results = shareInfoService.Search(verb.SearchString, Model.MAX_SEARCH_RESULTS, modifiedBefore, modifiedAfter, verb.SmallerThan, verb.LargerThan);
            r.Data = Serialize<SearchVerb>(this);
            results.Clear();
            return r;
        }

        public bool ReceiveResponse(NetworkRequest r)
        {
            SearchVerb search = Deserialise<SearchVerb>(r.Data);
            SearchString = search.SearchString;
            results = search.Results;
            return true;
        }

        public string SearchString { set; get; }
        public List<SearchResult> Results { get { return results; } set { results = value; } }
        public DateTime ModifiedBefore { set; get; }
        public DateTime ModifiedAfter { set; get; }
        public double LargerThan { set; get; }
        public double SmallerThan { set; get; }
    }
}
