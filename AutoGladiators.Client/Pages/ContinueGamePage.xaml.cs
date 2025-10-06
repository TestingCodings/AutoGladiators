using AutoGladiators.Client.Services.Storage;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AutoGladiators.Client.Pages;

namespace AutoGladiators.Client.Pages
{
    public partial class ContinueGamePage : ContentPage, INotifyPropertyChanged
    {
        private readonly PlayerProfileService _profileService;
        private ObservableCollection<PlayerProfileDisplayModel> _savedProfiles;

        public ObservableCollection<PlayerProfileDisplayModel> SavedProfiles
        {
            get => _savedProfiles;
            set
            {
                _savedProfiles = value;
                OnPropertyChanged();
            }
        }

        public ContinueGamePage(PlayerProfileService profileService)
        {
            InitializeComponent();
            _profileService = profileService;
            SavedProfiles = new ObservableCollection<PlayerProfileDisplayModel>();
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadSavedProfiles();
        }

        private async Task LoadSavedProfiles()
        {
            try
            {
                LoadingIndicator.IsRunning = true;
                NoProfilesFrame.IsVisible = false;
                ProfilesCollectionView.IsVisible = false;

                var profiles = await _profileService.GetSavedProfiles();
                
                SavedProfiles.Clear();
                
                if (profiles.Any())
                {
                    foreach (var profile in profiles.OrderByDescending(p => p.LastPlayed))
                    {
                        SavedProfiles.Add(new PlayerProfileDisplayModel(profile));
                    }
                    
                    ProfilesCollectionView.IsVisible = true;
                }
                else
                {
                    NoProfilesFrame.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load saved profiles: {ex.Message}", "OK");
                NoProfilesFrame.IsVisible = true;
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
            }
        }

        private async void OnLoadProfileClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is PlayerProfileDisplayModel profileDisplay)
            {
                try
                {
                    button.IsEnabled = false;
                    
                    // Load the full profile
                    var profile = await _profileService.LoadProfile(profileDisplay.Profile.Id);
                    if (profile != null)
                    {
                        // Set as current profile and navigate to main game
                        _profileService.SetCurrentProfile(profile);
                        
                        // Navigate to adventure page or main game screen
                        await Navigation.PushAsync(new AdventurePage());
                    }
                    else
                    {
                        await DisplayAlert("Error", "Failed to load the selected profile.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to load profile: {ex.Message}", "OK");
                }
                finally
                {
                    button.IsEnabled = true;
                }
            }
        }

        private async void OnDeleteProfileClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is PlayerProfileDisplayModel profileDisplay)
            {
                bool confirm = await DisplayAlert("Confirm Delete", 
                    $"Are you sure you want to delete '{profileDisplay.PlayerName}'? This action cannot be undone.", 
                    "Delete", "Cancel");

                if (confirm)
                {
                    try
                    {
                        await _profileService.DeleteProfile(profileDisplay.Profile.Id);
                        SavedProfiles.Remove(profileDisplay);
                        
                        // Show no profiles message if list is empty
                        if (!SavedProfiles.Any())
                        {
                            ProfilesCollectionView.IsVisible = false;
                            NoProfilesFrame.IsVisible = true;
                        }
                        
                        await DisplayAlert("Success", "Profile deleted successfully.", "OK");
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", $"Failed to delete profile: {ex.Message}", "OK");
                    }
                }
            }
        }

        private async void OnProfileTapped(object sender, EventArgs e)
        {
            if (sender is Frame frame && frame.BindingContext is PlayerProfileDisplayModel profileDisplay)
            {
                // Show profile details or load directly
                OnLoadProfileClicked(sender, e);
            }
        }

        private async void OnNewGameClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NewGamePage(_profileService));
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
        protected new void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Display model for saved profiles list
    public class PlayerProfileDisplayModel
    {
        public PlayerProfile? Profile { get; }
        public PlayerProfileSummary? Summary { get; }
        
        public PlayerProfileDisplayModel(PlayerProfile profile)
        {
            Profile = profile;
            Summary = null;
        }
        
        public PlayerProfileDisplayModel(PlayerProfileSummary summary)
        {
            Profile = null;
            Summary = summary;
        }

        public string PlayerName => Profile?.PlayerName ?? Summary?.PlayerName ?? string.Empty;
        public string PlayerNameInitial => string.IsNullOrEmpty(PlayerName) ? "?" : PlayerName.Substring(0, 1).ToUpper();
        
        public string DifficultyDisplay => Profile != null 
            ? $"{Profile.GameSettings.Difficulty} Mode"
            : "Unknown Mode";
        
        public string DifficultyColor => Profile?.GameSettings.Difficulty switch
        {
            "Easy" => "#00FF88",
            "Normal" => "#4A9EFF", 
            "Hard" => "#FF4444",
            _ => "#B0B0B0"
        };
        
        public string LevelDisplay => Profile != null 
            ? $"Level {Profile.Statistics.Level}"
            : "Level ?";
        public string BotsCollectedDisplay => Profile != null 
            ? $"{Profile.BotRoster.Count} Bots"
            : Summary != null 
                ? $"{Summary.BotCount} Bots"
                : "0 Bots";
        
        public string LastPlayedDisplay 
        {
            get
            {
                var lastPlayed = Profile?.LastPlayed ?? Summary?.LastPlayedDate ?? DateTime.MinValue;
                if (lastPlayed == DateTime.MinValue)
                    return "Unknown";
                
                var timeSince = DateTime.Now - lastPlayed;
                if (timeSince.TotalDays >= 1)
                    return $"{(int)timeSince.TotalDays} days ago";
                else if (timeSince.TotalHours >= 1)
                    return $"{(int)timeSince.TotalHours} hours ago";
                else if (timeSince.TotalMinutes >= 1)
                    return $"{(int)timeSince.TotalMinutes} minutes ago";
                else
                    return "Just now";
            }
        }
    }
}