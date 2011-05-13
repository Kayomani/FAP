using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace HttpServer
{
    /// <summary>
    /// Secure version of the HTTP listener.
    /// </summary>
    public class SecureHttpListener : HttpListener
    {
        private readonly X509Certificate _certificate;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureHttpListener"/> class.
        /// </summary>
        /// <param name="address">Address to accept new connections on.</param>
        /// <param name="port">Port to accept connections on.</param>
        /// <param name="certificate">Certificate securing the connection.</param>
        public SecureHttpListener(IPAddress address, int port, X509Certificate certificate) : base(address, port)
        {
            Protocol = SslProtocols.Tls;
            _certificate = certificate;
        }

        /// <summary>
        /// Gets if listener is secure.
        /// </summary>
        /// <value></value>
        public override bool IsSecure
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets SSL protocol.
        /// </summary>
        public SslProtocols Protocol { get; set; }

        /// <summary>
        /// Gets or sets if client certificate should be used.
        /// </summary>
        public bool UseClientCertificate { get; set; }

        /// <summary>
        /// Create a new context
        /// </summary>
        /// <param name="socket">Accepted socket</param>
        /// <returns>A new context.</returns>
        /// <remarks>
        /// Factory is assigned by the <see cref="HttpListener"/> on each incoming request.
        /// </remarks>
        protected override HttpContext CreateContext(Socket socket)
        {
            return Factory.Get<SecureHttpContext>(_certificate, Protocol, socket);
        }
    }
}