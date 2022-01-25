using Microsoft.Extensions.Caching.Abstractions;
using System;
using System.Net.Http;

namespace Microsoft.Extensions.Caching.InMemory
{
    /// <summary>
    ///     Provides keys to store or retrieve data in the cache in the default way (http method + http request Uri)
    /// </summary>
    public class DefaultCacheKeysProvider : ICacheKeysProvider
    {
        /// <summary>
        ///     Return the key for the request message <paramref name="request"/> by composing a string
        ///     with <see cref="HttpRequestMessage.Method"/> and <see cref="HttpRequestMessage.RequestUri"/>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public string GetKey(HttpRequestMessage request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var key = request.Method + request.RequestUri.ToString();

            return key;
        }
    }
}
