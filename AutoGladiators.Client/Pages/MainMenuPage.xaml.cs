using System;
using Microsoft.Maui.Controls;
using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Services.Logging;
using AutoGladiators.Client.Services.Storage;
using AutoGladiators.Core.Services.Exploration;

namespace AutoGladiators.Client.Pages
{
    public partial class MainMenuPage : ContentPage
    {
        private static readonly IAppLogger Log = AppLog.For<MainMenuPage>();
        private readonly PlayerProfileService _profileService;
        
        public MainMenuPage(PlayerProfileService profileService)
        {
            InitializeComponent();
            _profileService = profileService;
            Log.Info("MainMenuPage initialized");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateCurrentProfileDisplay();
        }

        private void UpdateCurrentProfileDisplay()
        {
            var currentProfile = _profileService.GetCurrentProfile();
            if (currentProfile != null)
            {
                CurrentProfileFrame.IsVisible = true;
                QuickPlayFrame.IsVisible = true;
                
                ProfileInitialLabel.Text = string.IsNullOrEmpty(currentProfile.PlayerName) 
                    ? "?" : currentProfile.PlayerName.Substring(0, 1).ToUpper();
                CurrentProfileLabel.Text = $"{currentProfile.PlayerName} - Level {currentProfile.Statistics.Level}";
                QuickPlayButton.Text = $"🗺️ CONTINUE AS {currentProfile.PlayerName.ToUpper()}";
            }
            else
            {
                CurrentProfileFrame.IsVisible = false;
                QuickPlayFrame.IsVisible = false;
            }
        }

        private async void OnNewGameClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("New Game button clicked");
                await Navigation.PushAsync(new NewGamePage(_profileService));
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to open new game: {ex.Message}", ex);
                await DisplayAlert("Error", $"Failed to start new game: {ex.Message}", "OK");
            }
        }

        private async void OnContinueGameClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("Continue Game button clicked");
                await Navigation.PushAsync(new ContinueGamePage(_profileService));
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to open continue game: {ex.Message}", ex);
                await DisplayAlert("Error", $"Failed to load continue game: {ex.Message}", "OK");
            }
        }

        private async void OnQuickAdventureClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("Quick Adventure button clicked");
                var currentProfile = _profileService.GetCurrentProfile();
                if (currentProfile != null)
                {
                    // Navigate to world exploration page
                    var explorationPage = Handler?.MauiContext?.Services?.GetService<ExplorationPage>();
                    if (explorationPage != null)
                    {
                        await Navigation.PushAsync(explorationPage);
                    }
                    else
                    {
                        await DisplayAlert("Error", "Exploration system not available.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("No Profile", "Please select a profile first using Continue Game.", "OK");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to open world exploration: {ex.Message}", ex);
                await DisplayAlert("Error", $"Failed to start world exploration: {ex.Message}", "OK");
            }
        }

        private async void OnProfileSettingsClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("Profile Settings button clicked");
                var currentProfile = _profileService.GetCurrentProfile();
                if (currentProfile != null)
                {
                    // Show profile management options
                    var action = await DisplayActionSheet(
                        $"Profile: {currentProfile.PlayerName}", 
                        "Cancel", 
                        null, 
                        "🗺️ Explore World",
                        "View Bot Roster", 
                        "View Inventory", 
                        "Game Statistics",
                        "Switch Profile",
                        "Log Out");

                    switch (action)
                    {
                        case "🗺️ Explore World":
                            var explorationPage = Handler?.MauiContext?.Services?.GetService<ExplorationPage>();
                            if (explorationPage != null)
                            {
                                await Navigation.PushAsync(explorationPage);
                            }
                            else
                            {
                                await DisplayAlert("Error", "Exploration system not available.", "OK");
                            }
                            break;
                        case "View Bot Roster":
                            await Navigation.PushAsync(new BotRosterPage());
                            break;
                        case "View Inventory":
                            await Navigation.PushAsync(new InventoryPage());
                            break;
                        case "Game Statistics":
                            await DisplayAlert("Statistics", 
                                $"Level: {currentProfile.Statistics.Level}\n" +
                                $"Experience: {currentProfile.Statistics.Experience} XP\n" +
                                $"Bots Collected: {currentProfile.BotRoster.Count}\n" +
                                $"Battles Won: {currentProfile.Statistics.BattlesWon}\n" +
                                $"Total Playtime: {currentProfile.Statistics.TotalPlaytimeMinutes} minutes", 
                                "OK");
                            break;
                        case "Switch Profile":
                            await Navigation.PushAsync(new ContinueGamePage(_profileService));
                            break;
                        case "Log Out":
                            _profileService.SetCurrentProfile(null);
                            UpdateCurrentProfileDisplay();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Profile settings error: {ex.Message}", ex);
                await DisplayAlert("Error", $"Profile settings error: {ex.Message}", "OK");
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
