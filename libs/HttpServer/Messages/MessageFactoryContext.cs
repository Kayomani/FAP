using System;
using System.IO;
using System.Net;
using HttpServer.Headers;
using HttpServer.Logging;
using HttpServer.Messages.Parser;

namespace HttpServer.Messages
{
	/// <summary>
	/// Creates a single message for one of the end points.
	/// </summary>
	/// <remarks>
	/// The factory is 
	/// </remarks>
	public class MessageFactoryContext : IDisposable
	{
		private readonly HeaderFactory _factory;
		private readonly ILogger _logger = LogFactory.CreateLogger(typeof (MessageFactoryContext));
		private readonly MessageFactory _msgFactory;
		private readonly HttpParser _parser;
		private IMessage _message;

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageFactoryContext"/> class.
		/// </summary>
		/// <param name="msgFactory">The MSG factory.</param>
		/// <param name="factory">The factory.</param>
		/// <param name="parser">The parser.</param>
		public MessageFactoryContext(MessageFactory msgFactory, HeaderFactory factory, HttpParser parser)
		{
			_msgFactory = msgFactory;
			_factory = factory;
			_parser = parser;
			parser.HeaderParsed += OnHeader;
			parser.MessageComplete += OnMessageComplete;
			parser.RequestLineParsed += OnRequestLine;
			parser.ResponseLineParsed += OnResponseLine;
			parser.BodyBytesReceived += OnBody;
		}

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
		}

		#endregion

		private void OnBody(object sender, BodyEventArgs e)
		{
			_message.Body.Write(e.Buffer, e.Offset, e.Count);
		}


		/// <summary>
		/// Received a header from parser
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnHeader(object sender, HeaderEventArgs e)
		{
			_logger.Trace(e.Name + ": " + e.Value);
			IHeader header = _factory.Parse(e.Name, e.Value);
			_message.Add(header.Name.ToLower(), header);
			if (header.Name.ToLower() == "expect" && e.Value.ToLower().Contains("100-continue"))
			{
			    Console.WriteLine("Got 100 continue request.");
				ContinueResponseRequested(this, new ContinueEventArgs((IRequest) _message));
			}
		}

		private void OnMessageComplete(object sender, EventArgs e)
		{
			_message.Body.Seek(0, SeekOrigin.Begin);
			if (_message is IRequest)
				RequestCompleted(this, new FactoryRequestEventArgs((IRequest) _message));
			else
				ResponseCompleted(this, new FactoryResponseEventArgs((IResponse) _message));
		}

		private void OnRequestLine(object sender, RequestLineEventArgs e)
		{
			_logger.Trace(e.Method + ": " + e.UriPath);
			_message = _msgFactory.CreateRequest(e.Method, e.UriPath, e.Version);
		}

		private void OnResponseLine(object sender, ResponseLineEventArgs e)
		{
			_logger.Trace(e.StatusCode + ": " + e.ReasonPhrase);
			_message = _msgFactory.CreateResponse(e.Version, e.StatusCode, e.ReasonPhrase);
		}

		/// <summary>
		/// Will continue the parsing until nothing more can be parsed.
		/// </summary>
		/// <param name="buffer">buffer to parse</param>
		/// <param name="offset">where to start in the buffer</param>
		/// <param name="length">number of bytes to process.</param>
		/// <returns>Position where parser stopped parsing.</returns>
		/// <exception cref="ParserException">Parsing failed.</exception>
		public int Parse(byte[] buffer, int offset, int length)
		{
			return _parser.Parse(buffer, offset, length);
		}

		/// <summary>
		/// Reset parser.
		/// </summary>
		/// <remarks>
		/// Something failed, reset parser so it can start on a new request.
		/// </remarks>
		public void Reset()
		{
			_parser.Reset();
		}

		/// <summary>
		/// A request have been successfully parsed.
		/// </summary>
		public event EventHandler<FactoryRequestEventArgs> RequestCompleted = delegate { };

		/// <summary>
		/// A response have been successfully parsed.
		/// </summary>
		public event EventHandler<FactoryResponseEventArgs> ResponseCompleted = delegate { };

		/// <summary>
		/// Client asks if he may continue.
		/// </summary>
		/// <remarks>
		/// If the body is too large or anything like that you should respond <see cref="HttpStatusCode.ExpectationFailed"/>.
		/// </remarks>
		public event EventHandler<ContinueEventArgs> ContinueResponseRequested = delegate { };
	}

	/// <summary>
	/// Used to notify about 100-continue header.
	/// </summary>
	public class ContinueEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ContinueEventArgs"/> class.
		/// </summary>
		/// <param name="request">request that want to continue.</param>
		public ContinueEventArgs(IRequest request)
		{
			Request = request;
		}

		/// <summary>
		/// Gets request that want to continue
		/// </summary>
		public IRequest Request { get; private set; }
	}
}