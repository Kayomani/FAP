using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using HttpServer.Logging;
using HttpServer.Messages;

namespace HttpServer.Tools
{
    /// <summary>
    /// Provides sessions.
    /// </summary>
    /// <typeparam name="T">Type of session object</typeparam>
    /// <remarks>
    /// <para>Will always use files for sessions (utilizing the binary formatter), but can
    /// also cache them in memory.</para>
    /// <para>
    /// If caching is enabled, it will only write sessions to disk every 20 seconds if they have
    /// been accessed the last minute (to not keep writing dead sessions to disk).
    /// </para>
    /// </remarks>
    [Obsolete("Use the Sessions namespace instead")]
    public class SessionProvider<T> where T : Session, new()
    {
        [ThreadStatic] private static T _currentSession;
        [ThreadStatic] private static bool _setCookie;
        private readonly Dictionary<string, T> _cachedEntries = new Dictionary<string, T>();
        private readonly string _path;
        private readonly ManualResetEvent _quit = new ManualResetEvent(false);
        private readonly object _syncRoot = new object();
        private readonly Thread _worker;
        private bool _cache;
        private int _sessionExpireSeconds = 60*30; // number of seconds
        private readonly ILogger _logger = LogFactory.CreateLogger(typeof (SessionProvider<T>));

        /// <summary>
        /// Gets or sets session cookie name
        /// </summary>
        public string CookieName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionProvider&lt;T&gt;"/> class.
        /// </summary>
        /// <exception cref="InvalidOperationException">Session type must use [Serializable] attribute.</exception>
        public SessionProvider()
        {
            CookieName = "__session_id";
            // Check if class uses [Serializable]
            bool found = false;
            foreach (object attribute in typeof (T).GetCustomAttributes(false))
            {
                if (!(attribute is SerializableAttribute)) continue;
                found = true;
                break;
            }
            if (!found)
                throw new InvalidOperationException("'" + typeof (T).FullName + "' must use Serializable attribute.");

            // Disk path
            _path = Path.GetTempPath() + "\\WebServerSessions\\";
            Directory.CreateDirectory(_path);

            // Thread writing and deleting sessions.
            _worker = new Thread(OnCheckExpiredSessions) {IsBackground = true};
            _worker.Start();
        }

        /// <summary>
        /// Gets or sets cache
        /// </summary>
        public bool Cache
        {
            get { return _cache; }
            set
            {
                _cache = value;
                if (!_cache)
                    _cachedEntries.Clear();
            }
        }


        /// <summary>
        /// Gets current session.
        /// </summary>
        public T Current
        {
            get { return _currentSession; }
        }

        /// <summary>
        /// Gets or sets number of seconds before a session expired.
        /// </summary>
        /// <remarks>
        /// A session have expired if nothing have accessed it for X seconds. This
        /// class modifies the write time each time it's accessed.
        /// </remarks>
        public int SessionExpireSeconds
        {
            get { return _sessionExpireSeconds; }
            set { _sessionExpireSeconds = value; }
        }

        /// <summary>
        /// Determines if cookie should be set in the response.
        /// </summary>
        private static bool SetCookie
        {
            get { return _setCookie; }
            set { _setCookie = value; }
        }

        /// <summary>
        /// Create a new session.
        /// </summary>
        /// <returns></returns>
        public T Create()
        {
            SetCookie = true;
            _currentSession = new T
                              	{
                              		AccessedAt = DateTime.Now,
                              		SessionId = Guid.NewGuid().ToString().Replace("-", string.Empty),
                              	};

			// need to get event to be able to write sessions to disk.
			if (!_cache)
				_currentSession.Changed = OnSessionChanged;

        	return _currentSession;
        }

    	private void OnSessionChanged(Session session)
    	{
    		session.AccessedAt = DateTime.Now;
    		if (!_cache)
				Save((T)session);
    	}

    	private string GetFileName(string sessionId)
        {
            return _path + sessionId + ".bin";
        }

        /// <summary>
        /// Load session
        /// </summary>
        /// <param name="sessionId">Id of session.</param>
        /// <returns>Session if found; otherwise <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException"><c>sessionId</c> is <c>null</c>.</exception>
        public T Load(string sessionId)
        {
            if (sessionId == null)
                throw new ArgumentNullException("sessionId");

            string fileName = GetFileName(sessionId);

            T session;
            if (_cache && _cachedEntries.TryGetValue(sessionId, out session))
            {
                session.AccessedAt = DateTime.Now;
                return session;
            }

            var formatter = new BinaryFormatter();

            lock (_syncRoot)
            {
                if (!File.Exists(fileName))
                    return null;

                // touch it
                File.SetLastWriteTime(fileName, DateTime.Now);

                try
                {
                    using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                    {
                        session = (T)formatter.Deserialize(stream);
                        session.AccessedAt = DateTime.Now;

						// cache session
						if (_cache)
							_cachedEntries[sessionId] = session;
						else
							session.Changed = OnSessionChanged; //need to write changes.

                        return session;
                    }
                }
                catch(Exception err)
                {
                    _logger.Error("Failed to load session '" + fileName + "'.", err);
                    try
                    {
                        File.Delete(fileName);
                    }
                    catch(Exception)
                    {
                        _logger.Error("Failed to delete same session class.");
                    }

                    return null;
                }
            }
        }

        private void OnCheckExpiredSessions()
        {
            while (true)
            {
                if (_quit.WaitOne(10000))
                    return;

                try
                {
                    // Session must be have accessed last minute to be written
                    DateTime minAccessed = DateTime.Now.AddSeconds(-60);

                    // .. and session must not have been written the last 20 seconds.
                    DateTime maxWritten = DateTime.Now.AddSeconds(-20);

                    // write all sessions that have been recently accessed but not written to disk.
                    foreach (T entry in _cachedEntries.Values)
                    {
                        if (entry.AccessedAt < minAccessed)
                            continue;
                        if (entry.WrittenAt > maxWritten)
                            continue;
                        Save(entry);
                    }
                }
                catch (Exception e)
                {
                    if (e is ThreadAbortException)
                        break;

                    Console.WriteLine(e);
                }

                try
                {
                    // Remove sessions.
                    DateTime minTime = DateTime.Now.AddSeconds(0 - SessionExpireSeconds);
                    foreach (string file in Directory.GetFiles(_path, "*.bin"))
                    {
                        if (File.GetLastWriteTime(file) > minTime)
                            continue;

                        string sessionId = Path.GetFileNameWithoutExtension(file);

                        // don't delete files if sessions have been modified in memory.
                        T session;
                        if (_cache && _cachedEntries.TryGetValue(sessionId, out session) && session.AccessedAt > minTime)
                            continue;

                        lock (_syncRoot)
                        {
                            File.Delete(file);
                            _cachedEntries.Remove(sessionId);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e is ThreadAbortException)
                        break;

                    Console.WriteLine(e);
                }
            }
        }


        /// <summary>
        /// Load session when a new request comes in.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoadSession(object sender, RequestEventArgs e)
        {
            _currentSession = null;

            // always invoke event to prevent previous session from being left.
            RequestCookie cookie = e.Request.Cookies[CookieName];
            string sessionId = cookie != null ? cookie.Value : null;
            if (sessionId == null)
                return;

            _currentSession = Load(sessionId);
        }

        private void OnSendingResponse(object sender, RequestEventArgs e)
        {
            if (!SetCookie || _currentSession == null)
                return;

            SetCookie = false;
            Save(_currentSession);
            e.Response.Cookies.Add(
                new ResponseCookie(new RequestCookie("__session_id", _currentSession.SessionId),
                                   DateTime.Now.AddDays(90)));
        }

        /// <summary>
        /// Save a session to disk.
        /// </summary>
        /// <param name="session">Session to write to disk.</param>
        /// <remarks>
        /// You are responsible for writing sessions to disk if you are not using caching.
        /// </remarks>
        public void Save(T session)
        {
            var formatter = new BinaryFormatter();
            string fileName = GetFileName(session.SessionId);
            lock (_syncRoot)
            {
                using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    formatter.Serialize(stream, session);
                    session.WrittenAt = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Start the session system and hook
        /// </summary>
        /// <param name="webServer"></param>
        public void Start(Server webServer)
        {
            webServer.RequestReceived += OnLoadSession;
            webServer.SendingResponse += OnSendingResponse;
        }

        /// <summary>
        /// Stop session handling
        /// </summary>
        public void Stop()
        {
            _quit.Set();
            if (!_worker.Join(1000))
                _worker.Abort();

            _cachedEntries.Clear();
        }
    }

	/// <summary>
	/// Invoked when a session have been changed and should be written to disc.
	/// </summary>
	internal delegate void SessionChangedHandler(Session session);
}