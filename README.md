# HttpClient.Caching
[![Version](https://img.shields.io/nuget/v/HttpClient.Caching.svg)](https://www.nuget.org/packages/HttpClient.Caching)  [![Downloads](https://img.shields.io/nuget/dt/HttpClient.Caching.svg)](https://www.nuget.org/packages/HttpClient.Caching)

<img src="https://raw.githubusercontent.com/thomasgalliker/HttpClient.Caching/master/logo.png" alt="HttpClient.Caching" align="right">
HttpClient.Caching adds http response caching to HttpClient.

### Download and Install HttpClient.Caching
This library is available on NuGet: https://www.nuget.org/packages/HttpClient.Caching/
Use the following command to install HttpClient.Caching using NuGet package manager console:

    PM> Install-Package HttpClient.Caching

You can use this library in any .Net project which is compatible to .Net Framework 4.5+ and .Net Standard 1.2+ (e.g. Xamarin Android, iOS, Universal Windows Platform, etc.)

### API Usage
#### Using IMemoryCache for caching HTTP GET Results
Declare IMemoryCache in your API service, either by creating an instance manually or by injecting IMemoryCache into your API service class.
```C#
private readonly IMemoryCache memoryCache = new MemoryCache();
```

Following example show how IMemoryCache can be used to store an HTTP GET result in memory for a given time span (cacheExpirection):
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
    result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(jsonResponse, this.serializerSettings));

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

### License
This project is Copyright &copy; 2018 [Thomas Galliker](https://ch.linkedin.com/in/thomasgalliker). Free for non-commercial use. For commercial use please contact the author.
