using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Tracing;
using WpfSample.Model;
using WpfSample.Services;

namespace WpfSample.ViewModels
{
    public class WeatherViewModel : ViewModelBase
    {
        private readonly IWeatherService weatherService;
        private readonly ILocationService locationService;

        private string condition;
        private WeatherForecastRoot forecast;
        private ICommand getCurrentLocationCommand;
        private ICommand getWeatherCommand;
        private string location;
        private string temperature;
        private bool isBusy;

        public WeatherViewModel(ILocationService locationService, IWeatherService weatherService)
        {
            this.locationService = locationService;
            this.weatherService = weatherService;

            this.LoadCurrentLocation();
        }

        public ICommand GetCurrentLocationCommand =>
            this.getCurrentLocationCommand ??
            (this.getCurrentLocationCommand = new RelayCommand(async () => await this.LoadCurrentLocation(), () => !this.IsBusy));

        private async Task LoadCurrentLocation()
        {
            try
            {
                // Get city name using location service
                this.Location = await this.locationService.GetCity();
            }
            catch (Exception ex)
            {
                this.Location = "Cham, Switzerland";
                this.Condition = $"{ex}";
                Debug.WriteLine(ex.Message);
            }
        }

        public ICommand GetWeatherCommand =>
            this.getWeatherCommand ??
            (this.getWeatherCommand = new RelayCommand(async () => await this.ExecuteGetWeatherCommand(), () => !this.IsBusy));

        private async Task ExecuteGetWeatherCommand()
        {
            if (this.IsBusy)
            {
                return;
            }

            this.IsBusy = true;
            try
            {
                const Units units = Units.Metric;

                // Get weather by city
                var weatherRoot = await this.weatherService.GetWeather(this.Location, units);

                // Get forecast based on CityId
                this.Forecast = await this.weatherService.GetForecast(weatherRoot.CityId, units);

                this.Temperature = $"{weatherRoot?.MainWeather?.Temperature ?? 0}°C";
                this.Condition = $"{weatherRoot.Name} (CityId {weatherRoot.CityId}): {weatherRoot?.Weather?[0]?.Description ?? string.Empty}";
            }
            catch (Exception ex)
            {
                this.Temperature = "N/A";
                this.Condition = $"{this.Location}: {ex}";
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.Set(ref this.isBusy, value, nameof(this.IsBusy));
        }

        public string Location
        {
            get => this.location;
            set => this.Set(ref this.location, value, nameof(this.Location));
        }

        public string Temperature
        {
            get => this.temperature;
            set => this.Set(ref this.temperature, value, nameof(this.Temperature));
        }

        public string Condition
        {
            get => this.condition;
            set => this.Set(ref this.condition, value, nameof(this.Condition));
        }

        public WeatherForecastRoot Forecast
        {
            get => this.forecast;
            set => this.Set(ref this.forecast, value, nameof(this.Forecast));
        }
    }
}