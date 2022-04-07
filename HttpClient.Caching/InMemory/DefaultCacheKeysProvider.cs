using System;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Caching.Abstractions;

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
        /// <returns>
        ///     An example of return value: "MET_GET;URI_https://www.google.it"
        /// </returns>
        public string GetKey(HttpRequestMessage request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var sb = new StringBuilder();

            sb.AppendFormat("MET_{0};", request.Method);
            sb.AppendFormat("URI_{0};", request.RequestUri);

            return sb.ToString();
        }
    }
}
