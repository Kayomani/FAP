using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using HttpServer.Authentication;
using HttpServer.BodyDecoders;
using HttpServer.Headers;
using HttpServer.Logging;
using HttpServer.Messages;
using HttpServer.Modules;
using HttpServer.Routing;

namespace HttpServer
{
	/// <summary>
	/// Http server.
	/// </summary>
	public class Server
	{
		[ThreadStatic] private static Server _server;
		private readonly BodyDecoderCollection _bodyDecoders = new BodyDecoderCollection();
		private readonly List<IHttpListener> _listeners = new List<IHttpListener>();
		private readonly ILogger _logger = LogFactory.CreateLogger(typeof (Server));
		private readonly List<IModule> _modules = new List<IModule>();
		private readonly List<IRouter> _routers = new List<IRouter>();
		private HttpFactory _factory;
		private bool _isStarted;

	    /// <summary>
		/// Initializes a new instance of the <see cref="Server"/> class.
		/// </summary>
		/// <param name="factory">Factory used to create objects used in this library.</param>
		public Server(HttpFactory factory)
		{
	        ServerName = "C# WebServer";
			_server = this;
			_factory = factory;
            AuthenticationProvider = new AuthenticationProvider();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Server"/> class.
		/// </summary>
		public Server()
            : this(new HttpFactory())
		{
		}

		/// <summary>
		/// Gets current server.
		/// </summary>
		/// <remarks>
		/// Only valid when a request have been received and is being processed.
		/// </remarks>
		public static Server Current
		{
			get { return _server; }
		}

	    /// <summary>
	    /// Gets or sets server name.
	    /// </summary>
	    /// <remarks>
	    /// Used in the "Server" header when serving requests.
	    /// </remarks>
	    public string ServerName { get; set; }

		/// <summary>
		/// Add a decoder.
		/// </summary>
		/// <param name="decoder">decoder to add</param>
		/// <remarks>
		/// Adding zero decoders will make the server add the 
		/// default ones which is <see cref="MultiPartDecoder"/> and <see cref="UrlDecoder"/>.
		/// </remarks>
		public void Add(IBodyDecoder decoder)
		{
			_bodyDecoders.Add(decoder);
		}

		/// <summary>
		/// Add a new router.
		/// </summary>
		/// <param name="router">Router to add</param>
		/// <exception cref="InvalidOperationException">Server have been started.</exception>
		public void Add(IRouter router)
		{
			if (_isStarted)
				throw new InvalidOperationException("Server have been started.");

			_routers.Add(router);
		}

		/// <summary>
		/// Add a file module
		/// </summary>
		/// <param name="module">Module to add</param>
		/// <exception cref="ArgumentNullException"><c>module</c> is <c>null</c>.</exception>
		/// <exception cref="InvalidOperationException">Cannot add modules when server have been started.</exception>
		public void Add(IModule module)
		{
			if (module == null)
				throw new ArgumentNullException("module");
			if (_isStarted)
				throw new InvalidOperationException("Cannot add modules when server have been started.");
			_modules.Add(module);
		}

		/// <summary>
		/// Add a HTTP listener.
		/// </summary>
		/// <param name="listener"></param>
		/// <exception cref="InvalidOperationException">Listener have been started.</exception>
		public void Add(IHttpListener listener)
		{
			if (listener.IsStarted)
				throw new InvalidOperationException("Listener have been started.");

			_listeners.Add(listener);
		}

		protected virtual void DecodeBody(IRequest request)
		{
			Encoding encoding = null;
			if (request.ContentType != null)
			{
				string encodingStr = request.ContentType.Parameters["Encoding"];
				if (!string.IsNullOrEmpty(encodingStr))
					encoding = Encoding.GetEncoding(encodingStr);
			}

			if (encoding == null)
				encoding = Encoding.UTF8;

			// process body.
			DecodedData data = _bodyDecoders.Decode(request.Body, request.ContentType, encoding);
			if (data == null)
				return;

		    if (!(request is Request))
		        throw new InternalServerException("Request object has to derive from Request (sorry for breaking LSP).");

            var r = (Request) request;
	        r.Files = data.Files;
	        r.Form = data.Parameters;
		}

		private void Listener_OnErrorPage(object sender, ErrorPageEventArgs e)
		{
			_server = this;
		    DisplayErrorPage(e.Context, e.Exception);
		    e.IsHandled = true;
		}

        /// <summary>
        /// An error have occurred and we need to send a result pack to the client
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="exception">The exception.</param>
        /// <remarks>
        /// Invoke base class (<see cref="Server"/>) to send the contents
        /// of <see cref="IHttpContext.Response"/>.
        /// </remarks>
	    protected virtual void DisplayErrorPage(IHttpContext context, Exception exception)
	    {
	        var httpException = exception as HttpException;
            if (httpException != null)
            {
                context.Response.Reason = httpException.Code.ToString();
                context.Response.Status = httpException.Code;
            }
            else
            {
                context.Response.Reason = "Internal Server Error";
                context.Response.Status = HttpStatusCode.InternalServerError;
            }


            var args = new ErrorPageEventArgs(context) { Exception = exception };
            ErrorPageRequested(this, args);

            ResponseWriter writer = HttpFactory.Current.Get<ResponseWriter>();
            if (args.IsHandled)
                writer.Send(context, context.Response);
            else
            {
                writer.SendErrorPage(context, context.Response, exception);
                args.IsHandled = true;
            }
	    }


	    private void OnRequest(object sender, RequestEventArgs e)
		{
			_server = this;

			Exception exception;
			try
			{
                var result = HandleRequest(e);
                if (result != ProcessingResult.Continue)
					return;

				exception = null;
			}
			catch (HttpException err)
			{
				_logger.Error("Got an HTTP exception.", err);
				e.Response.Status = err.Code;
				e.Response.Reason = err.Message;
				exception = err;
			}
			catch (Exception err)
			{
				_logger.Error("Got an unhandled exception.", err);
				exception = err;
				e.Response.Status = HttpStatusCode.InternalServerError;
				e.Response.Reason = "Failed to process request.";
			}


			if (exception == null)
			{
				e.Response.Status = HttpStatusCode.NotFound;
				e.Response.Reason = "Requested resource is not found. Sorry ;(";
                exception = new HttpException(HttpStatusCode.NotFound, "Failed to find uri " + e.Request.Uri);
            }
	        DisplayErrorPage(e.Context, exception);
	        e.IsHandled = true;
		}

		private ProcessingResult HandleRequest(RequestEventArgs e)
		{
			var context = new RequestContext
			              	{
			              		HttpContext = e.Context,
			              		Request = e.Request,
			              		Response = e.Response
			              	};

		    OnAuthentication(context);
		    OnBeforeRequest(context);
            PrepareRequest(this, e);


			if (e.Request.ContentLength.Value > 0)
				DecodeBody(e.Request);

			// Process routers.
			ProcessingResult result = ProcessRouters(context);
			if (ProcessResult(result, e))
				_logger.Debug("Routers processed the request.");


			// process modules.
			result = ProcessModules(context);
			if (ProcessResult(result, e))
				return result;

            RequestReceived(this, e);


			return ProcessingResult.Continue;
		}

        /// <summary>
        /// Called before anything else.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <remarks>
        /// Looks after a <see cref="AuthorizationHeader"/> in the request and will
        /// use the <see cref="AuthenticationProvider"/> if found.
        /// </remarks>
	    protected virtual void OnAuthentication(RequestContext context)
	    {
	        var authHeader = (AuthorizationHeader) context.Request.Headers[AuthorizationHeader.Key];
            if (authHeader != null)
                AuthenticationProvider.Authenticate(context.Request);
	    }

        /// <summary>
        /// Requests authentication from the user.
        /// </summary>
        /// <param name="realm">Host/domain name that the server hosts.</param>
        /// <remarks>
        /// Used when calculating hashes in Digest authentication. 
        /// </remarks>
        /// <seealso cref="DigestAuthentication"/>
        /// <seealso cref="DigestAuthentication.GetHA1"/>
        protected virtual void RequestAuthentication(string realm)
        {
            AuthenticationProvider.CreateChallenge(HttpContext.Current.Response, realm);
        }


	    /// <summary>
        /// All server modules are about to be invoked.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <remarks>
        /// Called when routers have been invoked but no modules yet.
        /// </remarks>
	    protected virtual void OnBeforeModules(RequestContext context)
	    {
	        
	    }

        /// <summary>
        /// A request have arrived but not yet been processed yet.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <remarks>
        /// Default implementation adds a <c>Date</c> header and <c>Server</c> header.
        /// </remarks>
	    protected virtual void OnBeforeRequest(RequestContext context)
	    {
            context.Response.Add(new DateHeader("Date", DateTime.UtcNow));
            context.Response.Add(new StringHeader("Server", ServerName));
	    }

	    /// <summary>
		/// Go through all modules and check if any of them can handle the current request.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		private ProcessingResult ProcessModules(RequestContext context)
		{
            OnBeforeModules(context);
			foreach (IModule module in _modules)
			{
				ProcessingResult result = module.Process(context);
				if (result != ProcessingResult.Continue)
				{
					_logger.Debug(module.GetType().Name + ": " + result);
					return result;
				}
			}

			return ProcessingResult.Continue;
		}

		/// <summary>
		/// Process result (check if it should be sent back or not)
		/// </summary>
		/// <param name="result"></param>
		/// <param name="e"></param>
		/// <returns><c>true</c> if request was processed properly.; otherwise <c>false</c>.</returns>
		protected virtual bool ProcessResult(ProcessingResult result, RequestEventArgs e)
		{
			if (result == ProcessingResult.Abort)
			{
				e.IsHandled = true;
				return true;
			}

			if (result == ProcessingResult.SendResponse)
			{
				SendResponse(e.Context, e.Request, e.Response);
				e.IsHandled = true;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Processes all routers.
		/// </summary>
		/// <param name="context">Request context.</param>
		/// <returns>Processing result.</returns>
		private ProcessingResult ProcessRouters(RequestContext context)
		{
			foreach (IRouter router in _routers)
			{
			    if (router.Process(context) != ProcessingResult.SendResponse) 
                    continue;

			    _logger.Debug(router.GetType().Name + " sends the response.");
			    return ProcessingResult.SendResponse;
			}

			return ProcessingResult.Continue;
		}


		/// <summary>
		/// Send a response.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="request"></param>
		/// <param name="response"></param>
		protected void SendResponse(IHttpContext context, IRequest request, IResponse response)
		{
			SendingResponse(this, new RequestEventArgs(context, request, response));

			var generator = HttpFactory.Current.Get<ResponseWriter>();
			generator.Send(context, response);
			if (request.Connection != null && request.Connection.Type == ConnectionType.Close)
			{
				context.Stream.Close();
				_logger.Debug("Closing connection.");
			}
		}

		/// <summary>
		/// Start http server.
		/// </summary>
		/// <param name="backLog">Number of pending connections.</param>
		public void Start(int backLog)
		{
			if (_isStarted)
				return;

			if (_bodyDecoders.Count == 0)
			{
				_bodyDecoders.Add(new MultiPartDecoder());
				_bodyDecoders.Add(new UrlDecoder());
			}

            foreach (IHttpListener listener in _listeners)
			{
				listener.ErrorPageRequested += Listener_OnErrorPage;
				listener.RequestReceived += OnRequest;
                listener.ContentLengthLimit = ContentLengthLimit;
				listener.Start(backLog);
			}

			_isStarted = true;
		}

	    /// <summary>
	    /// Gets or sets number of bytes that a body can be.
	    /// </summary>
	    /// <remarks>
	    /// <para>
        /// Used to determine the answer to a 100-continue request.
        /// </para>
        /// <para>
        ///  0 = turned off.
        /// </para>
	    /// </remarks>
	    public int ContentLengthLimit { get; set; }

	    /// <summary>
	    /// Gets the authentication provider.
	    /// </summary>
	    /// <remarks>
	    /// A authentication provider is used to keep track of all authentication types
	    /// that can be used.
	    /// </remarks>
	    public AuthenticationProvider AuthenticationProvider { get; private set; }

	    /// <summary>
		/// Invoked just before a response is sent back to the client.
		/// </summary>
		public event EventHandler<RequestEventArgs> SendingResponse = delegate { };

		/// <summary>
		/// Invoked *after* the web server has tried to handled the request.
		/// </summary>
		/// <remarks>
		/// The event can be used to handle the request after all routes and modules
		/// have tried to process the request.
		/// </remarks>
		public event EventHandler<RequestEventArgs> RequestReceived = delegate { };

        /// <summary>
        /// Invoked *before* the web server has tried to handled the request.
        /// </summary>
        /// <remarks>
        /// Event can be used to load a session from a cookie or to force
        /// authentication or anything other you might need t do before a request
        /// is handled.
        /// </remarks>
        public event EventHandler<RequestEventArgs> PrepareRequest = delegate { };

		/// <summary>
		/// An error page have been requested.
		/// </summary>
		public event EventHandler<ErrorPageEventArgs> ErrorPageRequested = delegate { };
	}
}