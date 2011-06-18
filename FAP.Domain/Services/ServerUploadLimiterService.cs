using System.Collections.Generic;
using System.Linq;
using FAP.Domain.Entities;

namespace FAP.Domain.Services
{
    public class ServerUploadLimiterService
    {
        private readonly List<ServerUploadToken> activeTokenList = new List<ServerUploadToken>();
        private readonly Model model;
        private readonly Queue<ServerUploadToken> recycledList = new Queue<ServerUploadToken>();

        public ServerUploadLimiterService(Model model)
        {
            this.model = model;
        }

        public ServerUploadToken RequestUploadToken(string node)
        {
            ServerUploadToken token;
            lock (activeTokenList)
            {
                //Create token
                if (recycledList.Count > 0)
                    token = recycledList.Dequeue();
                else
                    token = new ServerUploadToken();

                activeTokenList.Add(token);
                token.RemoteEndPoint = node;

                int totalUploads = activeTokenList.Where(t => t.GlobalQueuePosition == 0).Count();
                //If we have reached the global uploads then pause the download
                if (totalUploads > model.MaxUploads)
                {
                    //Record queue position
                    token.GlobalQueuePosition = activeTokenList.Where(t => t.GlobalQueuePosition != 0).Count() + 1;
                }
                else
                    token.GlobalQueuePosition = 0;
            }
            return token;
        }

        public int GetActiveTokenCount()
        {
            lock (activeTokenList)
                return activeTokenList.Where(n => n.GlobalQueuePosition == 0).Count();
        }

        public int GetQueueLength()
        {
            lock (activeTokenList)
                return activeTokenList.Where(n => n.GlobalQueuePosition != 0).Count();
        }


        public void FreeToken(ServerUploadToken itoken)
        {
            lock (activeTokenList)
            {
                //Recycle token
                itoken.RemoteEndPoint = null;
                itoken.GlobalQueuePosition = 0;
                recycledList.Enqueue(itoken);
                activeTokenList.Remove(itoken);

                //Start next item
                List<ServerUploadToken> list =
                    activeTokenList.Where(t => t.GlobalQueuePosition > 0).OrderBy(t => t.GlobalQueuePosition).ToList();
                if (list.Count > 0)
                    list[0].GlobalQueuePosition = 0;
                for (int i = 1; i < list.Count; i++)
                    list[i].GlobalQueuePosition = i;
            }
        }
    }
}