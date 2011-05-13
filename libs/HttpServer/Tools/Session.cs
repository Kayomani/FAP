using System;
using System.Runtime.Serialization;

namespace HttpServer.Tools
{
    /// <summary>
    /// Base class for sessions.
    /// </summary>
    /// <remarks>
    /// Your class must be tagged with <see cref="ISerializable"/> attribute to be able to use sessions.
    /// </remarks>
    [Serializable]
    [Obsolete("Use the Sessions namespace instead")]
    public class Session
    {
        [ThreadStatic] private static Session _currentSession;

        /// <summary>
        /// Gets or sets when session was accessed last
        /// </summary>
        public DateTime AccessedAt { get; set; }

        /// <summary>
        /// Gets current session.
        /// </summary>
        public static Session CurrentSession
        {
            get { return _currentSession; }
            protected set { _currentSession = value; }
        }

        /// <summary>
        /// Gets or sets session id.
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets when the session was last written to disk.
        /// </summary>
        internal DateTime WrittenAt { get; set; }

		/// <summary>
		/// The session have been changed and should be written to disk.
		/// </summary>
		public void TriggerChanged()
		{
			if (Changed != null)
				Changed(this);
		}

		/// <summary>
		/// Session have been changed.
		/// </summary>
		[field: NonSerialized]
		internal SessionChangedHandler Changed;
    }
}