using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using HttpServer.Modules;

namespace HttpServer
{
    /// <summary>
    /// Convention over configuration server.
    /// </summary>
    /// <remarks>
    /// Used to make it easy to create and use a web server.
    /// <para>
    /// All resources must exist in the "YourProject.Content" namespace (or a subdirectory called "Content" relative to yourapp.exe).
    /// </para>
    /// </remarks>
    public class SimpleServer : Server
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleServer"/> class.
        /// </summary>
        public SimpleServer()
        {
            Add(new BodyDecoders.MultiPartDecoder());
            Add(new BodyDecoders.UrlDecoder());

            var fileModule = new FileModule();
            fileModule.AddDefaultMimeTypes();
            AddEmbeddedResources(Assembly.GetCallingAssembly(), fileModule);
            AddFileResources(Assembly.GetCallingAssembly(), fileModule);
        }

        private void AddFileResources(Assembly assembly, FileModule fileModule)
        {
            var assemblyPath = Path.GetDirectoryName(assembly.Location);
            var filePath = Path.Combine(assemblyPath, "Public");
            if (Directory.Exists(filePath))
                fileModule.Resources.Add(new Resources.FileResources("/content/", filePath));
        }

        private void AddEmbeddedResources(Assembly assembly, FileModule fileModule)
        {
            string contentNamespace = null;
            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                if (!resourceName.Contains("Content"))
                    continue;

                contentNamespace = resourceName;
                break;
            }

            if (contentNamespace == null) 
                return;

            int pos = contentNamespace.IndexOf("Content");
            contentNamespace = contentNamespace.Substring(0, pos);
            fileModule.Resources.Add(new Resources.EmbeddedResourceLoader("/content/", Assembly.GetCallingAssembly(),
                                                                          contentNamespace));
        }
    }
}
