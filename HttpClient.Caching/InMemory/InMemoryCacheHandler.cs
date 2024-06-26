using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Abstractions;

namespace Microsoft.Extensions.Caching.InMemory
{
    /// <summary>
    ///     Tries to retrieve the result from an InMemory cache, and if that's not available, gets the value from the
    ///     underlying handler and caches that result.
    /// </summary>
    public class InMemoryCacheHandler : DelegatingHandler
    {
        private static HashSet<HttpMethod> CachedHttpMethods = new HashSet<HttpMethod>
        {
            HttpMethod.Get,
            HttpMethod.Head
        };

        public IStatsProvider StatsProvider { get; }

        private readonly IDictionary<HttpStatusCode, TimeSpan> cacheExpirationPerHttpResponseCode;
        private readonly IMemoryCache responseCache;

        /// <summary>
        /// Cache key provider being used
        /// </summary>
        public ICacheKeysProvider CacheKeysProvider { get; }

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
        /// <param name="cacheKeysProvider">
        ///     An <see cref="ICacheKeysProvider"/> that provides keys to retrieve and store items in the cache
        /// </param>
        public InMemoryCacheHandler(HttpMessageHandler innerHandler = null,
            IDictionary<HttpStatusCode, TimeSpan> cacheExpirationPerHttpResponseCode = null,
            IStatsProvider statsProvider = null,
            ICacheKeysProvider cacheKeysProvider = null)
            : this(innerHandler,
                   cacheExpirationPerHttpResponseCode,
                   statsProvider,
                   new MemoryCache(new MemoryCacheOptions()),
                   cacheKeysProvider)
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
        /// <param name="cacheKeysProvider">The <see cref="ICacheKeysProvider"/> cache keys provider to use</param>
        internal InMemoryCacheHandler(
            HttpMessageHandler innerHandler,
            IDictionary<HttpStatusCode, TimeSpan> cacheExpirationPerHttpResponseCode,
            IStatsProvider statsProvider,
            IMemoryCache cache,
            ICacheKeysProvider cacheKeysProvider)
            : base(innerHandler ?? new HttpClientHandler())
        {
            this.StatsProvider = statsProvider ?? new StatsProvider(nameof(InMemoryCacheHandler));
            this.cacheExpirationPerHttpResponseCode = cacheExpirationPerHttpResponseCode ?? new Dictionary<HttpStatusCode, TimeSpan>();
            this.responseCache = cache ?? new MemoryCache(new MemoryCacheOptions());
            this.CacheKeysProvider = cacheKeysProvider ?? new DefaultCacheKeysProvider();
        }

        /// <summary>
        ///     Allows to invalidate the cache.
        /// </summary>
        /// <param name="uri">The URI to invalidate.</param>
        /// <param name="httpMethod">An optional <see cref="HttpMethod"/> to invalidate. If none is provided, the cache is cleaned for all methods.</param>
        public void InvalidateCache(Uri uri, HttpMethod httpMethod = null)
        {
            var httpMethods = httpMethod != null
                ? new HashSet<HttpMethod> { httpMethod }
                : CachedHttpMethods;

            foreach (var method in httpMethods)
            {
                var httpRequestMessage = new HttpRequestMessage(method, uri);
                var key = this.CacheKeysProvider.GetKey(httpRequestMessage);
                this.responseCache.Remove(key);
            }
        }

        /// <summary>
        ///     Tries to get the value from the cache, and only calls the delegating handler on cache misses.
        /// </summary>
        /// <returns>The HttpResponseMessage from cache, or a newly invoked one.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string key = null;

            // Gets the data from cache, and returns the data if it's a cache hit
            var isCachedHttpMethod = CachedHttpMethods.Contains(request.Method);
            if (isCachedHttpMethod)
            {
                key = this.CacheKeysProvider.GetKey(request);

                if (this.TryGetCachedHttpResponseMessage(request, key, out var cachedResponse))
                {
                    return cachedResponse;
                }
            }

            // Cache misses need to ask the inner handler for an actual response
            var response = await base.SendAsync(request, cancellationToken);

            // Puts the retrieved response into the cache and returns the cached entry
            if (isCachedHttpMethod)
            {
                var absoluteExpirationRelativeToNow = response.StatusCode.GetAbsoluteExpirationRelativeToNow(this.cacheExpirationPerHttpResponseCode);

                this.StatsProvider.ReportCacheMiss(response.StatusCode);

                if (TimeSpan.Zero != absoluteExpirationRelativeToNow)
                {
                    var entry = await response.ToCacheEntryAsync();
                    await this.responseCache.TrySetAsync(key, entry, absoluteExpirationRelativeToNow);
                    return request.PrepareCachedEntry(entry);
                }
            }

            // Returns the original response
            return response;
        }

#if NET5_0_OR_GREATER
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string key = null;

            // Gets the data from cache, and returns the data if it's a cache hit
            var isCachedHttpMethod = CachedHttpMethods.Contains(request.Method);
            if (isCachedHttpMethod)
            {
                key = this.CacheKeysProvider.GetKey(request);

                if (this.TryGetCachedHttpResponseMessage(request, key, out var cachedResponse))
                {
                    return cachedResponse;
                }
            }

            var response = base.Send(request, cancellationToken);

            // Puts the retrieved response into the cache and returns the cached entry
            if (isCachedHttpMethod)
            {
                var absoluteExpirationRelativeToNow = response.StatusCode.GetAbsoluteExpirationRelativeToNow(this.cacheExpirationPerHttpResponseCode);

                this.StatsProvider.ReportCacheMiss(response.StatusCode);

                if (TimeSpan.Zero != absoluteExpirationRelativeToNow)
                {
                    var cacheData = response.ToCacheEntry();
                    this.responseCache.TrySetCacheData(key, cacheData, absoluteExpirationRelativeToNow);
                    return request.PrepareCachedEntry(cacheData);
                }
            }

            // Returns the original response
            return response;
        }
#endif

        private bool TryGetCachedHttpResponseMessage(HttpRequestMessage request, string key, out HttpResponseMessage cachedResponse)
        {
            if (this.responseCache.TryGetCacheData(key, out var cacheData))
            {
                cachedResponse = request.PrepareCachedEntry(cacheData);
                this.StatsProvider.ReportCacheHit(cachedResponse.StatusCode);
                return true;
            }

            cachedResponse = default;
            return false;
        }
    }
}