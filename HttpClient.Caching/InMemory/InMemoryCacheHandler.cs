using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Abstractions;

using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.Extensions.Caching.InMemory
{
    /// <summary>
    ///     Tries to retrieve the result from an InMemory cache, and if that's not available, gets the value from the
    ///     underlying handler and caches that result.
    /// </summary>
    public class InMemoryCacheHandler : DelegatingHandler
    {
        public IStatsProvider StatsProvider { get; }

        private readonly IDictionary<HttpStatusCode, TimeSpan> cacheExpirationPerHttpResponseCode;
        private readonly IMemoryCache responseCache;

        /// <summary>
        ///     Create a new InMemoryCacheHandler.
        /// </summary>
        /// <param name="innerHandler">The inner handler to retrieve the content from on cache misses.</param>
        /// <param name="cacheExpirationPerHttpResponseCode">
        ///     A mapping of HttpStatusCode to expiration times. If unspecified takes
        ///     a default value.
        /// </param>
        /// <param name="statsProvider">
        ///     An <see cref="IStatsProvider" /> that records statistic information about the caching
        ///     behavior.
        /// </param>
        public InMemoryCacheHandler(HttpMessageHandler innerHandler = null, IDictionary<HttpStatusCode, TimeSpan> cacheExpirationPerHttpResponseCode = null, IStatsProvider statsProvider = null)
            : this(innerHandler, cacheExpirationPerHttpResponseCode, statsProvider, new MemoryCache(new MemoryCacheOptions()))
        {
        }

        /// <summary>
        ///     Used for injecting an IMemoryCache for unit testing purposes.
        /// </summary>
        /// <param name="innerHandler">The inner handler to retrieve the content from on cache misses.</param>
        /// <param name="cacheExpirationPerHttpResponseCode">
        ///     A mapping of HttpStatusCode to expiration times. If unspecified takes
        ///     a default value.
        /// </param>
        /// <param name="statsProvider">
        ///     An <see cref="IStatsProvider" /> that records statistic information about the caching
        ///     behavior.
        /// </param>
        /// <param name="cache">The cache to be used.</param>
        internal InMemoryCacheHandler(HttpMessageHandler innerHandler, IDictionary<HttpStatusCode, TimeSpan> cacheExpirationPerHttpResponseCode, IStatsProvider statsProvider, IMemoryCache cache)
            : base(innerHandler ?? new HttpClientHandler())
        {
            this.StatsProvider = statsProvider ?? new StatsProvider(nameof(InMemoryCacheHandler));
            this.cacheExpirationPerHttpResponseCode = cacheExpirationPerHttpResponseCode ?? new Dictionary<HttpStatusCode, TimeSpan>();
            this.responseCache = cache ?? new MemoryCache(new MemoryCacheOptions());
        }

        /// <summary>
        ///     Allows to invalidate the cache.
        /// </summary>
        /// <param name="uri">The URI to invalidate.</param>
        /// <param name="method">An optional method to invalidate. If none is provided, the cache is cleaned for all methods.</param>
        public void InvalidateCache(Uri uri, HttpMethod method = null)
        {
            var methods = method != null ? new[] { method } : new[] { HttpMethod.Get, HttpMethod.Head };
            foreach (var m in methods)
            {
                var key = m + uri.ToString();
                this.responseCache.Remove(key);
            }
        }

        /// <summary>
        ///     Tries to get the value from the cache, and only calls the delegating handler on cache misses.
        /// </summary>
        /// <returns>The HttpResponseMessage from cache, or a newly invoked one.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var key = request.Method + request.RequestUri.ToString();
            // gets the data from cache, and returns the data if it's a cache hit
            if (request.Method == HttpMethod.Get || request.Method == HttpMethod.Head)
            {
                var data = await this.responseCache.TryGetAsync(key);
                if (data != null)
                {
                    var cachedResponse = request.PrepareCachedEntry(data);
                    this.StatsProvider.ReportCacheHit(cachedResponse.StatusCode);
                    return cachedResponse;
                }
            }

            // cache misses need to ask the inner handler for an actual response
            var response = await base.SendAsync(request, cancellationToken);

            // puts the retrieved response into the cache and returns the cached entry
            if (request.Method == HttpMethod.Get || request.Method == HttpMethod.Head)
            {
                var absoluteExpirationRelativeToNow = response.StatusCode.GetAbsoluteExpirationRelativeToNow(this.cacheExpirationPerHttpResponseCode);

                this.StatsProvider.ReportCacheMiss(response.StatusCode);

                if (TimeSpan.Zero != absoluteExpirationRelativeToNow)
                {
                    var entry = await response.ToCacheEntry();
                    await this.responseCache.TrySetAsync(key, entry, absoluteExpirationRelativeToNow);
                    return request.PrepareCachedEntry(entry);
                }
            }

            // returns the original response
            return response;
        }
    }
}