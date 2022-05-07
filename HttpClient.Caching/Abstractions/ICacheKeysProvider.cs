using System.Net.Http;

namespace Microsoft.Extensions.Caching.Abstractions
{
    /// <summary>
    ///     Provides keys to store or retrieve data in the cache
    /// </summary>
    public interface ICacheKeysProvider
    {
        /// <summary>
        ///     Return the key for the request message <paramref name="request"/>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        string GetKey(HttpRequestMessage request);
    }
}
