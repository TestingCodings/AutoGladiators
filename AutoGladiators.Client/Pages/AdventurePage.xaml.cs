using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Logic;
using AutoGladiators.Core.Services;
using System.Linq;


namespace AutoGladiators.Client.Pages
{
    public partial class AdventurePage : ContentPage
    {
        private Label? _statusLabel;
        private Button? _enterWildsButton;
        private readonly AutoGladiators.Core.Services.GameStateService _gameState;

        public AdventurePage()
        {
            InitializeComponent();
            _gameState = AutoGladiators.Core.Services.GameStateService.Instance;
            
            // Initialize game state if needed
            if (_gameState.CurrentPlayer == null)
            {
                _gameState.InitializeNewGame("Adventure Player");
            }
            
            CreateMVPInterface();
            UpdateStatusDisplay();
        }
        
        private void CreateMVPInterface()
        {
            // Clear existing content and create MVP interface
            Content = new StackLayout
            {
                Padding = new Thickness(20),
                Spacing = 20,
                BackgroundColor = Colors.Black,
                Children =
                {
                    new Label
                    {
                        Text = "Adventure Mode",
                        FontSize = 24,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.Gold,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    (_statusLabel = new Label
                    {
                        FontSize = 16,
                        TextColor = Colors.White,
                        HorizontalOptions = LayoutOptions.Center,
                        Margin = new Thickness(0, 10)
                    }),
                    (_enterWildsButton = new Button
                    {
                        Text = "Enter the Wilds",
                        FontSize = 18,
                        BackgroundColor = Colors.DarkGreen,
                        TextColor = Colors.White,
                        CornerRadius = 10,
                        Padding = new Thickness(20, 15),
                        Margin = new Thickness(0, 20)
                    })
                }
            };
            
            _enterWildsButton.Clicked += OnEnterWildsClicked;
        }

        private async void OnEnterWildsClicked(object sender, EventArgs e)
        {
            try
            {
                // Generate encounter through GameStateService
                var encounterGenerator = new AutoGladiators.Core.Services.EncounterGenerator();
                var enemyBot = encounterGenerator.GenerateWildEncounter("Wilds");
                
                if (enemyBot != null)
                {
                    // Set the encounter in game state
                    _gameState.CurrentEncounter = enemyBot;
                    
                    // Get player's current bot
                    var playerBot = _gameState.GetCurrentBot();
                    
                    if (playerBot != null && enemyBot != null)
                    {
                        // Navigate to battle
                        await Navigation.PushAsync(new BattlePage(playerBot, enemyBot));
                    }
                    else
                    {
                        await DisplayAlert("Error", "No bot available for battle!", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("No Encounter", "The wilds are quiet today...", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to enter wilds: {ex.Message}", "OK");
            }
        }
        
        private void UpdateStatusDisplay()
        {
            if (_gameState.CurrentPlayer != null)
            {
                var player = _gameState.CurrentPlayer;
                var botCount = _gameState.BotRoster.Count;
                _statusLabel.Text = $"Player: {player.playerName}\n" +
                                   $"Level: {player.Level} | XP: {player.Experience}\n" +
                                   $"Gold: {player.Gold} | Bots: {botCount}";
            }
        }
        
        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateStatusDisplay();
        }
    }
}
