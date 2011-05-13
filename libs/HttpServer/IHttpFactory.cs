namespace HttpServer
{
    /// <summary>
    /// Get or create components used in the web server framework
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public interface IHttpFactory
    {
        /// <summary>
        /// Get or create a type.
        /// </summary>
        /// <typeparam name="T">Type to create</typeparam>
        /// <returns>Created type.</returns>
        /// <remarks>
        /// Gets or creates types in the framework. 
        /// Check <see cref="HttpFactory"/> for more information on which
        /// types the factory should contain.
        /// </remarks>
        T Get<T>(params object[] constructorArguments) where T : class;
    }
}