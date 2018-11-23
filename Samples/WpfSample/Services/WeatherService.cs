using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Caching.InMemory;
using Newtonsoft.Json;
using Tracing;
using WpfSample.Extensions;
using WpfSample.Model;

namespace WpfSample.Services
{
    public class WeatherService : IWeatherService
    {
        private const string WeatherCoordinatesUri = "http://api.openweathermap.org/data/2.5/weather?lat={0}&lon={1}&units={2}&appid=fc9f6c524fc093759cd28d41fda89a1b";
        private const string WeatherCityUri = "http://api.openweathermap.org/data/2.5/weather?q={0}&units={1}&appid=fc9f6c524fc093759cd28d41fda89a1b";
        private const string ForecastUri = "http://api.openweathermap.org/data/2.5/forecast?id={0}&units={1}&appid=fc9f6c524fc093759cd28d41fda89a1b";

        private readonly ITracer tracer;
        private readonly HttpClient httpClient;

        public WeatherService(ITracer tracer)
        {
            this.tracer = tracer;

            WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultNetworkCredentials;

            var httpClientHandler = new HttpClientHandler();
            var cacheExpirationPerHttpResponseCode = CacheExpirationProvider.CreateSimple(TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));
            var inMemoryCacheHandler = new InMemoryCacheHandler(httpClientHandler, cacheExpirationPerHttpResponseCode);
            this.httpClient = new HttpClient(inMemoryCacheHandler);
        }

        public async Task<WeatherRoot> GetWeather(double latitude, double longitude, Units units = Units.Metric)
        {
            var stopwatch = Stopwatch.StartNew();

            var url = string.Format(WeatherCoordinatesUri, latitude, longitude, units.ToString().ToLower());
            var json = await this.httpClient.GetStringAsync(url);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            this.tracer.Debug($"GetWeather returned for latitude={latitude:F}, longitude={longitude:F}  in {stopwatch.Elapsed.ToSecondsString()}");

            return JsonConvert.DeserializeObject<WeatherRoot>(json);
        }

        public async Task<WeatherRoot> GetWeather(string city, Units units = Units.Metric)
        {
            var stopwatch = Stopwatch.StartNew();

            var url = string.Format(WeatherCityUri, city, units.ToString().ToLower());
            var json = await this.httpClient.GetStringAsync(url);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            this.tracer.Debug($"GetWeather returned for city '{city}' in {stopwatch.Elapsed.ToSecondsString()}");

            return JsonConvert.DeserializeObject<WeatherRoot>(json);
        }

        public async Task<WeatherForecastRoot> GetForecast(int cityId, Units units = Units.Metric)
        {
            var stopwatch = Stopwatch.StartNew();

            var url = string.Format(ForecastUri, cityId, units.ToString().ToLower());
            var json = await this.httpClient.GetStringAsync(url);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            this.tracer.Debug($"GetForecast returned for cityId {cityId} in {stopwatch.Elapsed.ToSecondsString()}");

            return JsonConvert.DeserializeObject<WeatherForecastRoot>(json);
        }

        public void Dispose()
        {
            this.httpClient?.Dispose();
        }
    }
}