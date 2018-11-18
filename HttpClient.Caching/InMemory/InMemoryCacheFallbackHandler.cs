using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Abstractions;

using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.Extensions.Caching.InMemory
{
    /// <summary>
    /// Tries to retrieve the result from the HTTP call, and if it times out or results in an unsuccessful status code, tries to get it from cache.
    /// </summary>
    public class InMemoryCacheFallbackHandler : DelegatingHandler
    {
        public IStatsProvider StatsProvider { get; }
        private readonly TimeSpan maxTimeout;
        private readonly TimeSpan cacheDuration;
        private readonly IMemoryCache responseCache;
        internal const string CacheFallbackKeyPrefix = "cfb";

        /// <summary>
        /// Create a new InMemoryCacheHandler.
        /// </summary>
        /// <param name="innerHandler">The inner handler to retrieve the content from on cache misses.</param>
        /// <param name="maxTimeout">The maximum timeout to wait for the service to respond.</param>
        /// <param name="cacheDuration">The maximum time span the item should be remained in the cache if not renewed before then.</param>
        /// <param name="statsProvider">An <see cref="IStatsProvider"/> that records statistic information about the caching behavior.</param>
        public InMemoryCacheFallbackHandler(HttpMessageHandler innerHandler, TimeSpan maxTimeout, TimeSpan cacheDuration, IStatsProvider statsProvider = null)
            : this(innerHandler, maxTimeout, cacheDuration, statsProvider, new MemoryCache(new MemoryCacheOptions())) {}

        /// <summary>
        /// Used for injecting an IMemoryCache for unit testing purposes.
        /// </summary>
        /// <param name="innerHandler">The inner handler to retrieve the content from on cache misses.</param>
        /// <param name="maxTimeout">The maximum timeout to wait for the service to respond.</param>
        /// <param name="cacheDuration">The maximum time span the item should be remained in the cache if not renewed before then.</param>
        /// <param name="statsProvider">An <see cref="IStatsProvider"/> that records statistic information about the caching behavior.</param>
        /// <param name="cache">The cache to be used.</param>
        internal InMemoryCacheFallbackHandler(HttpMessageHandler innerHandler, TimeSpan maxTimeout, TimeSpan cacheDuration, IStatsProvider statsProvider, IMemoryCache cache) : base(innerHandler ?? new HttpClientHandler())
        {
            this.StatsProvider = statsProvider ?? new StatsProvider(nameof(InMemoryCacheHandler));
            this.maxTimeout = maxTimeout;
            this.cacheDuration = cacheDuration;
            this.responseCache = cache ?? new MemoryCache(new MemoryCacheOptions());
        }

        /// <summary>
        /// Tries to get the value from the cache, and only calls the delegating handler on cache misses.
        /// </summary>
        /// <returns>The HttpResponseMessage from cache, or a newly invoked one.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // only handle GET methods
            if (request.Method != HttpMethod.Get && request.Method != HttpMethod.Head)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var key = CacheFallbackKeyPrefix + request.Method + request.RequestUri.ToString();

            // start 3 tasks
            var httpSendTask = base.SendAsync(request, cancellationToken);
            var timeoutTask = Task.Delay(this.maxTimeout, cancellationToken);
            var cacheTask = this.responseCache.TryGetAsync(key);
            
            // ensure the send task completes (or the timeout task, whatever comes first).
            var firstCompletedTask = await Task.WhenAny(httpSendTask, timeoutTask);

            // timeout occurred
            if (firstCompletedTask == timeoutTask)
            {
                var data = await this.ExtractCachedResponse(request, cacheTask);
                if (data != null)
                {
                    // update the cache after the http task eventually completes, without awaiting it
                    var cacheResultTask = httpSendTask.ContinueWith(t => this.SaveToCache(t.Result, key), TaskContinuationOptions.OnlyOnRanToCompletion);

                    return data;
                }
            }

            // we got it from the HTTP data directly, or it wasn't in the cache and got it after the max timeout value; save that result to the cache and return it
            var response = await httpSendTask;
            
            // try to save it to the cache
            var entry = await this.SaveToCache(response, key);

            // when successful, return that value
            if (entry != null)
            {
                this.StatsProvider.ReportCacheMiss(response.StatusCode);
                return request.PrepareCachedEntry(entry);
            }

            // when unsuccessful, try to get it from the cache, which was the last successful invocation
            var cachedResponse = await this.ExtractCachedResponse(request, cacheTask);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            this.StatsProvider.ReportCacheMiss(response.StatusCode);

            return response;
        }

        private async Task<HttpResponseMessage> ExtractCachedResponse(HttpRequestMessage request, Task<CacheData> cacheTask)
        { 
            // get from cache
            var data = await cacheTask;

            // it's in the cache, return that result
            if (data != null)
            {
                
                // get the data from the cache
                var cachedResponse = request.PrepareCachedEntry(data);
                this.StatsProvider.ReportCacheHit(cachedResponse.StatusCode);
                return cachedResponse;
            }

            return null;
        }

        private async Task<CacheData> SaveToCache(HttpResponseMessage response, string key)
        {
            if ((int) response.StatusCode < 500 && TimeSpan.Zero != this.cacheDuration)
            {
                var entry = await response.ToCacheEntry();
                await this.responseCache.TrySetAsync(key, entry, this.cacheDuration);
                return entry;
            }

            return null;
        }
    }
}
