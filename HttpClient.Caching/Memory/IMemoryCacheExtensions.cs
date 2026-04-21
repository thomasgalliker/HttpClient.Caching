using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Abstractions;

namespace Microsoft.Extensions.Caching.Memory
{
    internal static class IMemoryCacheExtensions
    {
        internal static bool TryGetCacheData(this IMemoryCache memoryCache, string key, [NotNullWhen(true)] out CacheData? cacheData)
        {
            var result = false;
            cacheData = null;

            try
            {
                if (memoryCache.TryGetValue<byte[]>(key, out var binaryData))
                {
                    cacheData = binaryData!.Deserialize();
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
        /// <param name="cacheData">The value of this cache entry.</param>
        /// <param name="absoluteExpirationRelativeToNow">Expiration relative to now.</param>
        /// <returns>A task, when completed, has tried to put the entry into the cache.</returns>
        internal static Task<bool> TrySetAsync(this IMemoryCache cache, string key, CacheData cacheData, TimeSpan absoluteExpirationRelativeToNow)
        {
            try
            {
                cache.Set(key, cacheData.Serialize(), absoluteExpirationRelativeToNow);
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                // ignore all exceptions
                return Task.FromResult(false);
            }
        }

        internal static bool TrySetCacheData(this IMemoryCache cache, string key, CacheData value, TimeSpan absoluteExpirationRelativeToNow)
        {
            bool result;

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
