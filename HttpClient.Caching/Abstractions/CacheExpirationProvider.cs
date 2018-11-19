using System;
using System.Collections.Generic;
using System.Net;

namespace Microsoft.Extensions.Caching.Abstractions
{
    /// <summary>
    ///     Provides the parameter needed to specify expiration timeouts based on HttpStatusCode for the
    ///     <see cref="InMemoryCacheHandler" />.
    /// </summary>
    public static class CacheExpirationProvider
    {
        /// <summary>
        ///     Creates a configuration for the <see cref="InMemoryCacheHandler" /> for the success, client error and server error
        ///     case.
        /// </summary>
        /// <param name="success">Cache expiration time for the successful retrieval of data, based on HTTP status code 200-299.</param>
        /// <param name="clientError">
        ///     Cache expiration time for the errornous retrieval of data given a client error, based on HTTP
        ///     status code 400-499.
        /// </param>
        /// <param name="serverError">
        ///     Cache expiration time for the errornous retrieval of data given a server error, based on HTTP
        ///     status code 500-599.
        /// </param>
        /// <returns></returns>
        public static IDictionary<HttpStatusCode, TimeSpan> CreateSimple(TimeSpan success, TimeSpan clientError, TimeSpan serverError)
        {
            return new Dictionary<HttpStatusCode, TimeSpan>
            {
                { HttpStatusCode.OK, success },
                { HttpStatusCode.BadRequest, clientError },
                { HttpStatusCode.InternalServerError, serverError }
            };
        }
    }
}