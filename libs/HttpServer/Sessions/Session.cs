using System;

namespace HttpServer.Sessions
{
    /// <summary>
    /// Session in the system
    /// </summary>
    [Serializable]
    public class Session
    {
        public Session(string sessionId)
        {
            SessionId = sessionId;
        }

        public Session()
        {
            SessionId = Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Gets or sets session id.
        /// </summary>
        public string SessionId { get; set; }
    }
}
