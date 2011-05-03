using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Domain.Entities;
using System.Threading;
using System.Net;

namespace FAP.Domain.Services
{
    public class UpdateCheckerService
    {
        private Model model;

        public UpdateCheckerService(Model m)
        {
            model = m;
        }

        public void Run()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(doCheck));
        }

        private void doCheck(object o)
        {
            try
            {
                WebClient client = new WebClient();
                string message = client.DownloadString("http://iownallyourbase.com/fap/updates.php?i=" + model.LocalNode.ID + "&v=" + Model.AppVersion);
                if (null != message)
                {
                    foreach(var split in message.Split('\n'))
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
