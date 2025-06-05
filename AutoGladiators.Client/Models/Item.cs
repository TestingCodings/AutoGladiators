using Microsoft.Maui.Controls;
using AutoGladiators.Client.Pages;

namespace AutoGladiators.Client
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent(); // must match the x:Class in App.xaml
            MainPage = new NavigationPage(new SimulationPage());
        }
    }
}
