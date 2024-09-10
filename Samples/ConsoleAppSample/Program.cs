using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Caching.InMemory;

namespace ConsoleAppSample
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            const string url = "http://worldtimeapi.org/api/timezone/Europe/Zurich";

            // HttpClient uses an HttpClientHandler nested into InMemoryCacheHandler in order to handle http get response caching
            var httpClientHandler = new HttpClientHandler();

            var cacheExpirationPerHttpResponseCode = CacheExpirationProvider.CreateSimple(
                success: TimeSpan.FromSeconds(60),
                clientError: TimeSpan.FromSeconds(10),
                serverError: TimeSpan.FromSeconds(5));

            var inMemoryCacheHandler = new InMemoryCacheHandler(httpClientHandler, cacheExpirationPerHttpResponseCode);

            using (var httpClient = new HttpClient(inMemoryCacheHandler))
            {
                // HttpClient calls the same API endpoint five times:
                // - The first attempt is called against the real API endpoint since no cache is available
                // - Attempts 2 to 5 can be read from cache
                for (var i = 1; i <= 5; i++)
                {
                    Console.Write($"Attempt {i}: HTTP GET {url}...");
                    var stopwatch = Stopwatch.StartNew();

                    // Send the http GET request using GetAsync
                    var httpResponseMessage = await httpClient.GetAsync(url);

                    // Alternatively, use the Send or SendAsync methods to send the http request
                    //var httpResponseMessage = httpClient.Send(new HttpRequestMessage(HttpMethod.Get, url));

                    // Do something useful with the returned content...
                    var httpResponseContent = await httpResponseMessage.Content.ReadAsStringAsync();
                    Console.WriteLine($" completed in {stopwatch.ElapsedMilliseconds}ms");

                    // Artificial wait time...
                    Thread.Sleep(1000);
                }
            }

            Console.WriteLine();

            var stats = inMemoryCacheHandler.StatsProvider.GetStatistics();
            Console.WriteLine($"Statistics:");
            Console.WriteLine($"-> TotalRequests: {stats.Total.TotalRequests}");
            Console.WriteLine($"-> CacheHit: {stats.Total.CacheHit}");
            Console.WriteLine($"-> CacheMiss: {stats.Total.CacheMiss}");
            Console.ReadKey();
        }
    }
}