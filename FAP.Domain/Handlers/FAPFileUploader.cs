using System;
using System.IO;
using System.Linq;
using System.Text;
using FAP.Domain.Entities;
using FAP.Domain.Net;
using FAP.Domain.Services;
using Fap.Foundation;
using HttpServer;
using HttpServer.Headers;
using NLog;

namespace FAP.Domain.Handlers
{
    public class FAPFileUploader : ITransferWorker
    {
        private static readonly byte[] CRLF = Encoding.ASCII.GetBytes("\r\n");
        private static readonly int CHUNK_SIZE_LIMIT = 2000000000; //1.86gb
        private readonly BufferService bufferService;
        private readonly NetworkSpeedMeasurement nsm;
        private readonly ServerUploadLimiterService uploadLimiter;

        private bool isComplete;
        private long length;
        private long position;
        private string status = "FAP Upload - Connecting..";

        public FAPFileUploader(BufferService b, ServerUploadLimiterService u)
        {
            bufferService = b;
            uploadLimiter = u;
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
            ResumePoint = 0;
            length = stream.Length;
            IHeader rangeHeader =
                context.Request.Headers.Where(n => n.Name.ToLowerInvariant() == "range").FirstOrDefault();
            ServerUploadToken token = null;
            try
            {
                //Check for range header
                if (null != rangeHeader)
                {
                    //Partial request
                    //Try to parse header
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
                            endtxt = header.Substring(header.IndexOf("-") + 1, header.Length - (header.IndexOf("-") + 1));
                        }
                        else
                        {
                            starttxt = header;
                        }

                        if (!string.IsNullOrEmpty(starttxt))
                            start = long.Parse(starttxt);
                        if (!string.IsNullOrEmpty(endtxt))
                            end = long.Parse(endtxt);
                        if (start != 0)
                        {
                            if (start > stream.Length)
                                start = stream.Length;
                            stream.Seek(start, SeekOrigin.Begin);
                            position = start;
                            ResumePoint = start;
                        }
                        //TODO: Implement this
                        if (end != 0)
                            throw new Exception("End range not supported");
                    }
                }

                //Send HTTP Headers
                SendChunkedHeaders(context);

                if (stream.Length - stream.Position > Model.FREE_FILE_LIMIT)
                {
                    //File isnt free leech, acquire a token before we send the file
                    token = uploadLimiter.RequestUploadToken(context.RemoteEndPoint.Address.ToString());
                    while (token.GlobalQueuePosition > 0)
                    {
                        int QueuePosition = token.GlobalQueuePosition;
                        if (QueuePosition > 0)
                        {
                            status = string.Format("{0} - {1} - {2} - Queue Position {3}", user, Path.GetFileName(url),
                                                   Utility.FormatBytes(stream.Length), QueuePosition);
                            SendChunkedData(context, Encoding.ASCII.GetBytes(QueuePosition.ToString() + '|'));
                            token.WaitTimeout();
                        }
                    }
                }

                TransferStart = DateTime.Now;
                //Zero queue flag, data follows
                SendChunkedData(context, Encoding.ASCII.GetBytes("0|"));
                status = string.Format("{0} - {1} - {2}", user, Path.GetFileName(url),
                                       Utility.FormatBytes(stream.Length));

                //Send data
                MemoryBuffer buffer = bufferService.GetBuffer();
                try
                {
                    //Unfortunatly the microsoft http client implementation uses an int32 for chunk sizes which limits them to 2047mb. 
                    //so sigh, send it smaller chunks.  Use a limit to 1.86gb for as it is more clean..

                    for (long i = 0; i < stream.Length; i += CHUNK_SIZE_LIMIT)
                    {
                        int chunkSize = 0;
                        if (stream.Length - position > CHUNK_SIZE_LIMIT)
                            chunkSize = CHUNK_SIZE_LIMIT;
                        else
                            chunkSize = (int) (stream.Length - position);

                        //Send chunk length
                        byte[] hexbytes = Encoding.ASCII.GetBytes(chunkSize.ToString("X"));
                        context.Stream.Write(hexbytes, 0, hexbytes.Length);
                        context.Stream.Write(CRLF, 0, CRLF.Length);
                        //Send data
                        long dataSent = 0;
                        int bytesRead = 0;
                        while (dataSent < chunkSize)
                        {
                            context.LastAction = DateTime.Now;
                            int toRead = buffer.Data.Length;
                            //Less that one buffer to send, ensure we dont send too much data just incase the file size has increased.
                            if (chunkSize - dataSent < toRead)
                                toRead = (int) (chunkSize - dataSent);
                            bytesRead = stream.Read(buffer.Data, 0, toRead);
                            context.Stream.Write(buffer.Data, 0, bytesRead);
                            nsm.PutData(bytesRead);
                            dataSent += bytesRead;
                            position += bytesRead;
                        }
                        //End data chunk
                        context.Stream.Write(CRLF, 0, CRLF.Length);
                    }
                }
                catch (Exception err)
                {
                    LogManager.GetLogger("faplog").TraceException("Failed write file to http stream", err);
                }
                finally
                {
                    bufferService.FreeBuffer(buffer);
                }
                //Send empty chunk, to flag stream end
                SendChunkedData(context, new byte[0]);
            }
            finally
            {
                status = string.Format("{0} - {1} - Complete", user, Path.GetFileName(url));
                if (null != token)
                    uploadLimiter.FreeToken(token);
                isComplete = true;
                position = length;
            }
        }

        private void SendChunkedData(IHttpContext context, byte[] data)
        {
            context.LastAction = DateTime.Now;
            string length = data.Length.ToString("X");
            byte[] hexLength = Encoding.ASCII.GetBytes(length);
            //Send chunk length
            context.Stream.Write(hexLength, 0, hexLength.Length);
            //Send CRLF
            context.Stream.Write(CRLF, 0, CRLF.Length);
            //Send data
            if (data.Length > 0)
                context.Stream.Write(data, 0, data.Length);
            //Send CRLF
            context.Stream.Write(CRLF, 0, CRLF.Length);
        }


        /// <summary>
        /// Send custom headers to the client.
        /// </summary>
        /// <param name="response">Response containing call headers.</param>
        /// <param name="context">Content used to send headers.</param>
        private void SendChunkedHeaders(IHttpContext context)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0} {1} {2}\r\n", context.Response.HttpVersion, 200, "OK");
            sb.AppendFormat("{0}: {1}\r\n", context.Response.ContentType.Name, "application/octet-stream");
            sb.AppendFormat("{0}: {1}\r\n", context.Response.Connection.Name, context.Response.Connection);
            sb.AppendFormat("{0}: {1}\r\n", "Transfer-Encoding", "chunked");
            sb.Append("\r\n");
            byte[] buffer = context.Response.Encoding.GetBytes(sb.ToString());
            context.Stream.Write(buffer, 0, buffer.Length);
        }
    }
}