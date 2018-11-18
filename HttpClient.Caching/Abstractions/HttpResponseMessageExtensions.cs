using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FishApp.Forms.Services.Http.Caching.Abstractions
{
    /// <summary>
    /// Extension methods of the HttpResponseMessage that are related to the caching functionality.
    /// </summary>
    public static class HttpResponseMessageExtensions
    {
        public static async Task<CacheData> ToCacheEntry(this HttpResponseMessage response)
        {
            var data = await response.Content.ReadAsByteArrayAsync();
            var copy = new HttpResponseMessage{ ReasonPhrase = response.ReasonPhrase, StatusCode = response.StatusCode, Version = response.Version };
            var headers = response.Headers.Where(h => h.Value != null && h.Value.Any()).ToDictionary(h => h.Key, h => h.Value);
            var contentHeaders = response.Content.Headers.Where(h => h.Value != null && h.Value.Any()).ToDictionary(h => h.Key, h => h.Value);
            var entry = new CacheData(data, copy, headers, contentHeaders);
            return entry;
        }

        /// <summary>
        /// Prepares the cached entry to be consumed by the caller, notably by setting the content.
        /// </summary>
        /// <param name="request">The request that invoked retrieving this response and need to be attached to the response.</param>
        /// <param name="cachedData">The deserialized data from the cache.</param>
        /// <returns>A valid HttpResponseMessage that can be consumed by the caller of this message handler.</returns>
        public static HttpResponseMessage PrepareCachedEntry(this HttpRequestMessage request, CacheData cachedData)
        {
            var response = cachedData.CachableResponse;
            if (cachedData.Headers != null)
            {
                foreach (var kvp in cachedData.Headers)
                {
                    response.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                }
            }

            response.Content = new ByteArrayContent(cachedData.Data);
            if (cachedData.ContentHeaders != null)
            {
                foreach (var kvp in cachedData.ContentHeaders)
                {
                    response.Content.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                }
            }
            response.RequestMessage = request;
            return response;
        }
    }
}