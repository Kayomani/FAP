namespace System.Waf
{
    /// <summary>
    /// Configuration settings for the WPF Application Framework (WAF).
    /// </summary>
    public static class WafConfiguration
    {
#if (DEBUG)
        private static bool debug = true;
#else
        private static bool debug = false;
#endif

        /// <summary>
        /// Gets or sets a value indicating whether WAF should run in Debug mode.
        /// </summary>
        /// <remarks>
        /// The Debug mode helps to find errors in the application but it might reduce the
        /// performance.
        /// </remarks>
        public static bool Debug
        {
            get { return debug; }
            set { debug = value; }
        }
    }
}
