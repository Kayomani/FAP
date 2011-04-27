using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using HttpServer.Messages;
using HttpServer.Transports;

namespace HttpServer
{
    internal class SecureHttpContext : HttpContext
    {
        private readonly X509Certificate _certificate;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureHttpContext"/> class.
        /// </summary>
        /// <param name="protocols">SSL protocol to use.</param>
        /// <param name="socket">The socket.</param>
        /// <param name="context">The context.</param>
        /// <param name="certificate">Server certificate to use.</param>
        public SecureHttpContext(X509Certificate certificate, SslProtocols protocols, Socket socket,
                                 MessageFactoryContext context) : base(socket, context)
        {
            _certificate = certificate;
            Protocol = protocols;
        }


        /// <summary>
        /// Gets or sets client certificate.
        /// </summary>
        public ClientCertificate ClientCertificate { get; private set; }

        public override bool IsSecure
        {
            get { return true; }
        }

        /// <summary>
        /// Gets used protocol.
        /// </summary>
        protected SslProtocols Protocol { get; private set; }

        /// <summary>
        /// Gets or sets if client certificate should be used instead of server certificate.
        /// </summary>
        public bool UseClientCertificate { get; set; }

        /// <summary>
        /// Create stream used to send and receive bytes from the socket.
        /// </summary>
        /// <param name="socket">Socket to wrap</param>
        /// <returns>Stream</returns>
        /// <exception cref="InvalidOperationException">Stream could not be created.</exception>
        protected override Stream CreateStream(Socket socket)
        {
            Stream stream = base.CreateStream(socket);

            var sslStream = new SslStream(stream, false, OnValidation);
            try
            {
                sslStream.AuthenticateAsServer(_certificate, UseClientCertificate, Protocol, false);
            }
            catch (IOException err)
            {
                Logger.Trace(err.Message);
                throw new InvalidOperationException("Failed to authenticate", err);
            }
            catch (ObjectDisposedException err)
            {
                Logger.Trace(err.Message);
                throw new InvalidOperationException("Failed to create stream.", err);
            }
            catch (AuthenticationException err)
            {
                Logger.Trace((err.InnerException != null) ? err.InnerException.Message : err.Message);
                throw new InvalidOperationException("Failed to authenticate.", err);
            }

            return sslStream;
        }

        private bool OnValidation(object sender, X509Certificate receivedCertificate, X509Chain chain,
                                  SslPolicyErrors sslPolicyErrors)
        {
            ClientCertificate = new ClientCertificate(receivedCertificate, chain, sslPolicyErrors);
            return !(UseClientCertificate && receivedCertificate == null);
        }
    }
}