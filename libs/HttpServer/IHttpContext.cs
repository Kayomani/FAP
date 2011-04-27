using System.IO;
using System.Net;
using System.Net.Security;
using HttpServer.Logging;
using HttpServer.Messages;

namespace HttpServer
{
    /// <summary>
    /// Context that received a HTTP request.
    /// </summary>
    public interface IHttpContext
    {
        /// <summary>
        /// Gets if current context is using a secure connection.
        /// </summary>
        bool IsSecure { get; }

        /// <summary>
        /// Gets logger.
        /// </summary>
        ILogger Logger { get; }

        /// <summary>
        /// Gets remote end point
        /// </summary>
        IPEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Gets stream used to send/receive data to/from remote end point.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The stream can be any type of stream, do not assume that it's a network
        /// stream. For instance, it can be a <see cref="SslStream"/> or a ZipStream.
        /// </para>
        /// </remarks>
        Stream Stream { get; }

        /// <summary>
        /// Gets the currently handled request
        /// </summary>
        /// <value>The request.</value>
        IRequest Request { get; }

        /// <summary>
        /// Gets the response that is going to be sent back
        /// </summary>
        /// <value>The response.</value>
        IResponse Response { get; }

        /// <summary>
        /// Disconnect context.
        /// </summary>
        void Disconnect();
    }
}