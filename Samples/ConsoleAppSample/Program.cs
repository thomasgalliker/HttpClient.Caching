using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Caching.InMemory;

namespace ConsoleAppSample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string url = "http://worldtimeapi.org/api/timezone/Europe/Zurich";

            // HttpClient uses an HttpClientHandler nested into InMemoryCacheHandler in order to handle http get response caching
            var httpClientHandler = new HttpClientHandler();
            var cacheExpirationPerHttpResponseCode = CacheExpirationProvider.CreateSimple(TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));
            var handler = new InMemoryCacheHandler(httpClientHandler, cacheExpirationPerHttpResponseCode);
            using (var client = new HttpClient(handler))
            {
                // HttpClient calls the same API endpoint five times:
                // - The first attempt is called against the real API endpoint since no cache is available
                // - Attempts 2 to 5 can be read from cache
                for (var i = 1; i <= 5; i++)
                {
                    Console.Write($"Attempt {i}: HTTP GET {url}...");
                    var stopwatch = Stopwatch.StartNew();
                    var result = client.GetAsync(url).GetAwaiter().GetResult();

                    // Do something useful with the returned content...
                    var content = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Console.WriteLine($" completed in {stopwatch.ElapsedMilliseconds}ms");

                    // Artificial wait time...
                    Thread.Sleep(1000);
                }
            }

            Console.WriteLine();

            var stats = handler.StatsProvider.GetStatistics();
            Console.WriteLine($"TotalRequests: {stats.Total.TotalRequests}");
            Console.WriteLine($"-> CacheHit: {stats.Total.CacheHit}");
            Console.WriteLine($"-> CacheMiss: {stats.Total.CacheMiss}");
            Console.ReadKey();
        }
    }
}