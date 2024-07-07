using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Abstractions;

namespace Microsoft.Extensions.Caching.InMemory
{
    /// <summary>
    ///     Extension methods for an <see cref="IMemoryCache" />.
    /// </summary>
    internal static class IMemoryCacheExtensions
    {
        /// <summary>
        ///     Tries to get the data from cache, that is, ignoring all exceptions.
        /// </summary>
        /// <param name="cache">The in memory cache.</param>
        /// <param name="key">The key to retrieve from the cache.</param>
        /// <returns>The data of the cache entry, or null if not found or on any error.</returns>
        [Obsolete("Use TryGetCacheData")]
        public static Task<CacheData> TryGetAsync(this IMemoryCache cache, string key)
        {
            try
            {
                if (cache.TryGetValue(key, out byte[] binaryData))
                {
                    return Task.FromResult(binaryData.Deserialize());
                }

                return Task.FromResult(default(CacheData));
            }
            catch (Exception)
            {
                // ignore all exceptions; return null
                return Task.FromResult(default(CacheData));
            }
        }

        public static bool TryGetCacheData(this IMemoryCache cache, string key, out CacheData cacheData)
        {
            var result = false;
            cacheData = default;

            try
            {
                if (cache.TryGetValue(key, out byte[] binaryData))
                {
                    cacheData = binaryData.Deserialize();
                    result = true;
                }
            }
            catch
            {
                // Ignore exception
            }

            return result;
        }

        /// <summary>
        ///     Tries to set a new value to the cache, that is, ignoring all exceptions.
        /// </summary>
        /// <param name="cache">The in memory cache.</param>
        /// <param name="key">The key for this cache entry.</param>
        /// <param name="value">The value of this cache entry.</param>
        /// <param name="absoluteExpirationRelativeToNow">Expiration relative to now.</param>
        /// <returns>A task, when completed, has tried to put the entry into the cache.</returns>
        public static Task TrySetAsync(this IMemoryCache cache, string key, CacheData value, TimeSpan absoluteExpirationRelativeToNow)
        {
            try
            {
                cache.Set(key, value.Serialize(), absoluteExpirationRelativeToNow);
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                // ignore all exceptions
                return Task.FromResult(false);
            }
        }

        public static bool TrySetCacheData(this IMemoryCache cache, string key, CacheData value, TimeSpan absoluteExpirationRelativeToNow)
        {
            var result = false;

            try
            {
                cache.Set(key, value.Serialize(), absoluteExpirationRelativeToNow);
                result = true;
            }
            catch
            {
                // Ignore exceptions
                result = false;
            }

            return result;
        }
    }
}