using System;
using System.Collections.Generic;

namespace HttpServer.Tools.Properties
{
    /// <summary>
    /// Used to get or set properties on objects.
    /// </summary>
    /// <remarks>
    /// This class should be a bit faster than the standard reflection.
    /// </remarks>
    public class PropertyProvider
    {
        private static Dictionary<Type, CachedType> _types = new Dictionary<Type, CachedType>();
        private static  EmitReflector _emitReflector = new EmitReflector();

        /// <summary>
        /// Get cached type.
        /// </summary>
        /// <param name="type">Type to get/set properties in</param>
        /// <returns>Type to use</returns>
        public static ICachedType Get(Type type)
        {
            CachedType cachedType;
            if (_types.TryGetValue(type, out cachedType))
                return cachedType;

            lock (_types)
            {
                if (_types.TryGetValue(type, out cachedType))
                    return cachedType;

                cachedType = new CachedType(type, _emitReflector);
                _types[type] = cachedType;
                return cachedType;
            }
        }
    }
}
