using System;
using Microsoft.Maui.Controls;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.Services.Logging;
using AutoGladiators.Core.Enums;

namespace AutoGladiators.Client.Pages
{
    public partial class DebugMenuPage : ContentPage
    {
        private static readonly IAppLogger Log = AppLog.For<DebugMenuPage>();
        
        public DebugMenuPage()
        {
            InitializeComponent();
            Log.Info("DebugMenuPage initialized");
        }

        private async void OnAddBotClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("Adding random bot to roster");
                var bot = BotFactory.CreateBot("RustyCharger", 5);
                
                if (bot != null)
                {
                    GameStateService.Instance.AddBotToRoster(bot);
                    Log.Info($"Successfully added bot {bot.Name} to roster");
                    await DisplayAlert("Success", $"Added {bot.Name} (Level {bot.Level}) to your roster!\n\nStats:\n• HP: {bot.MaxHealth}\n• Attack: {bot.AttackPower}\n• Defense: {bot.Defense}\n• Element: {bot.ElementalCore}", "OK");
                }
                else
                {
                    Log.Error("Failed to create bot - BotFactory returned null");
                    await DisplayAlert("Error", "Failed to create bot. BotFactory returned null.", "OK");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error adding bot: {ex.Message}", ex);
                await DisplayAlert("Error", $"Failed to add bot: {ex.Message}", "OK");
            }
        }

        private async void OnAddHighLevelBotClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("Adding high-level bot to roster");
                var bot = BotFactory.CreateBot("EliteGuardian", 10);
                
                if (bot != null)
                {
                    GameStateService.Instance.AddBotToRoster(bot);
                    Log.Info($"Successfully added high-level bot {bot.Name} to roster");
                    await DisplayAlert("Success", $"Added {bot.Name} (Level {bot.Level}) to your roster!\n\nPowerful Stats:\n• HP: {bot.MaxHealth}\n• Attack: {bot.AttackPower}\n• Defense: {bot.Defense}\n• Element: {bot.ElementalCore}", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "Failed to create high-level bot.", "OK");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error adding high-level bot: {ex.Message}", ex);
                await DisplayAlert("Error", $"Failed to add high-level bot: {ex.Message}", "OK");
            }
        }

        private async void OnGiveChipsClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("Adding control chips to inventory");
                
                // Use modern inventory system that properly stacks items
                var captureDevice = new AutoGladiators.Core.Services.CaptureDevice
                {
                    Name = "Control Chip",
                    Description = "A high-tech chip used to capture and control wild bots.",
                    CaptureRate = 0.75f
                };
                
                AutoGladiators.Core.Services.InventoryService.Instance.AddItem(captureDevice, 5);
                
                Log.Info("Successfully added 5 control chips");
                await DisplayAlert("Inventory Updated", "Added 5 Control Chips to your inventory!\n\nUse these during battles to capture wild bots and add them to your collection.", "OK");
            }
            catch (Exception ex)
            {
                Log.Error($"Error adding chips: {ex.Message}", ex);
                await DisplayAlert("Error", $"Failed to add chips: {ex.Message}", "OK");
            }
        }

        private async void OnGiveEnergyClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("Adding energy packs to inventory");
                
                // Use modern inventory system that properly stacks items
                var energyPotion = new AutoGladiators.Core.Services.EnergyPotion
                {
                    Name = "Energy Pack",
                    Description = "Restores energy to tired bots.",
                    EnergyAmount = 50
                };
                
                AutoGladiators.Core.Services.InventoryService.Instance.AddItem(energyPotion, 3);
                
                await DisplayAlert("Inventory Updated", "Added 3 Energy Packs to your inventory!\n\nUse these to restore energy to your bots during long battles.", "OK");
            }
            catch (Exception ex)
            {
                Log.Error($"Error adding energy packs: {ex.Message}", ex);
                await DisplayAlert("Error", $"Failed to add energy packs: {ex.Message}", "OK");
            }
        }

        private async void OnTestBattleClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("Starting test battle");
                
                // Get the player's current bot
                var playerBot = GameStateService.Instance.GetCurrentBot();
                if (playerBot == null)
                {
                    Log.Warn("No active bot found for battle");
                    await DisplayAlert("No Bot Available", "You need to add a bot to your roster first!\n\nUse 'Add Random Bot to Roster' above, then try again.", "OK");
                    return;
                }

                // Generate a test opponent
                var enemyBot = BotFactory.CreateBot("WildScrapBot", Math.Max(1, playerBot.Level - 1));
                if (enemyBot == null)
                {
                    await DisplayAlert("Error", "Failed to generate opponent bot.", "OK");
                    return;
                }

                Log.Info($"Starting battle: {playerBot.Name} vs {enemyBot.Name}");
                
                // Create battle setup
                var setup = new BattleSetup(playerBot, enemyBot, PlayerInitiated: true);

                // Navigate to battle
                await GameLoop.GoToAsync(GameStateId.Battling, new StateArgs
                {
                    Reason = "DebugTestBattle",
                    Payload = setup
                });

                await Navigation.PushAsync(new BattlePage(playerBot, enemyBot));
            }
            catch (Exception ex)
            {
                Log.Error($"Test battle failed: {ex.Message}", ex);
                await DisplayAlert("Error", $"Failed to start test battle: {ex.Message}", "OK");
            }
        }

        private async void OnHardBattleClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("Starting hard battle");
                
                var playerBot = GameStateService.Instance.GetCurrentBot();
                if (playerBot == null)
                {
                    await DisplayAlert("No Bot Available", "You need to add a bot to your roster first!", "OK");
                    return;
                }

                // Generate a stronger opponent
                var enemyBot = BotFactory.CreateBot("EliteBossBot", playerBot.Level + 3);
                if (enemyBot == null)
                {
                    await DisplayAlert("Error", "Failed to generate boss bot.", "OK");
                    return;
                }

                // Make it tougher
                enemyBot.MaxHealth = (int)(enemyBot.MaxHealth * 1.5);
                enemyBot.CurrentHealth = enemyBot.MaxHealth;
                enemyBot.AttackPower = (int)(enemyBot.AttackPower * 1.3);
                
                var setup = new BattleSetup(playerBot, enemyBot, PlayerInitiated: true);
                await GameLoop.GoToAsync(GameStateId.Battling, new StateArgs
                {
                    Reason = "DebugHardBattle",
                    Payload = setup
                });

                await Navigation.PushAsync(new BattlePage(playerBot, enemyBot));
            }
            catch (Exception ex)
            {
                Log.Error($"Hard battle failed: {ex.Message}", ex);
                await DisplayAlert("Error", $"Failed to start hard battle: {ex.Message}", "OK");
            }
        }

        private async void OnShowStatsClicked(object sender, EventArgs e)
        {
            try
            {
                var gameState = GameStateService.Instance;
                var roster = gameState.BotRoster;
                var inventory = gameState.Inventory;
                
                var stats = $"🤖 GAME STATISTICS\n\n";
                stats += $"Bot Roster: {roster.Count} bots\n";
                stats += $"Inventory Items: {inventory.Items.Count}\n\n";
                
                if (roster.Count > 0)
                {
                    stats += $"ACTIVE BOT:\n";
                    var currentBot = gameState.GetCurrentBot();
                    if (currentBot != null)
                    {
                        stats += $"• {currentBot.Name} (Lv.{currentBot.Level})\n";
                        stats += $"• HP: {currentBot.CurrentHealth}/{currentBot.MaxHealth}\n";
                        stats += $"• Element: {currentBot.ElementalCore}\n\n";
                    }
                    
                    stats += $"ALL BOTS:\n";
                    foreach (var bot in roster)
                    {
                        stats += $"• {bot.Name} (Lv.{bot.Level}) - {bot.ElementalCore}\n";
                    }
                }
                else
                {
                    stats += "No bots in roster yet!\n";
                }
                
                await DisplayAlert("Game Stats", stats, "OK");
            }
            catch (Exception ex)
            {
                Log.Error($"Error showing stats: {ex.Message}", ex);
                await DisplayAlert("Error", $"Failed to show stats: {ex.Message}", "OK");
            }
        }

        private async void OnResetDataClicked(object sender, EventArgs e)
        {
            try
            {
                bool confirm = await DisplayAlert("Reset Game Data", "This will clear all your bots and items.\n\nAre you sure?", "Yes, Reset", "Cancel");
                if (confirm)
                {
                    GameStateService.Instance.BotRoster.Clear();
                    GameStateService.Instance.Inventory.Items.Clear();
                    Log.Info("Game data reset");
                    await DisplayAlert("Reset Complete", "All game data has been cleared.", "OK");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error resetting data: {ex.Message}", ex);
                await DisplayAlert("Error", $"Failed to reset data: {ex.Message}", "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("Returning to main menu from debug console");
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
