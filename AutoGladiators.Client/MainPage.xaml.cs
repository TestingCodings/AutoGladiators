using Microsoft.Maui.Controls;
using System;

namespace AutoGladiators.Client.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnStartSimulationClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SimulationPage());
        }

        private async void OnViewBotsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new BotsPage());
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }
    }
}