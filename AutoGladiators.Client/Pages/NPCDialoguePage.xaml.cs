using System;
using System.Linq;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Services.Storage;
using AutoGladiators.Client.ViewModels;
using AutoGladiators.Core.Models;
using Microsoft.Maui.Controls;

namespace AutoGladiators.Client.Pages
{
    public partial class NPCDialoguePage : ContentPage
    {
        private readonly NPCDialogueViewModel _viewModel;
        private readonly string _dialogueFile;

        public NPCDialoguePage(string dialogueFile)
        {
            InitializeComponent();
            
            _dialogueFile = dialogueFile;
            
            // Create storage service and dialogue loader
            var storage = new AutoGladiators.Client.Services.Storage.AppStorage();
            var dialogueLoader = new NPCDialogueLoader(storage);
            
            _viewModel = new NPCDialogueViewModel("NPC", dialogueLoader);
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            try
            {
                await _viewModel.InitializeFromFile(_dialogueFile);
                UpdateDialogueOptions();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load dialogue: {ex.Message}", "OK");
            }
        }

        private void UpdateDialogueOptions()
        {
            // Clear existing options
            OptionsContainer.Children.Clear();

            if (_viewModel.CurrentOptions == null || !_viewModel.CurrentOptions.Any())
            {
                // Add a default close option
                var closeButton = new Button
                {
                    Text = "Close Dialogue",
                    BackgroundColor = Colors.Gray,
                    TextColor = Colors.White,
                    CornerRadius = 5,
                    Margin = new Thickness(0, 5)
                };
                closeButton.Clicked += async (_, _) => await Navigation.PopAsync();
                OptionsContainer.Children.Add(closeButton);
                return;
            }

            // Add option buttons
            for (int i = 0; i < _viewModel.CurrentOptions.Count; i++)
            {
                var option = _viewModel.CurrentOptions[i];
                var button = new Button
                {
                    Text = $"{i + 1}. {option.Text}",
                    BackgroundColor = Colors.DarkBlue,
                    TextColor = Colors.White,
                    CornerRadius = 8,
                    Margin = new Thickness(0, 5),
                    Padding = new Thickness(15, 10),
                    HorizontalOptions = LayoutOptions.Fill
                };

                button.Clicked += (_, _) =>
                {
                    var cmd = _viewModel.SelectOptionCommand;
                    if (cmd?.CanExecute(option) == true)
                    {
                        cmd.Execute(option);
                        UpdateDialogueOptions(); // Refresh options after selection
                    }
                };

                OptionsContainer.Children.Add(button);
            }
        }

        private async void OnLeaveClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnInventoryClicked(object sender, EventArgs e)
        {
            // Navigate to inventory page
            await Navigation.PushAsync(new InventoryPage());
        }
    }
}