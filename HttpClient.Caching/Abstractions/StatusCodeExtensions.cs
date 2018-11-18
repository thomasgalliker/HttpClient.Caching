using System;
using System.Collections.Generic;
using System.Net;

namespace Microsoft.Extensions.Caching.Abstractions
{
    /// <summary>
    /// Extension methods to HttpStatusCode that are related to caching functionality of this library.
    /// </summary>
    public static class StatusCodeExtensions
    {
        /// <summary>
        /// Gets an expiration time for the cache entry.
        /// </summary>
        /// <param name="statusCode">The status code to inspect the mapping.</param>
        /// <param name="mapping">The mapping providing configuration of what data to use.</param>
        /// <returns>A TimeSpan that should be used for the given status code when putting it into the cache.</returns>
        public static TimeSpan GetAbsoluteExpirationRelativeToNow(this HttpStatusCode statusCode, IDictionary<HttpStatusCode, TimeSpan> mapping)
        {
            int code = (int)statusCode;
            TimeSpan expiration;

            // get the expiration settings for the given status code
            if (mapping.TryGetValue(statusCode, out expiration))
            {
                return expiration;
            }

            // get the expiration settings for the status code category (200, 300, 400 and 500)
            if (mapping.TryGetValue((HttpStatusCode)(Math.Floor(code / 100.0) * 100), out expiration))
            {
                return expiration;
            }

            // return the default expiration
            return TimeSpan.FromDays(1);
        }
    }
}