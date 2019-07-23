using System.Windows;
using Tracing;
using WpfSample.Services;
using WpfSample.ViewModels;

namespace WpfSample.Views
{
    public partial class WeatherView : Window
    {
        public WeatherView()
        {
            this.InitializeComponent();

            var locationServiceTracer = new ActionTracer(nameof(LocationService), this.WriteLogToStatusBar);
            var locationService = new LocationService(locationServiceTracer);

            var weatherServiceTracer = new ActionTracer(nameof(LocationService), this.WriteLogToStatusBar);
            var weatherService = new WeatherService(weatherServiceTracer);

            this.DataContext = new WeatherViewModel(locationService, weatherService);
        }

        private void WriteLogToStatusBar(string source, TraceEntry entry)
        {
            this.StatusBarTextBlock.Text = $"{source}: {entry.Message}";
        }
    }
}