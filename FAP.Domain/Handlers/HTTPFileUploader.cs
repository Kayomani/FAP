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
using System.IO;
using System.Linq;
using System.Net;
using FAP.Domain.Entities;
using FAP.Domain.Net;
using FAP.Domain.Services;
using Fap.Foundation;
using HttpServer;
using HttpServer.Headers;
using HttpServer.Messages;
using NLog;

namespace FAP.Domain.Handlers
{
    public class HTTPFileUploader : ITransferWorker
    {
        private readonly BufferService bufferService;
        private readonly NetworkSpeedMeasurement nsm;
        private readonly ServerUploadLimiterService uploadLimiter;
        private readonly Model model;

        private bool isComplete;
        private long length;
        private long position;
        private string status = "HTTP - Connecting..";

        public HTTPFileUploader(BufferService b, ServerUploadLimiterService u, Model m)
        {
            bufferService = b;
            uploadLimiter = u;
            model = m;
            nsm = new NetworkSpeedMeasurement(NetSpeedType.Upload);
        }

        public DateTime TransferStart { set; get; }
        public long ResumePoint { set; get; }

        #region ITransferWorker Members

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

        #endregion

        public void DoUpload(IHttpContext context, Stream stream, string user, string url)
        {
            length = stream.Length;
            ResumePoint = 0;
            IHeader rangeHeader =
                context.Request.Headers.Where(n => n.Name.ToLowerInvariant() == "range").FirstOrDefault();
            ServerUploadToken token = null;
            TransferSession session = null;
            long readEnd = stream.Length;


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
                        string starttxt = string.Empty;
                        string endtxt = string.Empty;

                        if (header.Contains('-'))
                        {
                            starttxt = header.Substring(0, header.IndexOf("-"));
                            endtxt = header.Substring(header.IndexOf("-") + 1,
                                                      header.Length - (header.IndexOf("-") + 1));
                        }
                        else
                        {
                            starttxt = header;
                        }

                        if (!string.IsNullOrEmpty(starttxt))
                            start = long.Parse(starttxt);
                        if (!string.IsNullOrEmpty(endtxt))
                            end = long.Parse(endtxt);
                        //Only allow a partial request start.  May implement this at some point but its beyond the scope of this atm.
                        if (start != 0)
                        {
                            if (start > stream.Length)
                                start = stream.Length;
                            stream.Seek(start, SeekOrigin.Begin);
                            position = start;
                            ResumePoint = start;
                        }

                        if (end < stream.Length)
                            readEnd = end;

                        context.Response.Status = HttpStatusCode.PartialContent;
                    }
                }
            }
            catch
            {
            }

            try
            {
                if ((readEnd - position) > Model.FREE_FILE_LIMIT)
                {

                    session = new TransferSession(this);
                    model.TransferSessions.Add(session);
                    //File isnt free leech, acquire a token before we send the file
                    token = uploadLimiter.RequestUploadToken(context.RemoteEndPoint.Address.ToString());
                    while (token.GlobalQueuePosition > 0)
                    {
                        context.LastAction = DateTime.Now;
                        status = string.Format("HTTP ({0}) queued upload in slot {1}", user, token.GlobalQueuePosition);
                        token.WaitTimeout();
                    }
                }

                status = string.Format("HTTP ({0}) Sending {1}", user, url);


                TransferStart = DateTime.Now;
                //Send headers
                context.Response.ContentLength.Value = readEnd - stream.Position;
                if (context.Response.ContentLength.Value == 0)
                {

                }

                var generator = new ResponseWriter();
                generator.SendHeaders(context, context.Response);
                //Send data
                MemoryBuffer buffer = bufferService.GetBuffer();
                try
                {
                    long sent = 0;
                    while (true)
                    {
                        int chunkLength = buffer.Data.Length;

                        if (readEnd - stream.Position < chunkLength)
                            chunkLength = (int)(readEnd - stream.Position);
                        if (chunkLength < 1)
                            break;
                        int bytesRead = stream.Read(buffer.Data, 0, chunkLength);
                        if (bytesRead == 0)
                            break;
                        context.Stream.Write(buffer.Data, 0, bytesRead);
                        position += bytesRead;
                        nsm.PutData(bytesRead);
                        sent += bytesRead;
                    }
                    if (context.Response.ContentLength.Value != sent)
                    {
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
                if (null != session)
                    model.TransferSessions.Remove(session);
                status = string.Format("HTTP ({0}) Upload complete", user);
                if (null != token)
                    uploadLimiter.FreeToken(token);
                isComplete = true;
                position = length;
            }
        }
    }
}