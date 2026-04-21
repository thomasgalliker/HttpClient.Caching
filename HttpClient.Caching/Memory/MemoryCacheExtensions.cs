namespace Microsoft.Extensions.Caching.Memory
{
    public static class MemoryCacheExtensions
    {
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
    }
}
