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
using FAP.Domain.Entities;
using HttpServer;
using System.IO;
using FAP.Domain.Services;
using System.Net;
using HttpServer.Messages;
using NLog;
using FAP.Domain.Net;

namespace FAP.Domain.Handlers
{
    public class HTTPFileUploader : ITransferWorker
    {
        private BufferService bufferService;
        private ServerUploadLimiterService uploadLimiter;
        private NetworkSpeedMeasurement nsm;

        private long length = 0;
        private bool isComplete = false;
        private string status = "HTTP - Connecting..";
        private long position = 0;

        public HTTPFileUploader(BufferService b, ServerUploadLimiterService u)
        {
            bufferService = b;
            uploadLimiter = u;
            nsm = new NetworkSpeedMeasurement(NetSpeedType.Upload);
        }

        public void DoUpload(IHttpContext context, Stream stream, string user, string url)
        {
            length = stream.Length;
            var rangeHeader = context.Request.Headers.Where(n => n.Name.ToLowerInvariant() == "range").FirstOrDefault();
            ServerUploadToken token = null;
            try
            {

                if (stream.Length > Model.FREE_FILE_LIMIT)
                {
                    //File isnt free leech, acquire a token before we send the file
                    token = uploadLimiter.RequestUploadToken(context.RemoteEndPoint.Address.ToString());
                    while (token.GlobalQueuePosition > 0)
                    {
                        status = string.Format("HTTP ({0}) queued upload in slot {1}", user,token.GlobalQueuePosition); 
                        token.WaitTimeout();
                    }
                }

                status = string.Format("HTTP ({0}) Sending {1}", user, url); 
                try
                {
                    if (null != rangeHeader)
                    {
                        //Partial request
                        //Try to parse header - if we fail just send a 200 ok from zero
                        long start = 0;
                        long end = 0;

                        if (rangeHeader.HeaderValue.StartsWith("bytes="))
                        {
                            string header = rangeHeader.HeaderValue.Substring(6).Trim();
                            string starttxt = header.Substring(0, header.IndexOf("-"));
                            string endtxt = header.Substring(header.IndexOf("-") + 1, header.Length - (header.IndexOf("-") + 1));

                            if (!string.IsNullOrEmpty(starttxt))
                                start = long.Parse(starttxt);
                            if (!string.IsNullOrEmpty(endtxt))
                                end = long.Parse(endtxt);
                            //Only allow a partial request start.  May implement this at some point but its beyond the scope of this atm.
                            if (start != 0 && end == 0)
                            {
                                if (start > stream.Length)
                                    start = stream.Length;
                                stream.Seek(start, SeekOrigin.Begin);
                                position = start;
                                context.Response.Status = HttpStatusCode.PartialContent;
                            }
                        }
                    }
                }
                catch { }

                context.Response.ContentLength.Value = stream.Length - stream.Position;

                ResponseWriter generator = new ResponseWriter();
                generator.SendHeaders(context, context.Response);
                //Send data
                var buffer = bufferService.GetBuffer();
                try
                {
                    int bytesRead = stream.Read(buffer.Data, 0, buffer.Data.Length);
                    while (bytesRead > 0)
                    {
                        context.Stream.Write(buffer.Data, 0, bytesRead);
                        position += bytesRead;
                        nsm.PutData(bytesRead);
                        bytesRead = stream.Read(buffer.Data, 0, buffer.Data.Length);
                    }
                }
                catch (Exception err)
                {
                    LogManager.GetLogger("faplog").TraceException("Failed to send body through context stream.", err);
                }
                finally
                {
                    bufferService.FreeBuffer(buffer);
                }
            }
            finally
            {
                status = string.Format("HTTP ({0}) Upload complete", user); 
                if (null != token)
                    uploadLimiter.FreeToken(token);
                isComplete = true;
                position = length;
            }
        }

        public long Length
        {
            get { return length; }
        }

        public bool IsComplete
        {
            get { return isComplete; }
        }

        public long Speed
        {
            get { return nsm.GetSpeed(); }
        }

        public string Status
        {
            get { return status; }
        }

        public long Position
        {
            get { return position; }
        }
    }
}
