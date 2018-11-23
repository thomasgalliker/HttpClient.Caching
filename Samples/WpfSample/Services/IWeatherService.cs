using System;
using System.Threading.Tasks;
using WpfSample.Model;

namespace WpfSample.Services
{
    public interface IWeatherService : IDisposable
    {
        Task<WeatherRoot> GetWeather(double latitude, double longitude, Units units = Units.Metric);

        Task<WeatherRoot> GetWeather(string city, Units units = Units.Metric);

        Task<WeatherForecastRoot> GetForecast(int cityId, Units units = Units.Metric);
    }
}