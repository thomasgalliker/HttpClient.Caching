using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Abstractions;

namespace Microsoft.Extensions.Caching.Memory
{
    public static class MemoryCacheExtensions
    {
        /// <summary>
        /// Tries to get the value associated with the given key.
        /// </summary>
        /// <typeparam name="TItem">The type of the object to get.</typeparam>
        /// <param name="cache">The <see cref="IMemoryCache"/> instance this method extends.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">The value associated with the given key.</param>
        /// <returns><c>true</c> if the key was found; <c>false</c> otherwise.</returns>
        public static bool TryGetValue<TItem>(this IMemoryCache cache, object key, [NotNullWhen(true)] out TItem? value)
        {
            if (cache.TryGetValue(key, out var result))
            {
                if (result is TItem item)
                {
                    value = item;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Clears all entries from the cache.
        /// </summary>
        /// <param name="memoryCache">The cache to clear.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="memoryCache" /> is <c>null</c>.</exception>
        /// <exception cref="NotSupportedException">Thrown when the cache implementation cannot be cleared wholesale.</exception>
        public static void Clear(this IMemoryCache memoryCache)
        {
            if (memoryCache is null)
            {
                throw new ArgumentNullException(nameof(memoryCache));
            }

            if (memoryCache is MemoryCache m)
            {
                m.Clear();
                return;
            }

            throw new NotSupportedException($"Clear is not supported for cache type '{memoryCache.GetType().FullName}'.");
        }

        internal static bool TryGetCacheData(this IMemoryCache memoryCache, string key, [NotNullWhen(true)] out CacheData? cacheData)
        {
            var result = false;
            cacheData = null;

            try
            {
                if (TryGetValue<byte[]>(memoryCache, key, out var binaryData))
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
