using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Caching.InMemory;
using Newtonsoft.Json;
using Tracing;
using WpfSample.Extensions;

namespace WpfSample.Services
{
    public class ApiService : IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly IMemoryCache memoryCache;
        private readonly ITracer tracer;

        public ApiService(ITracer tracer, IApiServiceConfiguration apiServiceConfiguration, IMemoryCache memoryCache)
        {
            if (tracer == null)
            {
                throw new ArgumentNullException(nameof(tracer));
            }

            if (apiServiceConfiguration == null)
            {
                throw new ArgumentNullException(nameof(apiServiceConfiguration));
            }

            if (memoryCache == null)
            {
                throw new ArgumentNullException(nameof(memoryCache));
            }

            WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultNetworkCredentials;

            if (apiServiceConfiguration.CachingEnabled)
            {
                var inMemoryCacheHandler = new InMemoryCacheHandler();
                this.httpClient = new HttpClient(inMemoryCacheHandler);
            }
            else
            {
                this.httpClient = new HttpClient();
            }

            this.httpClient.BaseAddress = new Uri(apiServiceConfiguration.BaseUrl);
            this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (apiServiceConfiguration.Timeout != TimeSpan.Zero)
            {
                this.httpClient.Timeout = apiServiceConfiguration.Timeout;
            }

            this.tracer = tracer;
            this.memoryCache = memoryCache;
        }

        public async Task<string> GetAsync(string uri)
        {
            var httpResponseMessage = await this.httpClient.GetAsync(uri);

            return await this.HandleResponse(httpResponseMessage);
        }

        public async Task<TResult> GetAsync<TResult>(string uri, TimeSpan? cacheExpiration = null)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var caching = cacheExpiration.HasValue;
            if (caching && this.memoryCache.TryGetValue(uri, out TResult result))
            {
                stopwatch.Stop();
                this.tracer.Debug($"{nameof(this.GetAsync)} for Uri '{uri}' finished in {stopwatch.Elapsed.ToSecondsString()} (caching=true)");
                return result;
            }

            var httpResponseMessage = await this.httpClient.GetAsync(uri);
            var jsonResponse = await this.HandleResponse(httpResponseMessage);
            result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(jsonResponse));

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

        private async Task<string> HandleResponse(HttpResponseMessage response)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            return jsonResponse;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (this.httpClient != null)
            {
                this.httpClient.Dispose();
            }
        }
    }
}