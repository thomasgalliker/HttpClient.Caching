using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.Abstractions
{
    /// <summary>
    ///     Extension methods of the HttpResponseMessage that are related to the caching functionality.
    /// </summary>
    public static class HttpResponseMessageExtensions
    {
        public static async Task<CacheData> ToCacheEntryAsync(this HttpResponseMessage httpResponseMessage)
        {
            var contentBytes = await httpResponseMessage.Content.ReadAsByteArrayAsync();
            return httpResponseMessage.ToCacheEntry(contentBytes);
        }

#if NET5_0_OR_GREATER
        public static CacheData ToCacheEntry(this HttpResponseMessage httpResponseMessage)
        {
            using var contentStream = httpResponseMessage.Content.ReadAsStream();
            using (var memoryStream = new MemoryStream())
            {
                contentStream.CopyTo(memoryStream);
                var contentBytes =  memoryStream.ToArray();
                return httpResponseMessage.ToCacheEntry(contentBytes);
            }
        }
#endif

        public static CacheData ToCacheEntry(this HttpResponseMessage httpResponseMessage, byte[] contentBytes)
        {

            var httpResponseMessageCopy = new HttpResponseMessage
            {
                ReasonPhrase = httpResponseMessage.ReasonPhrase,
                StatusCode = httpResponseMessage.StatusCode,
                Version = httpResponseMessage.Version
            };

            var headers = httpResponseMessage.Headers
                .Where(h => h.Value != null && h.Value.Any())
                .ToDictionary(h => h.Key, h => h.Value);

            var contentHeaders = httpResponseMessage.Content.Headers
                .Where(h => h.Value != null && h.Value.Any())
                .ToDictionary(h => h.Key, h => h.Value);

            var cacheData = new CacheData(contentBytes, httpResponseMessageCopy, headers, contentHeaders);
            return cacheData;
        }

        /// <summary>
        ///     Prepares the cached entry to be consumed by the caller, notably by setting the content.
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