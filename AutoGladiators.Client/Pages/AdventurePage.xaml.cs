using Microsoft.Maui.Controls;
using System;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.Services.Logging;
using AutoGladiators.Core.Models;

namespace AutoGladiators.Client.Pages
{
    public partial class AdventurePage : ContentPage
    {
        private static readonly IAppLogger Log = AppLog.For<AdventurePage>();
        private readonly GameStateService _gameState;

        public AdventurePage()
        {
            InitializeComponent();
            _gameState = GameStateService.Instance;
            
            // Initialize game state if needed
            if (_gameState.CurrentPlayer == null)
            {
                _gameState.InitializeNewGame("Adventurer");
            }
            
            UpdateStatusDisplay();
            Log.Info("AdventurePage initialized");
        }
        
        private async void OnExploreScrapYardsClicked(object sender, EventArgs e)
        {
            await ExploreLocation("Scrap Yards", "ScrapYards", 1, 3);
        }
        
        private async void OnExploreElectricWastesClicked(object sender, EventArgs e)
        {
            await ExploreLocation("Electric Wastes", "ElectricWastes", 3, 6);
        }
        
        private async void OnExploreVolcanicDepthsClicked(object sender, EventArgs e)
        {
            await ExploreLocation("Volcanic Depths", "VolcanicDepths", 5, 9);
        }
        
        private async void OnExploreCrystalCavernsClicked(object sender, EventArgs e)
        {
            await ExploreLocation("Crystal Caverns", "CrystalCaverns", 8, 15);
        }

        private async Task ExploreLocation(string locationName, string locationId, int minLevel, int maxLevel)
        {
            try
            {
                Log.Info($"Exploring {locationName}");
                
                // Check if player has any bots
                var playerBot = _gameState.GetCurrentBot();
                if (playerBot == null)
                {
                    await DisplayAlert("No Bot Available", 
                        $"You need a bot to explore {locationName}!\n\nVisit the Debug Menu to add a bot to your roster, or check your Bot Roster.", 
                        "OK");
                    return;
                }

                // Show exploration message
                await DisplayAlert("Exploring...", 
                    $"You venture into the {locationName} with {playerBot.Name}...\n\nSearching for wild bots to encounter!", 
                    "Continue");

                // Generate encounter based on location difficulty
                var enemyLevel = Math.Max(minLevel, Math.Min(maxLevel, playerBot.Level + Random.Shared.Next(-1, 3)));
                var enemyBot = BotFactory.CreateBot($"{locationId}Bot", enemyLevel);
                
                if (enemyBot == null)
                {
                    await DisplayAlert("No Encounter", $"The {locationName} seems quiet today...\n\nTry exploring again!", "OK");
                    return;
                }

                // Customize enemy based on location
                CustomizeEnemyForLocation(enemyBot, locationId);
                
                Log.Info($"Generated encounter: {enemyBot.Name} (Level {enemyBot.Level}) in {locationName}");
                
                // Set encounter in game state
                _gameState.CurrentEncounter = enemyBot;
                
                // Show encounter preview
                bool shouldBattle = await DisplayAlert("Wild Bot Encounter!", 
                    $"A wild {enemyBot.Name} appears!\n\n" +
                    $"Level: {enemyBot.Level}\n" +
                    $"Element: {enemyBot.ElementalCore}\n" +
                    $"HP: {enemyBot.MaxHealth}\n\n" +
                    $"Do you want to battle with {playerBot.Name}?", 
                    "Battle!", "Retreat");
                
                if (shouldBattle)
                {
                    // Create battle setup
                    var setup = new BattleSetup(playerBot, enemyBot, PlayerInitiated: true);

                    // Navigate to battle
                    await GameLoop.GoToAsync(GameStateId.Battling, new StateArgs
                    {
                        Reason = $"Adventure_{locationId}",
                        Payload = setup
                    });

                    await Navigation.PushAsync(new BattlePage(playerBot, enemyBot));
                }
                else
                {
                    await DisplayAlert("Retreated", $"You safely retreat from the {locationName}.", "OK");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Exploration of {locationName} failed: {ex.Message}", ex);
                await DisplayAlert("Exploration Failed", $"Something went wrong exploring {locationName}: {ex.Message}", "OK");
            }
        }
        
        private void CustomizeEnemyForLocation(GladiatorBot enemyBot, string locationId)
        {
            switch (locationId)
            {
                case "ScrapYards":
                    enemyBot.Name = $"Rusty {enemyBot.Name}";
                    enemyBot.ElementalCore = AutoGladiators.Core.Enums.ElementalCore.Metal;
                    break;
                case "ElectricWastes":
                    enemyBot.Name = $"Charged {enemyBot.Name}";
                    enemyBot.ElementalCore = AutoGladiators.Core.Enums.ElementalCore.Electric;
                    enemyBot.AttackPower = (int)(enemyBot.AttackPower * 1.2); // Electric bonus
                    break;
                case "VolcanicDepths":
                    enemyBot.Name = $"Blazing {enemyBot.Name}";
                    enemyBot.ElementalCore = AutoGladiators.Core.Enums.ElementalCore.Fire;
                    enemyBot.MaxHealth = (int)(enemyBot.MaxHealth * 1.3); // Fire tanky
                    enemyBot.CurrentHealth = enemyBot.MaxHealth;
                    break;
                case "CrystalCaverns":
                    enemyBot.Name = $"Crystal {enemyBot.Name}";
                    enemyBot.ElementalCore = Random.Shared.Next(2) == 0 
                        ? AutoGladiators.Core.Enums.ElementalCore.Ice 
                        : AutoGladiators.Core.Enums.ElementalCore.Metal;
                    enemyBot.AttackPower = (int)(enemyBot.AttackPower * 1.4); // Legendary power
                    enemyBot.MaxHealth = (int)(enemyBot.MaxHealth * 1.4);
                    enemyBot.CurrentHealth = enemyBot.MaxHealth;
                    break;
            }
        }
        
        private async void OnReturnToBaseClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("Returning to main menu from adventure");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                Log.Error($"Navigation back failed: {ex.Message}", ex);
                await DisplayAlert("Error", $"Navigation failed: {ex.Message}", "OK");
            }
        }
        
        private void UpdateStatusDisplay()
        {
            try
            {
                if (_gameState.CurrentPlayer != null)
                {
                    var player = _gameState.CurrentPlayer;
                    var botCount = _gameState.BotRoster.Count;
                    var currentBot = _gameState.GetCurrentBot();
                    
                    var statusText = $"Player: {player.playerName} (Lv.{player.Level})  |  ";
                    statusText += $"XP: {player.Experience}  |  Gold: {player.Gold}  |  Bots: {botCount}";
                    
                    if (currentBot != null)
                    {
                        statusText += $"\nActive Bot: {currentBot.Name} (Lv.{currentBot.Level}) - {currentBot.ElementalCore}";
                    }
                    
                    PlayerStatusLabel.Text = statusText;
                }
                else
                {
                    PlayerStatusLabel.Text = "Initializing adventure...";
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error updating status display: {ex.Message}", ex);
                PlayerStatusLabel.Text = "Status unavailable";
            }
        }
        
        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateStatusDisplay();
        }
    }
}
