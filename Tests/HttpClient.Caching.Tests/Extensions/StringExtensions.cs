using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpClient.Caching.Tests.Extensions
{
    internal static class StringExtensions
    {
        internal static async Task<string> GetStringAsync(this string url, HttpMessageHandler messageHandler = null)
        {
            // set a timeout to 10 seconds to avoid waiting on that forever
            using (var client = new System.Net.Http.HttpClient(messageHandler) { Timeout = TimeSpan.FromSeconds(10) })
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // return the data URI according to the standard
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}