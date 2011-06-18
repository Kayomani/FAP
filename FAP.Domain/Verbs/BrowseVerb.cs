using System.Collections.Generic;
using FAP.Domain.Entities.FileSystem;
using FAP.Domain.Services;
using FAP.Network.Entities;

namespace FAP.Domain.Verbs
{
    public class BrowseVerb : BaseVerb, IVerb
    {
        private readonly ShareInfoService _infoService;

        public BrowseVerb(ShareInfoService i)
        {
            _infoService = i;
            Results = new List<BrowsingFile>();
        }

        public bool NoCache { set; get; }
        public string Path { set; get; }


        public List<BrowsingFile> Results { set; get; }

        #region IVerb Members

        public NetworkRequest CreateRequest()
        {
            var req = new NetworkRequest {Verb = "BROWSE", Data = Serialize(this)};
            return req;
        }

        public NetworkRequest ProcessRequest(NetworkRequest r)
        {
            var verb = Deserialise<BrowseVerb>(r.Data);

            List<BrowsingFile> results;
            if (_infoService.GetPath(verb.Path, verb.NoCache, true, out results))
                Results = results;

            r.Data = Serialize(this);
            //Clear collection to assist GC
            results.Clear();
            return r;
        }


        public bool ReceiveResponse(NetworkRequest r)
        {
            try
            {
                var verb = Deserialise<BrowseVerb>(r.Data);
                NoCache = verb.NoCache;
                Path = verb.Path;
                Results = verb.Results;
                return true;
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {
            }
            return false;
        }

        #endregion
    }
}