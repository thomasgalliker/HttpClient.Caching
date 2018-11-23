using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Caching.InMemory;
using Tracing;
using WpfSample.Extensions;

namespace WpfSample.Services
{
    public class LocationService : ILocationService
    {
        private const string IpInfoUri = "https://ipinfo.io/";
        private readonly ITracer tracer;
        private readonly HttpClient httpClient;

        public LocationService(ITracer tracer)
        {
            this.tracer = tracer;
            WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultNetworkCredentials;

            var httpClientHandler = new HttpClientHandler();
            var cacheExpirationPerHttpResponseCode = CacheExpirationProvider.CreateSimple(TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));
            var inMemoryCacheHandler = new InMemoryCacheHandler(httpClientHandler, cacheExpirationPerHttpResponseCode);

            this.httpClient = new HttpClient(inMemoryCacheHandler)
            {
                BaseAddress = new Uri(IpInfoUri)
            };
        }

        public async Task<string> GetCity()
        {
            var stopwatch = Stopwatch.StartNew();

            var httpResponseMessage = await this.httpClient.GetAsync("city");
            var city = (await httpResponseMessage.Content.ReadAsStringAsync()).Trim().Replace("\n", "").Replace("\r", "");

            this.tracer.Debug($"GetCity returned '{city}' in {stopwatch.Elapsed.ToSecondsString()}");
            
            return city;
        }

        public void Dispose()
        {
            this.httpClient?.Dispose();
        }
    }
}