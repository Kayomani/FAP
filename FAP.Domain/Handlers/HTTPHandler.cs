using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HttpServer;
using FAP.Domain.Services;
using FAP.Domain.Entities;
using System.Net;
using HttpServer.Messages;
using HttpServer.Headers;
using System.IO;

namespace FAP.Domain.Handlers
{
    public class HTTPHandler
    {
        private ShareInfoService infoService;
        private Model model;

        private bool isServer = false;

        private Dictionary<string, ContentTypeHeader> contentTypes =
           new Dictionary<string, ContentTypeHeader>();

        public HTTPHandler(ShareInfoService i, Model m, bool s)
        {
            infoService = i;
            model = m;
            isServer = s;
            AddDefaultMimeTypes();
        }

        public bool Handle(string req, RequestEventArgs e)
        {
            if (isServer)
            {
                return HandleServer(req, e);
            }
            else
            {
                return HandleClient(req, e);
            }
        }

        private bool HandleServer(string req, RequestEventArgs e)
        {
            e.Response.Status = HttpStatusCode.OK;
            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><title>FAP Overlord");
            sb.Append("</title></head><body>");

            sb.Append("<body></html>");

            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());

            ResponseWriter generator = new ResponseWriter();
            e.Response.ContentLength.Value = buffer.Length;
            e.Response.ContentType = new ContentTypeHeader("text/html");
            generator.SendHeaders(e.Context, e.Response);

            e.Context.Stream.Write(buffer, 0, buffer.Length);
            e.Context.Stream.Flush();
            return true;
        }


        private bool HandleClient(string req, RequestEventArgs e)
        {
            e.Response.Status = HttpStatusCode.OK;

            StringBuilder sb = new StringBuilder();


            sb.Append("<html><head><title>");
            sb.Append(Model.AppVersion);
            sb.Append(" :: ");
            sb.Append("Share browser");
            sb.Append("</title></head><body>");


            string path = e.Request.Uri.LocalPath;
            //   System.Web.HttpUtility.UrlEncode(

            if (path == null)
                path = string.Empty;
            if (!path.StartsWith("/"))
                path = "/" + path;

            if (path == "/")
            {
                //No path passed - send shares
                sb.Append("<h2>Shares</h2><table>");
                foreach (var share in model.Shares.ToList().OrderBy(s => s.Name))
                {
                    sb.Append("<tr><td>");
                    sb.Append("<a href=\"" + share.Name + "\">" + share.Name + "</a>");
                    sb.Append("</td></tr>");
                }
                sb.Append("</table>");
            }
            else
            {
                var info = infoService.GetPath(path);
                if (null != info)
                {
                    sb.Append("<h2>Shares</h2><table>");
                    sb.Append("<tr><td><a href=\"" + getParentDir(path) + "\">..</a></td></tr>");
                    foreach (var share in info.SubDirectories.OrderBy(s => s.Name))
                    {
                        sb.Append("<tr><td>");
                        sb.Append("<a href=\"" + path + "/" + share.Name + "\">" + share.Name + "</a>");
                        sb.Append("</td></tr>");
                    }

                    foreach (var share in info.Files.OrderBy(s => s.Name))
                    {
                        sb.Append("<tr><td>");
                        sb.Append("<a href=\"" + path + "/" + share.Name + "\">" + share.Name + "</a>");
                        sb.Append("</td></tr>");
                    }
                    sb.Append("</table>");
                }
                else
                {
                    string[] split = path.Split('/');

                    if (split.Length > 0)
                    {
                        string name = split[1];

                        var share = model.Shares.Where(s => s.Name == name).FirstOrDefault();
                        if (null != share)
                        {
                            string fullpath = share.Path + path.Substring(1 + share.Name.Length);
                            if (SendFile(e, fullpath))
                                return true;
                        }
                    }
                    sb.Append("<h2>404 Not found</h2>");
                    e.Response.Status = HttpStatusCode.NotFound;
                }
            }
            sb.Append("<body></html>");

            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());

            ResponseWriter generator = new ResponseWriter();
            e.Response.ContentLength.Value = buffer.Length;
            e.Response.ContentType = new ContentTypeHeader("text/html");
            generator.SendHeaders(e.Context, e.Response);

            e.Context.Stream.Write(buffer, 0, buffer.Length);
            e.Context.Stream.Flush();
            return true;
        }


        private bool SendFile(RequestEventArgs e, string path)
        {
            try
            {
                string fileExtension = Path.GetExtension(path);

                ContentTypeHeader header;
                if (!contentTypes.TryGetValue(fileExtension, out header))
                    header = contentTypes["default"];

                e.Response.ContentType = header;

                DateTime modified = File.GetLastWriteTime(path).ToUniversalTime();

                // Only send file if it has not been modified.
                var browserCacheDate = e.Request.Headers["If-Modified-Since"] as DateHeader;
                if (browserCacheDate != null)
                {
                    DateTime since = browserCacheDate.Value.ToUniversalTime();


                    // Allow for file systems with subsecond time stamps
                    modified = new DateTime(modified.Year, modified.Month, modified.Day, modified.Hour, modified.Minute, modified.Second, modified.Kind);
                    if (since >= modified)
                    {
                        e.Response.Status = HttpStatusCode.NotModified;

                        ResponseWriter generator = new ResponseWriter();
                        e.Response.ContentLength.Value = 0;
                        generator.SendHeaders(e.Context, e.Response);
                        return true;
                    }
                }

                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    e.Response.Add(new DateHeader("Last-Modified", modified));
                    // Send response and tell server to do nothing more with the request.
                    SendFile(e.Context, e.Response, fs);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Will send a file to client.
        /// </summary>
        /// <param name="context">HTTP context containing outbound stream.</param>
        /// <param name="response">Response containing headers.</param>
        /// <param name="stream">File stream</param>
        private void SendFile(IHttpContext context, IResponse response, Stream stream)
        {
            response.ContentLength.Value = stream.Length;

            ResponseWriter generator = new ResponseWriter();
            generator.SendHeaders(context, response);
            generator.SendBody(context, stream);
        }

        private string getParentDir(string path)
        {
            if(string.IsNullOrEmpty(path))
                return "/";
            path = path.Substring(0, path.LastIndexOf('/'));
            if (string.IsNullOrEmpty(path))
                return "/";
            return path;
        }



        public void AddDefaultMimeTypes()
        {
            contentTypes.Add("default", new ContentTypeHeader("application/octet-stream"));
            contentTypes.Add("txt", new ContentTypeHeader("text/plain"));
            contentTypes.Add("html", new ContentTypeHeader("text/html"));
            contentTypes.Add("htm", new ContentTypeHeader("text/html"));
            contentTypes.Add("jpg", new ContentTypeHeader("image/jpg"));
            contentTypes.Add("jpeg", new ContentTypeHeader("image/jpg"));
            contentTypes.Add("bmp", new ContentTypeHeader("image/bmp"));
            contentTypes.Add("gif", new ContentTypeHeader("image/gif"));
            contentTypes.Add("png", new ContentTypeHeader("image/png"));
            contentTypes.Add("ico", new ContentTypeHeader("image/vnd.microsoft.icon"));
            contentTypes.Add("css", new ContentTypeHeader("text/css"));
            contentTypes.Add("gzip", new ContentTypeHeader("application/x-gzip"));
            contentTypes.Add("zip", new ContentTypeHeader("multipart/x-zip"));
            contentTypes.Add("tar", new ContentTypeHeader("application/x-tar"));
            contentTypes.Add("pdf", new ContentTypeHeader("application/pdf"));
            contentTypes.Add("rtf", new ContentTypeHeader("application/rtf"));
            contentTypes.Add("xls", new ContentTypeHeader("application/vnd.ms-excel"));
            contentTypes.Add("ppt", new ContentTypeHeader("application/vnd.ms-powerpoint"));
            contentTypes.Add("doc", new ContentTypeHeader("application/application/msword"));
            contentTypes.Add("js", new ContentTypeHeader("application/javascript"));
            contentTypes.Add("au", new ContentTypeHeader("audio/basic"));
            contentTypes.Add("snd", new ContentTypeHeader("audio/basic"));
            contentTypes.Add("es", new ContentTypeHeader("audio/echospeech"));
            contentTypes.Add("mp3", new ContentTypeHeader("audio/mpeg"));
            contentTypes.Add("mp2", new ContentTypeHeader("audio/mpeg"));
            contentTypes.Add("mid", new ContentTypeHeader("audio/midi"));
            contentTypes.Add("wav", new ContentTypeHeader("audio/x-wav"));
            contentTypes.Add("swf", new ContentTypeHeader("application/x-shockwave-flash"));
            contentTypes.Add("avi", new ContentTypeHeader("video/avi"));
            contentTypes.Add("rm", new ContentTypeHeader("audio/x-pn-realaudio"));
            contentTypes.Add("ram", new ContentTypeHeader("audio/x-pn-realaudio"));
            contentTypes.Add("aif", new ContentTypeHeader("audio/x-aiff"));
        }
    }
}
