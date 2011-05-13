using System;
using System.Net;
using System.Net.Sockets;
using HttpServer.Logging;

namespace HttpServer
{
    /// <summary>
    /// Http listener
    /// </summary>
    public interface IHttpListener
    {
        /// <summary>
        /// Gets listener address.
        /// </summary>
        IPAddress Address { get; }

        /// <summary>
        /// Gets if listener is secure.
        /// </summary>
        bool IsSecure { get; }

        /// <summary>
        /// Gets if listener have been started.
        /// </summary>
        bool IsStarted { get; }

        /// <summary>
        /// Gets or sets logger.
        /// </summary>
        ILogger Logger { get; set; }

        /// <summary>
        /// Gets listening port.
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Gets the maximum content size.
        /// </summary>
        /// <value>The content length limit.</value>
        /// <remarks>
        /// Used when responding to 100-continue.
        /// </remarks>
        int ContentLengthLimit { get; set; }

        /// <summary>
        /// Start listener.
        /// </summary>
        /// <param name="backLog">Number of pending accepts.</param>
        /// <remarks>
        /// Make sure that you are subscribing on <see cref="RequestReceived"/> first.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Listener have already been started.</exception>
        /// <exception cref="SocketException">Failed to start socket.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Invalid port number.</exception>
        void Start(int backLog);

        /// <summary>
        /// Stop listener.
        /// </summary>
        void Stop();

        /// <summary>
        /// A new request have been received.
        /// </summary>
        event EventHandler<RequestEventArgs> RequestReceived;

        /// <summary>
        /// Can be used to reject certain clients.
        /// </summary>
        event EventHandler<SocketFilterEventArgs> SocketAccepted;

        /// <summary>
        /// A HTTP exception have been thrown.
        /// </summary>
        /// <remarks>
        /// Fill the body with a user friendly error page, or redirect to somewhere else.
        /// </remarks>
        event EventHandler<ErrorPageEventArgs> ErrorPageRequested;

    }
}