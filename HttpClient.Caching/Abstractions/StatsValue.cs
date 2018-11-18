namespace FishApp.Forms.Services.Http.Caching.Abstractions
{
    /// <summary>
    /// An individual statistics value with some pre-computed calculations.
    /// </summary>
    public class StatsValue
    {
        /// <summary>
        /// The number of cache hits.
        /// </summary>
        public long CacheHit { get; set; }

        /// <summary>
        /// The number of cache misses.
        /// </summary>
        public long CacheMiss { get; set; }

        /// <summary>
        /// The total number of requests that were made. Equals <see cref="CacheHit"/> + <see cref="CacheMiss"/>.
        /// </summary>
        public long TotalRequests => this.CacheHit + this.CacheMiss;

        /// <summary>
        /// The percent of requests that were cache hits. Between 0 and 1.
        /// </summary>
        public double CacheHitsPercent => this.CacheHit * 1.0 / this.TotalRequests;

        /// <summary>
        /// The percent of requests that were cache misses. Between 0 and 1.
        /// </summary>
        public double CacheMissPercent => this.CacheMiss * 1.0 / this.TotalRequests;
    }
}