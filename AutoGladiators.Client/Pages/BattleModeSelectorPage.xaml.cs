using System;
using Microsoft.Maui.Controls;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.Services.Logging;
using AutoGladiators.Core.Models;

namespace AutoGladiators.Client.Pages
{
    public partial class BattleModeSelectorPage : ContentPage
    {
        private static readonly IAppLogger Log = AppLog.For<BattleModeSelectorPage>();
        
        public BattleModeSelectorPage()
        {
            InitializeComponent();
            Log.Info("BattleModeSelectorPage initialized");
        }

        private async void OnQuickBattleClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("Quick Battle clicked");
                
                // Get the player's current bot
                var playerBot = GameStateService.Instance.GetCurrentBot();
                if (playerBot == null)
                {
                    await DisplayAlert("No Active Bot", "You need to select an active bot first! Go to Bot Roster to choose one.", "OK");
                    return;
                }

                // Generate a random opponent
                var encounterGenerator = new EncounterGenerator();
                var enemyBot = encounterGenerator.GenerateWildEncounter("QuickBattleArena");
                
                if (enemyBot == null)
                {
                    await DisplayAlert("Error", "Failed to generate opponent bot.", "OK");
                    return;
                }

                // Create battle setup
                var setup = new BattleSetup(playerBot, enemyBot, PlayerInitiated: true);

                // Navigate to battle
                await GameLoop.GoToAsync(GameStateId.Battling, new StateArgs
                {
                    Reason = "QuickBattle",
                    Payload = setup
                });

                await Navigation.PushAsync(new BattlePage(playerBot, enemyBot));
            }
            catch (Exception ex)
            {
                Log.Error($"Quick battle failed: {ex.Message}", ex);
                await DisplayAlert("Error", $"Failed to start quick battle: {ex.Message}", "OK");
            }
        }

        private async void OnTournamentClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("Tournament clicked");
                await DisplayAlert("Tournament", "Tournament mode coming soon! This will feature multiple consecutive battles with increasing difficulty and rewards.", "OK");
            }
            catch (Exception ex)
            {
                Log.Error($"Tournament error: {ex.Message}", ex);
                await DisplayAlert("Error", $"Tournament error: {ex.Message}", "OK");
            }
        }

        private async void OnCustomBattleClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("Custom Battle clicked");
                await DisplayAlert("Custom Battle", "Custom battle setup coming soon! You'll be able to choose specific opponents, battle rules, and conditions.", "OK");
            }
            catch (Exception ex)
            {
                Log.Error($"Custom battle error: {ex.Message}", ex);
                await DisplayAlert("Error", $"Custom battle error: {ex.Message}", "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("Back to main menu clicked");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                Log.Error($"Navigation back failed: {ex.Message}", ex);
                await DisplayAlert("Error", $"Navigation failed: {ex.Message}", "OK");
            }
        }
    }
}