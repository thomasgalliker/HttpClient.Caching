# HttpClient.Caching
[![Version](https://img.shields.io/nuget/v/HttpClient.Caching.svg)](https://www.nuget.org/packages/HttpClient.Caching) [![Downloads](https://img.shields.io/nuget/dt/HttpClient.Caching.svg)](https://www.nuget.org/packages/HttpClient.Caching) [![Buy Me a Coffee](https://img.shields.io/badge/support-buy%20me%20a%20coffee-FFDD00)](https://buymeacoffee.com/thomasgalliker)

### Download and Install HttpClient.Caching
This library is available on NuGet: https://www.nuget.org/packages/HttpClient.Caching/
Use the following command to install HttpClient.Caching using NuGet package manager console:

    PM> Install-Package HttpClient.Caching

You can use this library in projects targeting `.NET Framework 4.6.2`, `.NET Standard 2.0`, `.NET 8.0` and later.

### The Purpose of HTTP Caching
HTTP Caching affects both involved communication peers, the client and the server. On the server-side, caching is appropriate for improving throughput (scalability). HTTP caching doesn't make a single HTTP call faster but it can lead to better response performance in high-load scenarios. On the client-side, caching is used to avoid unnecessarily repetitiv HTTP calls. This leads to less waiting time on the client-side since cache reads have naturally a much better response performance than HTTP calls over relatively slow network links.

### API Usage
#### Using MemoryCache
Declare `IMemoryCache` in your API service, either by creating an instance manually or by injecting `IMemoryCache` into your API service class.
```C#
private readonly IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
```

Following example shows how `IMemoryCache` can be used to store an HTTP GET result in memory for a given time span (`cacheExpiration`):
```C#
public async Task<TResult> GetAsync<TResult>(string uri, TimeSpan? cacheExpiration = null)
{
    var stopwatch = new Stopwatch();
    stopwatch.Start();

    TResult result;
    var caching = cacheExpiration.HasValue;
    if (caching && this.memoryCache.TryGetValue(uri, out result))
    {
        stopwatch.Stop();
        this.tracer.Debug($"{nameof(this.GetAsync)} for Uri '{uri}' finished in {stopwatch.Elapsed.ToSecondsString()} (caching=true)");
        return result;
    }

    var httpResponseMessage = await this.HandleRequest(() => this.httpClient.GetAsync(uri));
    var jsonResponse = await this.HandleResponse(httpResponseMessage);
    result = await Task.Run(() => JsonSerializer.Deserialize<TResult>(jsonResponse, this.serializerSettings));

    if (caching)
    {
        this.memoryCache.Set(uri, result, cacheExpiration.Value);
    }
    else
    {
        this.memoryCache.Remove(uri);
    }

    stopwatch.Stop();
    this.tracer.Debug($"{nameof(this.GetAsync)} for Uri '{uri}' finished in {stopwatch.Elapsed.ToSecondsString()}");
    return result;
}
```

#### Using InMemoryCacheHandler
`HttpClient` allows injecting a custom HTTP handler. In the following example, an `HttpClientHandler` is nested into an `InMemoryCacheHandler`, and the `InMemoryCacheHandler` is responsible for maintaining and reading the cache.
```C#
static void Main(string[] args)
{
    const string url = "http://worldtimeapi.org/api/timezone/Europe/Zurich";

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

    StatsResult stats = handler.StatsProvider.GetStatistics();
    Console.WriteLine($"TotalRequests: {stats.Total.TotalRequests}");
    Console.WriteLine($"-> CacheHit: {stats.Total.CacheHit}");
    Console.WriteLine($"-> CacheMiss: {stats.Total.CacheMiss}");
    Console.ReadKey();
}
```

Console output:
```
Attempt 1: HTTP GET http://worldtimeapi.org/api/timezone/Europe/Zurich... completed in 625ms
Attempt 2: HTTP GET http://worldtimeapi.org/api/timezone/Europe/Zurich... completed in 48ms
Attempt 3: HTTP GET http://worldtimeapi.org/api/timezone/Europe/Zurich... completed in 1ms
Attempt 4: HTTP GET http://worldtimeapi.org/api/timezone/Europe/Zurich... completed in 1ms
Attempt 5: HTTP GET http://worldtimeapi.org/api/timezone/Europe/Zurich... completed in 1ms

TotalRequests: 5
-> CacheHit: 4
-> CacheMiss: 1
```

### Cache keys

By default, requests will be cached by using a key which is composed with http method and url (only HEAD and GET http methods are supported).
If this default behavior isn't enough, you can implement your own `ICacheKeysProvider`, which builds a cache key from an `HttpRequestMessage`.

The following example shows how to use a cache provider of type `MethodUriHeadersCacheKeysProvider`.
This cache key provider is already implemented and evaluates http method, specified headers and url to compose a cache key.
with InMemoryCacheHandler.
```C#
static void Main(string[] args)
{
    const string url = "http://worldtimeapi.org/api/timezone/Europe/Zurich";

    var httpClientHandler = new HttpClientHandler();
    var cacheExpirationPerHttpResponseCode = CacheExpirationProvider.CreateSimple(TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));
    // this cache key provider evaluates http method, specified headers and url to compose a key
    var cacheKeyProvider = new MethodUriHeadersCacheKeysProvider(new string[] { "FIRST-HEADER", "SECOND-HEADER" });
    var handler = new InMemoryCacheHandler(
        innerHandler: httpClientHandler,
        cacheExpirationPerHttpResponseCode: cacheExpirationPerHttpResponseCode,
        cacheKeysProvider: cacheKeyProvider
    );
    
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

    StatsResult stats = handler.StatsProvider.GetStatistics();
    Console.WriteLine($"TotalRequests: {stats.Total.TotalRequests}");
    Console.WriteLine($"-> CacheHit: {stats.Total.CacheHit}");
    Console.WriteLine($"-> CacheMiss: {stats.Total.CacheMiss}");
    Console.ReadKey();
}
```

### Links
[How-to: HTTP Caching for RESTful & Hypermedia APIs](https://www.apiacademy.co/articles/2015/12/how-to-http-caching-for-restful-hypermedia-apis)

### License
This project is Copyright &copy; 2022 [Thomas Galliker](https://ch.linkedin.com/in/thomasgalliker). Free for non-commercial use. For commercial use please contact the author.
