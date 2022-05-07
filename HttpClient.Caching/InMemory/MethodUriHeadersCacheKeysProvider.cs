using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Caching.Abstractions;

namespace Microsoft.Extensions.Caching.InMemory
{
    /// <summary>
    ///     Provides keys to store or retrieve data in the cache by using http method, specific headers and Uri
    /// </summary>
    public class MethodUriHeadersCacheKeysProvider : ICacheKeysProvider
    {
        private readonly string[] headersName;

        /// <summary>
        ///     Initialize the cache key provider passing the headers name that will be used to compose <paramref name="headersName"/>
        /// </summary>
        /// <param name="headersName"></param>
        public MethodUriHeadersCacheKeysProvider(string[] headersName)
        {
            if (headersName != null)
            {
                this.headersName = headersName.OrderBy(i => i).ToArray();
            }
        }

        /// <summary>
        ///     Return the key for the request message <paramref name="request"/> by composing a string
        ///     with <see cref="HttpRequestMessage.Method"/>, <see cref="HttpRequestMessage.Headers"/> and <see cref="HttpRequestMessage.RequestUri"/>
        /// </summary>
        /// <param name="request"></param>
        /// <returns>
        ///     An example of return value: "MET_GET;HEA_X-KID_389dfhuif;URI_https://www.google.it?par1=65&par2=20;"
        /// </returns>
        public string GetKey(HttpRequestMessage request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var sb = new StringBuilder();

            sb.AppendFormat("MET_{0};", request.Method);
            if (this.headersName != null)
            {
                foreach (var headerName in this.headersName)
                {
                    if (request.Headers.Contains(headerName))
                    {
                        sb.AppendFormat("HEA_{0}_{1};", headerName, this.GetHeaderValue(request, headerName));
                    }
                }
            }

            sb.AppendFormat("URI_{0};", request.RequestUri);

            return sb.ToString();
        }

        private string GetHeaderValue(HttpRequestMessage request, string headerName)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrEmpty(headerName))
            {
                throw new ArgumentException($"'{nameof(headerName)}' cannot be null or empty.", nameof(headerName));
            }

            var orderedHeaderValues = string.Empty;

            var headerValues = request.Headers.GetValues(headerName);
            if (headerValues != null)
            {
                orderedHeaderValues = string.Join(",", headerValues.OrderBy(i => i));
            }

            return orderedHeaderValues;
        }
    }
}
