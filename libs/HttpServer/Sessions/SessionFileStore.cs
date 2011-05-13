using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using HttpServer.Logging;

namespace HttpServer.Sessions
{
    ///<summary>
    /// Stores sessions in files.
    ///</summary>
    /// <remarks>
    /// All session parameters must be serializable.
    /// </remarks>
    public class SessionFileStore : ISessionStore
    {
        private readonly string _serverName;
        private string _path;
        private BinaryFormatter _formatter = new BinaryFormatter();
        private Timer _cleanSessionTimer;
        private int _sessionLifetime = 60*20; // 20 minutes
        private ILogger _logger = Logging.LogFactory.CreateLogger(typeof (ISessionStore));
        private static object _synclock = new object();

        public SessionFileStore(string serverName)
        {
            _serverName = serverName;
            CreateTempDirectory();
            _cleanSessionTimer = new Timer(OnCleanSessions, null, 30000, 30000);

        }

        private void OnCleanSessions(object state)
        {
            var expires = DateTime.Now.AddSeconds(0 - _sessionLifetime);
            foreach (var fileName in Directory.GetFiles(_path, "*.session"))
            {
                try
                {
                    
                    var info = new FileInfo(fileName);
                    if (info.LastWriteTime < expires)
                    {
                        lock(_synclock)
                            File.Delete(fileName);
                    }
                }
                catch(Exception err)
                {
                    _logger.Error("Failed to remove session: " + fileName, err);
                }
            }
        }

        private void CreateTempDirectory()
        {
            _path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _path=Path.Combine(_path, _serverName);
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
        }

        private string GetSessionFileName(string sessionId)
        {
            return Path.Combine(_path, sessionId + ".session");
        }


        /// <summary>
        /// Saves the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        public void Save(Session session)
        {
            lock (_synclock)
            {
                _logger.Info("Saving it");
                using (var stream = new FileStream(GetSessionFileName(session.SessionId), FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    _formatter.Serialize(stream, session);
                }
            }
        }

        /// <summary>
        /// Touches the specified session
        /// </summary>
        /// <param name="id">Session id.</param>
        /// <remarks>
        /// Used to prevent sessions from expiring.
        /// </remarks>
        public void Touch(string id)
        {
            var filename = GetSessionFileName(id);
            try
            {
                lock (_synclock)
                {
                    _logger.Info("Touching it");
                    File.SetLastWriteTime(filename, DateTime.Now);
                }
            }
            catch(Exception err)
            {
                _logger.Warning("Failed to touch " + filename, err);
            }
        }

        /// <summary>
        /// Loads a session
        /// </summary>
        /// <param name="id">Session id.</param>
        /// <returns>Session if found; otherwise <c>null</c>.</returns>
        public Session Load(string id)
        {
            var filename = GetSessionFileName(id);
            if (!File.Exists(filename))
                return null;

            lock (_synclock)
            {
                _logger.Info("Loading..");
                using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return (Session)_formatter.Deserialize(stream);
                }
            }
        }

        public void Delete(string id)
        {
            _logger.Info("deleting");
            var filename = GetSessionFileName(id);
            lock (_synclock)
            {
                if (File.Exists(filename))
                    File.Delete(filename);
            }
        }
    }
}
