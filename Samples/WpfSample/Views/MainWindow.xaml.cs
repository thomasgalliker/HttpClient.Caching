using System.Windows;
using WpfSample.ViewModels;

namespace WpfSample.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = new WeatherViewModel();
        }
    }
}