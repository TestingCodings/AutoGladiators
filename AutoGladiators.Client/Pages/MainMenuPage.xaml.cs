using System;
using Microsoft.Maui.Controls;
using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services.Logging;

namespace AutoGladiators.Client.Pages
{
    public partial class MainMenuPage : ContentPage
    {
        private static readonly IAppLogger Log = AppLog.For<MainMenuPage>();
        
        public MainMenuPage()
        {
            InitializeComponent();
            Log.Info("MainMenuPage initialized");
        }

        private async void OnAdventureClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("Adventure button clicked");
                await Navigation.PushAsync(new AdventurePage());
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to open adventure: {ex.Message}", ex);
                await DisplayAlert("Error", $"Failed to open adventure: {ex.Message}", "OK");
            }
        }

        private async void OnBotRosterClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("Bot Roster button clicked");
                await Navigation.PushAsync(new BotRosterPage());
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to open bot roster: {ex.Message}", ex);
                await DisplayAlert("Error", $"Failed to open bot roster: {ex.Message}", "OK");
            }
        }

        private async void OnInventoryClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("Inventory button clicked");
                await Navigation.PushAsync(new InventoryPage());
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to open inventory: {ex.Message}", ex);
                await DisplayAlert("Error", $"Failed to open inventory: {ex.Message}", "OK");
            }
        }

        private async void OnBattleArenaClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("Battle Arena button clicked");
                // Navigate to battle mode selector or create a quick battle
                await Navigation.PushAsync(new BattleModeSelectorPage());
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to open battle arena: {ex.Message}", ex);
                await DisplayAlert("Error", $"Failed to open battle arena: {ex.Message}", "OK");
            }
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("Settings button clicked");
                await DisplayAlert("Settings", "Settings menu coming soon!", "OK");
            }
            catch (Exception ex)
            {
                Log.Error($"Settings error: {ex.Message}", ex);
                await DisplayAlert("Error", $"Settings error: {ex.Message}", "OK");
            }
        }

        private async void OnDebugMenuClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("Debug Menu button clicked");
                await Navigation.PushAsync(new DebugMenuPage());
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to open debug menu: {ex.Message}", ex);
                await DisplayAlert("Error", $"Failed to open debug menu: {ex.Message}", "OK");
            }
        }
    }
}
