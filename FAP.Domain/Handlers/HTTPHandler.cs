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
using HttpServer;
using FAP.Domain.Services;
using FAP.Domain.Entities;
using System.Net;
using HttpServer.Messages;
using HttpServer.Headers;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Resources;
using System.Web;
using Fap.Foundation;
using NLog;

namespace FAP.Domain.Handlers
{
    public class HTTPHandler
    {
        private readonly string WEB_PREFIX = "/Fap.app.web/";
        private readonly string WEB_ICON_PREFIX = "/Fap.app.web/icon/";

        private ShareInfoService infoService;
        private Model model;
        private BufferService bufferService;
        private ServerUploadLimiterService uploadLimiter;

        private Dictionary<string, ContentTypeHeader> contentTypes =
           new Dictionary<string, ContentTypeHeader>();

        //Icon cache
        private Dictionary<string, byte[]> iconCache = new Dictionary<string, byte[]>();
        private object sync = new object();

        public HTTPHandler(ShareInfoService i, Model m, BufferService b, ServerUploadLimiterService u)
        {
            infoService = i;
            model = m;
            bufferService = b;
            uploadLimiter = u;
            AddDefaultMimeTypes();
        }

        public bool Handle(string req, RequestEventArgs e)
        {
            e.Response.Status = HttpStatusCode.OK;
            string path = HttpUtility.UrlDecode(e.Request.Uri.AbsolutePath);
            byte[] data = null;

            if (path.StartsWith(WEB_ICON_PREFIX))
            {
                //what icon been requested?
                string ext = path.Substring(path.LastIndexOf("/")+1);
                
                lock (sync)
                {
                    //Has the icon been requested already? if so just return that
                    if (iconCache.ContainsKey(ext))
                    {
                        data = iconCache[ext];
                        e.Response.ContentType = contentTypes["png"];
                    }
                    else
                    {
                        //item wasnt cached
                        if (ext == "folder")
                        {
                            data = GetResource("Images\\folder.png");
                            iconCache.Add("folder", data);
                            e.Response.ContentType = contentTypes["png"];
                        }
                        else
                        {


                            var icon = IconReader.GetFileIcon("file." + ext, IconReader.IconSize.Small, false);
                            using (MemoryStream mem = new MemoryStream())
                            {
                                using (var bmp = icon.ToBitmap())
                                {
                                    bmp.MakeTransparent();
                                    bmp.Save(mem, System.Drawing.Imaging.ImageFormat.Png);
                                    data = mem.ToArray();
                                    iconCache.Add(ext, data);
                                    e.Response.ContentType = contentTypes["png"];
                                }
                            }
                        }

                    }
                }
            }
            else if (path.StartsWith(WEB_PREFIX))
            {
                //Has a static file been requested?

                data = GetResource(path.Substring(WEB_PREFIX.Length));

                string ext = Path.GetExtension(path);
                if (ext != null && ext.StartsWith("."))
                    ext = ext.Substring(1);

                ContentTypeHeader header;
                if (!contentTypes.TryGetValue(ext, out header))
                    header = contentTypes["default"];
                e.Response.ContentType = header;
            }
            else
            {
                //A folder or file was requested
                string page = Encoding.UTF8.GetString(GetResource("template.html"));
                Dictionary<string, object> pagedata = new Dictionary<string, object>();

                pagedata.Add("model", model);
                pagedata.Add("appver", Model.AppVersion);
                pagedata.Add("freelimit", Utility.FormatBytes(Model.WEB_FREE_FILE_LIMIT));

                pagedata.Add("util", new Utility());

                if(!path.EndsWith("/"))
                    pagedata.Add("path", path + "/");
                else
                    pagedata.Add("path", path);

                //Add path info
                List<DisplayInfo> paths = new List<DisplayInfo>();

                string[] split = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < split.Length; i++)
                {
                    StringBuilder sb = new StringBuilder("/");
                    for (int y = 0; y <= i; y++)
                    {
                        sb.Append(split[y]);
                        sb.Append("/");
                    }

                    DisplayInfo di = new DisplayInfo();
                    di.SetData("Name", split[i]);
                    di.SetData("Path", sb.ToString());
                    paths.Add(di);
                }

                pagedata.Add("pathSplit", paths);

               /* List<DisplayInfo> peers = new List<DisplayInfo>();

                foreach (var peer in model.Network.Nodes.ToList().Where(n => n.NodeType != ClientType.Overlord && !string.IsNullOrEmpty(n.Nickname)))
                {
                    if (!string.IsNullOrEmpty(peer.Location))
                    {
                        DisplayInfo p = new DisplayInfo();
                        p.SetData("Name", string.IsNullOrEmpty(peer.Nickname) ? "Unknown" : peer.Nickname);
                        p.SetData("Location", peer.Location);
                        peers.Add(p);
                    }
                }

                pagedata.Add("peers", peers);*/

                List<DisplayInfo> files = new List<DisplayInfo>();
                long totalSize = 0;

                if (string.IsNullOrEmpty(path) || path == "/")
                {
                    //At the root - Send a list of shares

                    FAP.Domain.Entities.FileSystem.Directory root = new FAP.Domain.Entities.FileSystem.Directory();

                    foreach (var share in model.Shares.ToList())
                    {
                        DisplayInfo d = new DisplayInfo();
                        d.SetData("Name", share.Name);
                        d.SetData("Path", HttpUtility.UrlEncode(share.Name));
                        d.SetData("Icon", "folder");
                        d.SetData("Size", share.Size);
                        d.SetData("Sizetxt", Utility.FormatBytes(share.Size));
                        d.SetData("LastModifiedtxt", share.LastRefresh.ToShortDateString());
                        d.SetData("LastModified", share.LastRefresh.ToFileTime());
                        files.Add(d);
                        totalSize += share.Size;
                    }

                }
                else
                {
                    string localPath = string.Empty;

                    if (infoService.ToLocalPath(path,out localPath) && File.Exists(localPath))
                    {
                        //User has requested a file
                        return SendFile(e, localPath, path);
                    }
                    else
                    {
                        var fileInfo = infoService.GetPath(path);

                        foreach (var dir in fileInfo.SubDirectories)
                        {
                            DisplayInfo d = new DisplayInfo();
                            d.SetData("Name", dir.Name);
                            d.SetData("Path", HttpUtility.UrlEncode(dir.Name));
                            d.SetData("Icon", "folder");
                            d.SetData("Sizetxt", Utility.FormatBytes(dir.Size));
                            d.SetData("Size", dir.Size);
                            d.SetData("LastModifiedtxt", DateTime.FromFileTime(dir.LastModified).ToShortDateString());
                            d.SetData("LastModified", dir.LastModified);
                            files.Add(d);
                            totalSize += dir.Size;
                        }

                        foreach (var file in fileInfo.Files)
                        {
                            DisplayInfo d = new DisplayInfo();
                            d.SetData("Name", file.Name);
                            string ext = Path.GetExtension(file.Name);
                            if (ext != null && ext.StartsWith("."))
                                ext = ext.Substring(1);
                            d.SetData("Path", HttpUtility.UrlEncode(file.Name));
                            d.SetData("Icon", ext);
                            d.SetData("Size", file.Size);
                            d.SetData("Sizetxt", Utility.FormatBytes(file.Size));
                            d.SetData("LastModifiedtxt", DateTime.FromFileTime(file.LastModified).ToShortDateString());
                            d.SetData("LastModified", file.LastModified);
                            files.Add(d);
                            totalSize += file.Size;
                        }
                    }
                }

                pagedata.Add("files", files);
                pagedata.Add("totalSize", Utility.FormatBytes(totalSize));

                //Generate the page
                page = TemplateEngineService.Generate(page, pagedata);
                data = Encoding.UTF8.GetBytes(page);
                e.Response.ContentType = contentTypes["html"];

                //Clear up
                foreach (var item in pagedata.Values)
                {
                    if (item is DisplayInfo)
                    {
                        DisplayInfo i = item as DisplayInfo;
                        i.Clear();
                    }
                }
                pagedata.Clear();
            }
             
            if (null == data)
            {
                e.Response.Status = HttpStatusCode.NotFound;
                ResponseWriter generator = new ResponseWriter();
                e.Response.ContentLength.Value = 0;
                generator.SendHeaders(e.Context, e.Response);
            }
            else
            {
                ResponseWriter generator = new ResponseWriter();
                e.Response.ContentLength.Value = data.Length;
                generator.SendHeaders(e.Context, e.Response);
                e.Context.Stream.Write(data, 0, data.Length);
                e.Context.Stream.Flush();
            }
            data = null;
            return true;
        }

        

        private bool SendFile(RequestEventArgs e, string path, string url)
        {
            try
            {
                string fileExtension = Path.GetExtension(path);
                if (fileExtension != null && fileExtension.StartsWith("."))
                    fileExtension = fileExtension.Substring(1);


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

                using (FileStream fs = new FileStream(path, FileMode.Open,FileAccess.Read,FileShare.Read))
                {
                    e.Response.Add(new DateHeader("Last-Modified", modified));
                    // Send response and tell server to do nothing more with the request.
                    SendFile(e.Context, fs,url);
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
        private void SendFile(IHttpContext context, Stream stream, string url)
        {
            HTTPFileUploader worker = new HTTPFileUploader(bufferService, uploadLimiter);
            TransferSession session = null;
            try
            {
                if (stream.Length > Model.WEB_FREE_FILE_LIMIT)
                {
                    session = new TransferSession(worker);
                    model.TransferSessions.Add(session);
                }

                //Try to find the username of the request
                string userName = context.RemoteEndPoint.Address.ToString();
                var search = model.Network.Nodes.ToList().Where(n => n.NodeType != ClientType.Overlord && n.Host == userName).FirstOrDefault();
                if (null != search && !string.IsNullOrEmpty(search.Nickname))
                    userName = search.Nickname;

                worker.DoUpload(context, stream, userName, url);
            }
            finally
            {
                if(null!=session)
                    model.TransferSessions.Remove(session);
            }
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
            contentTypes.Add("nfo", new ContentTypeHeader("text/plain"));
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

        public byte[] GetResource(string name)
        {
           string path = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);

            try
            {
                using (FileStream stream = File.Open(path + "\\Web.Resources\\" + name, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    byte[] buffer = new byte[stream.Length];
                     stream.Read(buffer, 0, buffer.Length);
                     return buffer;
                }
            }
            catch
            {
            }
            return null;
        }
    }
}
