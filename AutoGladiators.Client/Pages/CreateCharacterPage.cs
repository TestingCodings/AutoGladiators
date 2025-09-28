using AutoGladiators.Core.Core;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.Services;
using Microsoft.Maui.Controls;

namespace AutoGladiators.Client.Pages
{
    public sealed class CreateCharacterPage : ContentPage
    {
        public CreateCharacterPage()
        {
            Title = "Create Character";

            var nameEntry = new Entry { Placeholder = "Enter your character name" };
            var startButton = new Button { Text = "Start Adventure" };

            startButton.Clicked += async (_, __) =>
            {
                var playerName = nameEntry.Text?.Trim();
                if (string.IsNullOrWhiteSpace(playerName))
                {
                    await DisplayAlert("Error", "Please enter a character name.", "OK");
                    return;
                }

                var game = GameStateService.Instance;

                // Use InitializeNewGame to set up the player profile and inventory
                game.InitializeNewGame(playerName);

                // Optionally, add a starter bot
                var starterBot = new GladiatorBot
                {
                    Name = $"{playerName}'s Bot",
                    Level = 1,
                    MaxHealth = 50,
                    Strength = 10,
                    Defense = 8
                };
                game.BotRoster.Clear();
                game.BotRoster.Add(starterBot);

                // Save profile (if you have a save service, hook it here)
                // SaveService.Save(game.CurrentPlayer, game.BotRoster);

                // Jump into ExploringState
                await GameRuntime.Instance.GoToAsync(GameStateId.Exploring);

                // Remove this page from back stack so player can't "go back" to creation
                Navigation.RemovePage(this);
            };

            Content = new VerticalStackLayout
            {
                Padding = 24,
                Children =
                {
                    new Label { Text = "Create your character", FontAttributes = FontAttributes.Bold, FontSize = 20 },
                    nameEntry,
                    startButton
                }
            };
        }
    }
}
            
