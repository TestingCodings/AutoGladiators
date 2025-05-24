using System;
using Microsoft.Maui.Controls;

namespace AutoGladiators_MAUI
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnRunSimulationClicked(object sender, EventArgs e)
        {
            DisplayAlert("Simulation", "Simulation started!", "OK");
        }
    }
}
