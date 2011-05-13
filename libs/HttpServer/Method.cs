using System.Collections.Generic;

namespace HttpServer
{
    /// <summary>
    /// HTTP methods.
    /// </summary>
    public static class Method
    {
        private static List<string> _supportedMethods = new List<string>
                                                     {
                                                         Post,
                                                         Get,
                                                         Put,
                                                         Delete,
                                                         Head,
                                                         Options
                                                     };

        public static bool IsSupported(string name)
        {
            return _supportedMethods.Contains(name);
        }

        public static void AddMethod(string name)
        {
            _supportedMethods.Add(name);
        }

        public static IEnumerable<string> Methods { get { return _supportedMethods; }}


        /// <summary>
        /// Unknown method
        /// </summary>
        public const string Unknown = "";

        /// <summary>
        /// Posting data
        /// </summary>
        public const string Post = "POST";

        /// <summary>
        /// Get data
        /// </summary>
        public const string Get = "GET";

        /// <summary>
        /// Update data
        /// </summary>
        public const string Put = "PUT";

        /// <summary>
        /// Remove data
        /// </summary>
        public const string Delete = "DELETE";

        /// <summary>
        /// Get only HTTP headers.
        /// </summary>
        public const string Head = "HEAD";

        /// <summary>
        /// Options HTTP 1.1 header.
        /// </summary>
        public const string Options = "OPTIONS";
    }
}