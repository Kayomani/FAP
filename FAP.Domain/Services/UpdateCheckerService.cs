using System.Net;
using System.Threading;
using FAP.Domain.Entities;

namespace FAP.Domain.Services
{
    public class UpdateCheckerService
    {
        private readonly Model model;

        public UpdateCheckerService(Model m)
        {
            model = m;
        }

        public void Run()
        {
            ThreadPool.QueueUserWorkItem(doCheck);
        }

        private void doCheck(object o)
        {
            try
            {
                var client = new WebClient();
                string message =
                    client.DownloadString("http://iownallyourbase.com/fap/updates.php?i=" + model.LocalNode.ID + "&v=" +
                                          Model.AppVersion);
                if (null != message)
                {
                    foreach (string split in message.Split('\n'))
                        model.Messages.Add(split);
                }
            }
            catch
            {
                model.Messages.Add("An error occured during client update check");
            }
        }
    }
}