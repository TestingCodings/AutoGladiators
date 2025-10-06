using Microsoft.Maui.Controls;
using System;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.Services.Logging;
using AutoGladiators.Core.Models;
using System.Threading.Tasks;

namespace AutoGladiators.Client.Pages
{
    public partial class AdventurePage : ContentPage
    {
        private static readonly IAppLogger Log = AppLog.For<AdventurePage>();
        private readonly GameStateService _gameState;
        private readonly WorldMapService _mapService;
        private Button[,] _mapButtons;

        public AdventurePage()
        {
            InitializeComponent();
            _gameState = GameStateService.Instance;
            _mapService = WorldMapService.Instance;
            _mapButtons = new Button[WorldMapService.MAP_SIZE, WorldMapService.MAP_SIZE];
            
            CreateInteractiveMap();
            UpdateStatusDisplay();
            UpdateLocationInfo();
            Log.Info("AdventurePage initialized with interactive map");
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
                    
                    var statusText = $"Player: {player.PlayerName} (Lv.{player.Level})  |  ";
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
        
        private void CreateInteractiveMap()
        {
            Log.Info("Creating interactive map grid...");
            
            try
            {
                // Clear existing children
                MapGrid.Children.Clear();
                
                // Create buttons for each map tile
                for (int x = 0; x < WorldMapService.MAP_SIZE; x++)
                {
                    for (int y = 0; y < WorldMapService.MAP_SIZE; y++)
                    {
                        var tile = _mapService.GetTile(x, y);
                        if (tile == null) continue;

                        var button = new Button
                        {
                            Text = tile.Icon,
                            FontSize = 20,
                            BackgroundColor = Color.FromArgb(tile.BackgroundColor),
                            TextColor = Colors.White,
                            CornerRadius = 8,
                            Margin = new Thickness(1),
                            Padding = new Thickness(0)
                        };

                        // Store coordinates in command parameter
                        button.CommandParameter = new { X = x, Y = y };
                        button.Clicked += OnMapTileClicked;

                        // Add to grid
                        Grid.SetRow(button, y);
                        Grid.SetColumn(button, x);
                        MapGrid.Children.Add(button);
                        
                        // Store reference for later updates
                        _mapButtons[x, y] = button;
                    }
                }
                
                RefreshMapDisplay();
                Log.Info("Interactive map created successfully");
            }
            catch (Exception ex)
            {
                Log.Error($"Error creating interactive map: {ex.Message}", ex);
            }
        }

        private void RefreshMapDisplay()
        {
            try
            {
                var (playerX, playerY) = _mapService.PlayerPosition;
                
                // Update all tile appearances
                for (int x = 0; x < WorldMapService.MAP_SIZE; x++)
                {
                    for (int y = 0; y < WorldMapService.MAP_SIZE; y++)
                    {
                        var button = _mapButtons[x, y];
                        var tile = _mapService.GetTile(x, y);
                        
                        if (button == null || tile == null) continue;

                        // Update tile appearance
                        button.BackgroundColor = Color.FromArgb(tile.BackgroundColor);
                        
                        // Highlight current position
                        if (x == playerX && y == playerY)
                        {
                            button.BorderColor = Colors.Gold;
                            button.BorderWidth = 3;
                            button.Text = $"📍";
                        }
                        else
                        {
                            button.BorderColor = Color.FromArgb(tile.BorderColor);
                            button.BorderWidth = tile.IsExplored ? 2 : 1;
                            button.Text = tile.Icon;
                        }
                        
                        // Update button state for movement
                        if (_mapService.CanMoveTo(x, y))
                        {
                            button.Opacity = 1.0;
                        }
                        else if (x == playerX && y == playerY)
                        {
                            button.Opacity = 1.0; // Current position always visible
                        }
                        else
                        {
                            button.Opacity = tile.IsExplored ? 0.8 : 0.5;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error refreshing map display: {ex.Message}", ex);
            }
        }

        private async void OnMapTileClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter != null)
                {
                    var coordinates = button.CommandParameter;
                    var coordinatesType = coordinates.GetType();
                    int x = (int)coordinatesType.GetProperty("X")?.GetValue(coordinates)!;
                    int y = (int)coordinatesType.GetProperty("Y")?.GetValue(coordinates)!;
                    
                    var tile = _mapService.GetTile(x, y);
                    if (tile == null) return;

                    var (currentX, currentY) = _mapService.PlayerPosition;
                    
                    // If clicking current position, explore
                    if (x == currentX && y == currentY)
                    {
                        OnExploreCurrentLocationClicked(sender, e);
                        return;
                    }

                    // Try to move to the clicked tile
                    if (_mapService.CanMoveTo(x, y))
                    {
                        if (_mapService.MovePlayerTo(x, y))
                        {
                            RefreshMapDisplay();
                            UpdateLocationInfo();
                            
                            // Animate movement
                            await button.ScaleTo(1.2, 100);
                            await button.ScaleTo(1.0, 100);
                        }
                    }
                    else
                    {
                        // Show why movement isn't possible
                        var distance = Math.Abs(x - currentX) + Math.Abs(y - currentY);
                        if (distance > 1)
                        {
                            await DisplayAlert("Can't Travel There", 
                                "You can only move to adjacent areas. Plan your route step by step!", 
                                "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error handling map tile click: {ex.Message}", ex);
            }
        }

        private async void OnExploreCurrentLocationClicked(object sender, EventArgs e)
        {
            var currentTile = _mapService.GetCurrentTile();
            
            // Map terrain to old location system for compatibility
            var locationInfo = GetLocationInfoFromTerrain(currentTile.Terrain);
            if (locationInfo.HasValue)
            {
                await ExploreLocation(locationInfo.Value.Name, locationInfo.Value.Id, locationInfo.Value.MinLevel, locationInfo.Value.MaxLevel);
            }
            else
            {
                await DisplayAlert("Safe Area", "This is a safe zone with no wild bot encounters.", "OK");
            }
        }

        private (string Name, string Id, int MinLevel, int MaxLevel)? GetLocationInfoFromTerrain(TerrainType terrain)
        {
            return terrain switch
            {
                TerrainType.ScrapYards => ("Scrap Yards", "ScrapYards", 1, 3),
                TerrainType.ElectricWastes => ("Electric Wastes", "ElectricWastes", 3, 6),
                TerrainType.VolcanicDepths => ("Volcanic Depths", "VolcanicDepths", 5, 9),
                TerrainType.CrystalCaverns => ("Crystal Caverns", "CrystalCaverns", 8, 15),
                _ => null
            };
        }

        private void UpdateLocationInfo()
        {
            try
            {
                var currentTile = _mapService.GetCurrentTile();
                var (x, y) = _mapService.PlayerPosition;
                
                CurrentLocationName.Text = $"{currentTile.Icon} {currentTile.Name}";
                CurrentLocationDescription.Text = currentTile.Description;
                LocationInfoLabel.Text = $"📍 Position: ({x}, {y}) | Explored: {(currentTile.IsExplored ? "✅" : "❌")}";
                
                // Update explore button
                if (currentTile.Terrain == TerrainType.StartingArea)
                {
                    ExploreButton.Text = "🏠 Safe Area - No Encounters";
                    ExploreButton.BackgroundColor = Colors.Gray;
                    ExploreButton.IsEnabled = false;
                }
                else
                {
                    ExploreButton.Text = $"🔍 Search for Bots ({GetDifficultyStars(currentTile.Difficulty)})";
                    ExploreButton.BackgroundColor = Color.FromArgb("#4A9EFF");
                    ExploreButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error updating location info: {ex.Message}", ex);
            }
        }

        private string GetDifficultyStars(DifficultyLevel difficulty) => difficulty switch
        {
            DifficultyLevel.Safe => "Safe",
            DifficultyLevel.Easy => "★",
            DifficultyLevel.Medium => "★★",
            DifficultyLevel.Hard => "★★★",
            DifficultyLevel.Extreme => "★★★★",
            _ => "?"
        };



        private async void OnViewTravelLogClicked(object sender, EventArgs e)
        {
            try
            {
                var exploredCount = 0;
                var totalTiles = WorldMapService.MAP_SIZE * WorldMapService.MAP_SIZE;
                
                for (int x = 0; x < WorldMapService.MAP_SIZE; x++)
                {
                    for (int y = 0; y < WorldMapService.MAP_SIZE; y++)
                    {
                        var tile = _mapService.GetTile(x, y);
                        if (tile?.IsExplored == true) exploredCount++;
                    }
                }
                
                var progress = (exploredCount * 100) / totalTiles;
                
                await DisplayAlert("🗂️ Travel Log", 
                    $"📊 Exploration Progress: {exploredCount}/{totalTiles} ({progress}%)\n\n" +
                    $"🏠 Base Camp: Always safe\n" +
                    $"🏭 Scrap Yards: Easy encounters\n" +
                    $"⚡ Electric Wastes: Medium encounters\n" +
                    $"🌋 Volcanic Depths: Hard encounters\n" +
                    $"🔮 Crystal Caverns: Extreme encounters\n\n" +
                    $"💡 Tip: You can only move to adjacent areas!", 
                    "Close");
            }
            catch (Exception ex)
            {
                Log.Error($"Error showing travel log: {ex.Message}", ex);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateStatusDisplay();
            UpdateLocationInfo();
            RefreshMapDisplay();
        }
    }
}
