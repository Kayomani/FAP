using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using HttpServer.Headers;
using HttpServer.Logging;
using HttpServer.Messages;
using HttpServer.Messages.Parser;
using HttpServer.Transports;

namespace HttpServer
{
	/// <summary>
	/// A HTTP context
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	[Component]
	public class HttpContext : IHttpContext, IDisposable
	{
		[ThreadStatic] private static IHttpContext _context;
		private readonly byte[] _buffer = new byte[65535];
		private readonly ILogger _logger = LogFactory.CreateLogger(typeof (HttpContext));
		private Timer _keepAlive;
		private int _keepAliveTimeout = 100000; // 100 seconds.

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpContext"/> class.
		/// </summary>
		/// <param name="socket">Socket received from HTTP listener.</param>
		/// <param name="context">Context used to parse incoming messages.</param>
		public HttpContext(Socket socket, MessageFactoryContext context)
		{
			Socket = socket;
			MessageFactoryContext = context;
			MessageFactoryContext.RequestCompleted += OnRequest;
			MessageFactoryContext.ContinueResponseRequested += On100Continue;
		}

		/// <summary>
		/// Gets currently executing HTTP context.
		/// </summary>
		public static IHttpContext Current
		{
			get { return _context; }
			internal set { _context = value; }
		}

		/// <summary>
		/// Gets or sets description
		/// </summary>
		internal HttpFactory HttpFactory { get; set; }

		/// <summary>
		/// gets factory used to build request objects
		/// </summary>
		internal MessageFactoryContext MessageFactoryContext { get; private set; }

		/// <summary>
		/// Gets socket
		/// </summary>
		internal Socket Socket { get; private set; }

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			Close();
		}

		#endregion

		#region IHttpContext Members

		/// <summary>
		/// Gets remove end point
		/// </summary>
		public IPEndPoint RemoteEndPoint
		{
			get { return (IPEndPoint) Socket.RemoteEndPoint; }
		}

		/// <summary>
		/// Gets network stream.
		/// </summary>
		public Stream Stream { get; private set; }

	    /// <summary>
	    /// Gets the currently handled request
	    /// </summary>
	    /// <value>The request.</value>
	    public IRequest Request { get; internal set; }

	    /// <summary>
	    /// Gets the response that is going to be sent back
	    /// </summary>
	    /// <value>The response.</value>
        public IResponse Response { get; internal set; }

	    /// <summary>
		/// Gets logger.
		/// </summary>
		public ILogger Logger
		{
			get { return _logger; }
		}

		/// <summary>
		/// Gets if current context is using a secure connection.
		/// </summary>
		public virtual bool IsSecure
		{
			get { return false; }
		}

		/// <summary>
		/// Disconnect context.
		/// </summary>
		public void Disconnect()
		{
			Close();
		}

		#endregion

		/// <summary>
		/// Triggered for all requests in the server (after the response have been sent)
		/// </summary>
		public static event EventHandler<RequestEventArgs> CurrentRequestCompleted = delegate { };

		/// <summary>
		/// Triggered for current request (after the response have been sent)
		/// </summary>
		public event EventHandler<RequestEventArgs> RequestCompleted = delegate { };

		private void On100Continue(object sender, ContinueEventArgs e)
		{
			ContinueResponseRequested(this, e);
		}

		/// <summary>
		/// Close and release socket.
		/// </summary>
		private void Close()
		{
			lock (this)
			{
				if (Socket == null)
					return;

                try
                {
                    if (_keepAlive != null)
                    {
                        _keepAlive.Dispose();
                        _keepAlive = null;
                    }

                    Socket.Disconnect(true);
                    Socket.Close();
                    Socket = null;
                    Stream.Close();
                    Stream.Dispose();
                    Stream = null;
                    MessageFactoryContext.RequestCompleted -= OnRequest;
                    MessageFactoryContext.ContinueResponseRequested -= On100Continue;
                    MessageFactoryContext.Reset();
                }
                catch(Exception err)
                {
                    _logger.Warning("Failed to close context properly.", err);
                }
			}
			Disconnected(this, EventArgs.Empty);
		}

		/// <summary>
		/// Create stream used to send and receive bytes from the socket.
		/// </summary>
		/// <param name="socket">Socket to wrap</param>
		/// <returns>Stream</returns>
		/// <exception cref="InvalidOperationException">Stream could not be created.</exception>
		protected virtual Stream CreateStream(Socket socket)
		{
			return new ReusableSocketNetworkStream(socket, true);
		}

		/// <summary>
		/// Interpret incoming data.
		/// </summary>
		/// <param name="ar"></param>
		private void OnReceive(IAsyncResult ar)
		{
			// been closed by our side.
			if (Stream == null)
				return;

			_context = this;
			HttpFactory.Current = HttpFactory;

			try
			{
				int bytesLeft = Stream.EndRead(ar);
				if (bytesLeft == 0)
				{
					_logger.Trace("Client disconnected.");
					Close();
					return;
				}

				_logger.Debug(Socket.RemoteEndPoint + " received " + bytesLeft + " bytes.");

				if (bytesLeft < 5000)
				{
					string temp = Encoding.ASCII.GetString(_buffer, 0, bytesLeft);
					_logger.Trace(temp);
				}

				int offset = ParseBuffer(bytesLeft);
				bytesLeft -= offset;

				if (bytesLeft > 0)
				{
					_logger.Warning("Moving " + bytesLeft + " from " + offset + " to beginning of array.");
					Buffer.BlockCopy(_buffer, offset, _buffer, 0, bytesLeft);
				}
				Stream.BeginRead(_buffer, 0, _buffer.Length - offset, OnReceive, null);
			}
			catch (ParserException err)
			{
				_logger.Warning(err.ToString());
				var response = new Response("HTTP/1.0", HttpStatusCode.BadRequest, err.Message);
				var generator = HttpFactory.Current.Get<ResponseWriter>();
			    generator.SendErrorPage(this, response, err);
				Close();
			}
			catch (Exception err)
			{
				if (!(err is IOException))
				{
					_logger.Error("Failed to read from stream: " + err);
                    var responseWriter = HttpFactory.Current.Get<ResponseWriter>();
				    var response = new Response("HTTP/1.0", HttpStatusCode.InternalServerError, err.Message);
				    responseWriter.SendErrorPage(this, response, err);
				}

				Close();
			}
		}

		/// <summary>
		/// A request was received from the parser.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnRequest(object sender, FactoryRequestEventArgs e)
		{
			_context = this;
			Response = HttpFactory.Current.Get<IResponse>(this, e.Request);
			_logger.Debug("Received '" + e.Request.Method + " " + e.Request.Uri.PathAndQuery + "' from " +
			              Socket.RemoteEndPoint);

			// keep alive.
			if (e.Request.Connection != null && e.Request.Connection.Type == ConnectionType.KeepAlive)
			{
				Response.Add(new StringHeader("Keep-Alive", "timeout=5, max=100"));

				// refresh timer
				if (_keepAlive != null)
					_keepAlive.Change(_keepAliveTimeout, _keepAliveTimeout);
			}

		    Request = e.Request;
            CurrentRequestReceived(this, new RequestEventArgs(this, e.Request, Response));
            RequestReceived(this, new RequestEventArgs(this, e.Request, Response));

			//
            if (Response.Connection.Type == ConnectionType.KeepAlive)
			{
				if (_keepAlive == null)
					_keepAlive = new Timer(OnConnectionTimeout, null, _keepAliveTimeout, _keepAliveTimeout);
			}

            RequestCompleted(this, new RequestEventArgs(this, e.Request, Response));
            CurrentRequestCompleted(this, new RequestEventArgs(this, e.Request, Response));
		}

		private void OnConnectionTimeout(object state)
		{
			if (_keepAlive != null)
				_keepAlive.Dispose();
			_logger.Info("Keep-Alive timeout");
			Disconnect();
		}

		/// <summary>
		/// Parse all complete requests in buffer.
		/// </summary>
		/// <param name="bytesLeft"></param>
		/// <returns>offset in buffer where parsing stopped.</returns>
		/// <exception cref="InvalidOperationException">Parsing failed.</exception>
		private int ParseBuffer(int bytesLeft)
		{
			int offset = MessageFactoryContext.Parse(_buffer, 0, bytesLeft);
			bytesLeft -= offset;

			// try another pass if we got bytes left.
			if (bytesLeft <= 0)
				return offset;

			// Continue until offset is not changed.
			int oldOffset = 0;
			while (offset != oldOffset)
			{
				oldOffset = offset;
				_logger.Trace("Parsing from index " + offset + ", " + bytesLeft + " bytes.");
				offset = MessageFactoryContext.Parse(_buffer, offset, bytesLeft);
				bytesLeft -= offset;
			}
			return offset;
		}

		/// <summary>
		/// Start content.
		/// </summary>
		/// <exception cref="SocketException">A socket operation failed.</exception>
		/// <exception cref="IOException">Reading from stream failed.</exception>
		internal void Start()
		{
			Stream = CreateStream(Socket);
			Stream.BeginRead(_buffer, 0, _buffer.Length, OnReceive, null);
		}

		/// <summary>
		/// A new request have been received.
		/// </summary>
		public event EventHandler<RequestEventArgs> RequestReceived = delegate { };

		/// <summary>
		/// A new request have been received (invoked for ALL requests)
		/// </summary>
		public static event EventHandler<RequestEventArgs> CurrentRequestReceived = delegate { };

		/// <summary>
		/// Client have been disconnected.
		/// </summary>
		public event EventHandler Disconnected = delegate { };

		/// <summary>
		/// Client asks if he may continue.
		/// </summary>
		/// <remarks>
		/// If the body is too large or anything like that you should respond <see cref="HttpStatusCode.ExpectationFailed"/>.
		/// </remarks>
		public event EventHandler<ContinueEventArgs> ContinueResponseRequested = delegate { };
	}
}