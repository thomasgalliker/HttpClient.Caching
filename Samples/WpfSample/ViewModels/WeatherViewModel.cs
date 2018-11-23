using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using WpfSample.Model;
using WpfSample.Services;

namespace WpfSample.ViewModels
{
    public class WeatherViewModel : ViewModelBase
    {
        WeatherService WeatherService { get; } = new WeatherService();

        public string Location
        {
            get { return this.location; }
            set
            {
                this.Set(ref this.location, value, nameof(this.Location));
            }
        }

        string temp = string.Empty;

        public string Temp
        {
            get { return this.temp; }
            set
            {
                this.Set(ref this.temp, value, nameof(this.Temp));
            }
        }

        string condition = string.Empty;

        public string Condition
        {
            get { return this.condition; }
            set
            {
                this.Set(ref this.condition, value, nameof(this.Condition));
            }
        }

        WeatherForecastRoot forecast;

        public WeatherForecastRoot Forecast
        {
            get { return this.forecast; }
            set
            {
                this.forecast = value;
                this.RaisePropertyChanged(nameof(this.Forecast));
            }
        }

        ICommand getWeather;
        private string location;

        public ICommand GetWeatherCommand =>
            this.getWeather ??
            (this.getWeather = new RelayCommand(async () => await this.ExecuteGetWeatherCommand()));

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

                //Get weather by city
                var weatherRoot = await this.WeatherService.GetWeather(this.Location.Trim(), units);

                //Get forecast based on cityId
                this.Forecast = await this.WeatherService.GetForecast(weatherRoot.CityId, units);

                var unit = "C";
                this.Temp = $"Temp: {weatherRoot?.MainWeather?.Temperature ?? 0}°{unit}";
                this.Condition = $"{weatherRoot.Name}: {weatherRoot?.Weather?[0]?.Description ?? string.Empty}";
            }
            catch (Exception ex)
            {
                this.Temp = "Unable to get Weather";
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        public bool IsBusy { get; set; }
    }
}