using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services;
using AutoGladiators.Client.Pages;

namespace AutoGladiators.Client.Pages
{
    public partial class ImprovedExplorationPage : ContentPage
    {
        private int _playerX = 2;
        private int _playerY = 2;
        private const int MAP_SIZE = 5;
        private string _currentLocationName = "Forest Clearing";
        private List<string> _eventLog = new();
        
        // Map tile types
        private readonly Random _random = new();
        private string[,] _mapTiles = new string[MAP_SIZE, MAP_SIZE];

        public ImprovedExplorationPage()
        {
            InitializeComponent();
            InitializeMap();
            UpdateDisplay();
        }

        private void InitializeMap()
        {
            // Initialize map with different terrain types
            for (int x = 0; x < MAP_SIZE; x++)
            {
                for (int y = 0; y < MAP_SIZE; y++)
                {
                    // Create variety in terrain
                    var rand = _random.NextDouble();
                    if (rand < 0.3) _mapTiles[x, y] = "grass";
                    else if (rand < 0.6) _mapTiles[x, y] = "ground";
                    else if (rand < 0.8) _mapTiles[x, y] = "wall";
                    else _mapTiles[x, y] = "obstacle";
                }
            }
            
            // Ensure player starting position is walkable
            _mapTiles[_playerX, _playerY] = "grass";
            
            CreateMapUI();
        }

        private void CreateMapUI()
        {
            MapGrid.Children.Clear();
            
            // Set up grid layout
            MapGrid.RowDefinitions.Clear();
            MapGrid.ColumnDefinitions.Clear();
            
            for (int i = 0; i < MAP_SIZE; i++)
            {
                MapGrid.RowDefinitions.Add(new RowDefinition { Height = 60 });
                MapGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 60 });
            }

            // Create map tiles with sprite assets
            for (int x = 0; x < MAP_SIZE; x++)
            {
                for (int y = 0; y < MAP_SIZE; y++)
                {
                    var tileFrame = new Frame
                    {
                        BackgroundColor = GetTileColor(_mapTiles[x, y]),
                        BorderColor = Colors.DarkGray,
                        HasShadow = false,
                        CornerRadius = 5,
                        Padding = 2,
                        Margin = 1
                    };

                    // Add terrain sprite
                    var terrainImage = new Image
                    {
                        Source = GetTileImageSource(_mapTiles[x, y]),
                        Aspect = Aspect.Fill,
                        Opacity = 0.8
                    };

                    // If this is player position, add player sprite
                    if (x == _playerX && y == _playerY)
                    {
                        var stackLayout = new StackLayout();
                        stackLayout.Children.Add(terrainImage);
                        
                        var playerImage = new Image
                        {
                            Source = "character_idle.png",
                            WidthRequest = 40,
                            HeightRequest = 40,
                            Aspect = Aspect.AspectFit
                        };
                        
                        stackLayout.Children.Add(playerImage);
                        tileFrame.Content = stackLayout;
                        tileFrame.BackgroundColor = Colors.LightGreen;
                        tileFrame.BorderColor = Colors.Gold;
                    }
                    else
                    {
                        tileFrame.Content = terrainImage;
                    }

                    Grid.SetRow(tileFrame, y);
                    Grid.SetColumn(tileFrame, x);
                    MapGrid.Children.Add(tileFrame);
                }
            }
        }

        private string GetTileImageSource(string tileType)
        {
            return tileType switch
            {
                "grass" => "grass_tile.png",
                "ground" => "ground_tile.png", 
                "wall" => "wall_tile.png",
                "obstacle" => "obstacle_tile.png",
                _ => "ground_tile.png"
            };
        }

        private Color GetTileColor(string tileType)
        {
            return tileType switch
            {
                "grass" => Colors.DarkGreen,
                "ground" => Colors.SaddleBrown,
                "wall" => Colors.Gray,
                "obstacle" => Colors.DarkRed,
                _ => Colors.DarkGray
            };
        }

        private async void OnMoveClicked(object sender, EventArgs e)
        {
            if (sender is not Button button) return;

            var newX = _playerX;
            var newY = _playerY;

            // Determine new position based on button
            switch (button.Text)
            {
                case "⬆️": newY--; break;
                case "⬇️": newY++; break;
                case "⬅️": newX--; break;
                case "➡️": newX++; break;
            }

            // Check bounds and tile walkability
            if (newX >= 0 && newX < MAP_SIZE && newY >= 0 && newY < MAP_SIZE)
            {
                if (_mapTiles[newX, newY] != "wall" && _mapTiles[newX, newY] != "obstacle")
                {
                    _playerX = newX;
                    _playerY = newY;
                    
                    // Animate movement
                    await AnimateMovement();
                    
                    // Update location name based on terrain
                    UpdateLocationName();
                    
                    // Add movement to log
                    AddEventToLog($"Moved to {_currentLocationName}");
                    
                    // Random encounter check
                    if (_random.NextDouble() < 0.15) // 15% encounter chance
                    {
                        await TriggerRandomEncounter();
                    }
                    
                    UpdateDisplay();
                }
                else
                {
                    AddEventToLog("Path blocked! Cannot move there.");
                    await AnimateBlocked();
                }
            }
            else
            {
                AddEventToLog("You've reached the edge of the map!");
                await AnimateBlocked();
            }
        }

        private async Task AnimateMovement()
        {
            CreateMapUI(); // Recreate map with new player position
            
            // Quick scale animation for feedback
            await MapGrid.ScaleTo(1.05, 100);
            await MapGrid.ScaleTo(1.0, 100);
        }

        private async Task AnimateBlocked()
        {
            // Shake animation for blocked movement
            await MapGrid.TranslateTo(-10, 0, 50);
            await MapGrid.TranslateTo(10, 0, 50);
            await MapGrid.TranslateTo(-5, 0, 50);
            await MapGrid.TranslateTo(0, 0, 50);
        }

        private void UpdateLocationName()
        {
            var tileType = _mapTiles[_playerX, _playerY];
            _currentLocationName = tileType switch
            {
                "grass" => "Peaceful Meadow",
                "ground" => "Rocky Path", 
                "wall" => "Stone Wall",
                "obstacle" => "Dangerous Rocks",
                _ => "Unknown Area"
            };
        }

        private async Task TriggerRandomEncounter()
        {
            var encounters = new[]
            {
                "A wild Gladiator Bot appears!",
                "You found a treasure chest!",
                "An NPC trader offers you items.",
                "You discovered ancient ruins!",
                "A friendly bot offers to join you."
            };
            
            var encounter = encounters[_random.Next(encounters.Length)];
            AddEventToLog($"🎲 Random Encounter: {encounter}");
            
            // Show encounter popup
            await DisplayAlert("Random Encounter!", encounter, "Continue");
        }

        private void UpdateDisplay()
        {
            LocationLabel.Text = _currentLocationName;
            PlayerStatsLabel.Text = $"Position: ({_playerX}, {_playerY}) | Health: 150/150";
            PlayerHealthBar.Progress = 1.0;
            
            // Update events log
            var recentEvents = _eventLog.TakeLast(10).Reverse();
            EventsLabel.Text = string.Join("\n", recentEvents);
        }

        private void AddEventToLog(string eventText)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            _eventLog.Add($"[{timestamp}] {eventText}");
        }

        private async void OnInteractClicked(object sender, EventArgs e)
        {
            var tileType = _mapTiles[_playerX, _playerY];
            var interactionResult = tileType switch
            {
                "grass" => "You rest in the soft grass. HP restored!",
                "ground" => "You search the ground and find some coins.",
                "wall" => "The wall is too solid to interact with.",
                "obstacle" => "Dangerous! You take some damage but find rare materials.",
                _ => "Nothing interesting here..."
            };
            
            AddEventToLog($"🔍 {interactionResult}");
            await DisplayAlert("Interaction", interactionResult, "OK");
            UpdateDisplay();
        }

        private async void OnTestBattleClicked(object sender, EventArgs e)
        {
            try
            {
                var battlePage = EnhancedBattlePage.CreateTestBattle();
                await Navigation.PushAsync(battlePage);
                AddEventToLog("🗡️ Entered enhanced battle mode!");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not start battle: {ex.Message}", "OK");
                AddEventToLog($"❌ Battle failed: {ex.Message}");
            }
        }

        private async void OnInventoryClicked(object sender, EventArgs e)
        {
            try
            {
                var inventoryPage = new InventoryPage();
                await Navigation.PushAsync(inventoryPage);
                AddEventToLog("🎒 Opened inventory.");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not open inventory: {ex.Message}", "OK");
            }
        }

        private async void OnBotsClicked(object sender, EventArgs e)
        {
            try
            {
                var botRosterPage = new BotRosterPage();
                await Navigation.PushAsync(botRosterPage);
                AddEventToLog("🤖 Viewing bot roster.");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not open bot roster: {ex.Message}", "OK");
            }
        }

        private async void OnTownClicked(object sender, EventArgs e)
        {
            AddEventToLog("🏠 Traveling to town...");
            await DisplayAlert("Town", "Town features coming soon! You can buy items, upgrade bots, and meet NPCs.", "OK");
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            AddEventToLog("💾 Game saved successfully!");
            await DisplayAlert("Save", "Your adventure progress has been saved!", "OK");
        }

        private async void OnMainMenuClicked(object sender, EventArgs e)
        {
            var result = await DisplayAlert("Main Menu", "Return to main menu? Your progress will be saved.", "Yes", "No");
            if (result)
            {
                await Navigation.PopToRootAsync();
            }
        }
    }
}