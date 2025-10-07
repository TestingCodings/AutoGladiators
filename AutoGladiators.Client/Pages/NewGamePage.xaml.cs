using AutoGladiators.Client.Services.Storage;
using AutoGladiators.Core.Services;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace AutoGladiators.Client.Pages
{
    public partial class NewGamePage : ContentPage
    {
        private readonly PlayerProfileService _profileService;

        public NewGamePage(PlayerProfileService profileService)
        {
            InitializeComponent();
            _profileService = profileService;
        }

        private async void OnPlayerNameChanged(object sender, TextChangedEventArgs e)
        {
            await ValidatePlayerName(e.NewTextValue);
        }

        private async Task<bool> ValidatePlayerName(string name)
        {
            NameValidationLabel.IsVisible = false;
            
            if (string.IsNullOrWhiteSpace(name))
            {
                NextButton.IsEnabled = false;
                return false;
            }

            if (name.Length < 2)
            {
                NameValidationLabel.Text = "Name must be at least 2 characters long";
                NameValidationLabel.IsVisible = true;
                NextButton.IsEnabled = false;
                return false;
            }

            if (name.Length > 25)
            {
                NameValidationLabel.Text = "Name cannot be longer than 25 characters";
                NameValidationLabel.IsVisible = true;
                NextButton.IsEnabled = false;
                return false;
            }

            // Check for invalid characters
            if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z0-9\s\-_]+$"))
            {
                NameValidationLabel.Text = "Name can only contain letters, numbers, spaces, hyphens, and underscores";
                NameValidationLabel.IsVisible = true;
                NextButton.IsEnabled = false;
                return false;
            }

            // Check if name already exists
            if (await _profileService.DoesProfileExist(name))
            {
                NameValidationLabel.Text = "A trainer with this name already exists";
                NameValidationLabel.IsVisible = true;
                NextButton.IsEnabled = false;
                return false;
            }

            NextButton.IsEnabled = true;
            return true;
        }

        private async void OnNextClicked(object sender, EventArgs e)
        {
            if (!await ValidatePlayerName(PlayerNameEntry.Text))
                return;

            try
            {
                // Disable the button to prevent double-clicks
                NextButton.IsEnabled = false;

                // Navigate to starter selection with simplified parameters (no difficulty)
                await Navigation.PushAsync(new StarterSelectionPage(PlayerNameEntry.Text.Trim(), _profileService));
            }
            catch (Exception ex)
            {
                // Re-enable button on error
                NextButton.IsEnabled = true;
                await DisplayAlert("Error", $"Failed to proceed to starter selection: {ex.Message}", "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Clear form when returning to this page
            PlayerNameEntry.Text = "";
            NameValidationLabel.IsVisible = false;
            NextButton.IsEnabled = false;
        }
    }
}