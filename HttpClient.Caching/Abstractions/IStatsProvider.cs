using System.Net;

namespace Microsoft.Extensions.Caching.Abstractions
{
    /// <summary>
    ///     Provides statistical information of the cache hits or misses.
    /// </summary>
    public interface IStatsProvider
    {
        /// <summary>
        ///     Report a cache hit.
        /// </summary>
        /// <param name="statusCode">The status code that experienced a cache hit.</param>
        void ReportCacheHit(HttpStatusCode statusCode);

        /// <summary>
        ///     Report a cache miss.
        /// </summary>
        /// <param name="statusCode">The status code that experienced a cache miss.</param>
        void ReportCacheMiss(HttpStatusCode statusCode);

        /// <summary>
        ///     Gets the current statistics.
        /// </summary>
        /// <returns>A <see cref="StatsResult" /> object that contains the statistics at the current time.</returns>
        StatsResult GetStatistics();
    }
}