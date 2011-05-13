namespace HttpServer
{
    /// <summary>
    /// Result of processing.
    /// </summary>
    public enum ProcessingResult
    {
        /// <summary>
        /// Continue with the next handler
        /// </summary>
        Continue,

        /// <summary>
        /// No more handlers can process the request.
        /// </summary>
        /// <remarks>
        /// The server will process the response object and
        /// generate a HTTP response from it.
        /// </remarks>
        SendResponse,

        /// <summary>
        /// Response have been sent back by the handler.
        /// </summary>
        /// <remarks>
        /// This option should only be used if you are streaming
        /// something or sending back a custom result. The server will
        /// not process the response object or send anything back
        /// to the client.
        /// </remarks>
        Abort
    }
}