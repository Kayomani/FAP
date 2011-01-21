using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fap.Network;
using Fap.Network.Entity;

namespace Fap.Domain.Verbs
{
    public class DownloadVerb : VerbBase, IVerb
    {

        public Request CreateRequest()
        {
            Request r = new Request();
            r.Command = "GET";
            r.Param = Path;
            r.RequestID = ID;
            if (ResumePoint != 0)
                r.AdditionalHeaders.Add("resume", ResumePoint.ToString());
            return r;
        }

        /// <summary>
        /// Only used to parse incoming request
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public Network.Entity.Response ProcessRequest(Network.Entity.Request req)
        {
            ID = req.RequestID;
            Path = req.Param;
            string resumetext = GetValueSafe(req.AdditionalHeaders, "resume");
            long resume = 0;
            long.TryParse(resumetext, out resume);
            ResumePoint = resume;
            return null;
        }

        public Response CreateResponse()
        {
            Response response = new Response();
            response.RequestID = ID;
            response.ContentSize = FileSize;
            response.Status = Error ? 1 : 0;
            if (QueuePosition != 0)
                response.AdditionalHeaders.Add("queue", QueuePosition.ToString());
            return response;
        }

        public bool ReceiveResponse(Network.Entity.Response r)
        {
            ID = r.RequestID;
            FileSize = r.ContentSize;
            Path = string.Empty;
            switch (r.Status)
            {
                case 0:
                    Error = false;
                    ErrorMsg = string.Empty;
                    break;
                case 1:
                    Error = true;
                    ErrorMsg = "File not found";
                    break;
                case 2:
                    Error = true;
                    ErrorMsg = "Exceeded file queues";
                    break;
                case 3:
                    Error = true;
                    ErrorMsg = "Server error";
                    break;
                default:
                    Error = true;
                    ErrorMsg = "Unknown Error";
                    break;
            }
            string queuePoint = GetValueSafe(r.AdditionalHeaders, "queue");
            if (string.IsNullOrEmpty(queuePoint))
            {
                InQueue = false;
            }
            else
            {
                InQueue = true;
                int location = 0;
                int.TryParse(queuePoint, out location);
                QueuePosition = location;
            }
            return true;
        }

        public string ID { set; get; } //RX+TX
        public string Path { set; get; } //TX
        public long FileSize { set; get; } //RX+TX
        public bool InQueue { set; get; } //RX
        public int QueuePosition { set; get; } //RX
        public bool Error { set; get; }
        public string ErrorMsg { set; get; }//RX
        public long ResumePoint { set; get; }//TX
    }
}
