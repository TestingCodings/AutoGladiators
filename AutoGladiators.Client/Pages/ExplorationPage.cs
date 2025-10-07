using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Models.Exploration;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Services.Exploration;
using AutoGladiators.Core.Rng;

namespace AutoGladiators.Client.Pages
{
    public partial class ExplorationPage : ContentPage
    {
        private readonly PlayerProfileService _playerProfileService;
        private readonly WorldManager _worldManager;
        private readonly MovementManager _movementManager;
        private readonly AutoGladiators.Core.Services.Exploration.EncounterService _encounterService;
        private readonly IRng _rngService;
        
        private PlayerProfile? _currentPlayer;
        private WorldZone? _currentZone;
        
        // UI Elements
        private Grid _mapGrid;
        private Label _locationLabel;
        private Label _playerInfoLabel;
        private Button _northButton;
        private Button _southButton;
        private Button _eastButton;
        private Button _westButton;
        private Button _interactButton;
        private Button _menuButton;
        
        // Map rendering
        private const int TILE_SIZE = 40;
        private const int VISIBLE_RANGE = 5;
        
        public ExplorationPage(PlayerProfileService playerProfileService, 
                             WorldManager worldManager, 
                             MovementManager movementManager,
                             AutoGladiators.Core.Services.Exploration.EncounterService encounterService,
                             IRng rngService)
        {
            _playerProfileService = playerProfileService;
            _worldManager = worldManager;
            _movementManager = movementManager;
            _encounterService = encounterService;
            _rngService = rngService;
            
            InitializeComponent();
            SetupEventHandlers();
        }
        
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadPlayerAndWorld();
            UpdateDisplay();
        }
        
        private void InitializeComponent()
        {
            Title = "Exploration";
            BackgroundColor = Colors.DarkGreen;
            
            // Create main layout
            var mainLayout = new Grid();
            mainLayout.RowDefinitions.Add(new RowDefinition { Height = 60 });
            mainLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            mainLayout.RowDefinitions.Add(new RowDefinition { Height = 120 });
            
            // Info bar
            var infoStack = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                BackgroundColor = Colors.Black,
                Padding = new Thickness(10, 5)
            };
            
            _locationLabel = new Label
            {
                Text = "Loading...",
                TextColor = Colors.White,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold
            };
            
            _playerInfoLabel = new Label
            {
                Text = "Player Info",
                TextColor = Colors.LightGray,
                FontSize = 12
            };
            
            infoStack.Children.Add(_locationLabel);
            infoStack.Children.Add(_playerInfoLabel);
            Grid.SetRow(infoStack, 0);
            mainLayout.Children.Add(infoStack);
            
            // Map area with scroll view
            var scrollView = new ScrollView
            {
                BackgroundColor = Colors.DarkGreen
            };
            
            _mapGrid = new Grid
            {
                BackgroundColor = Colors.Green,
                Padding = new Thickness(10)
            };
            
            scrollView.Content = _mapGrid;
            Grid.SetRow(scrollView, 1);
            mainLayout.Children.Add(scrollView);
            
            // Control buttons
            var controlGrid = new Grid
            {
                BackgroundColor = Colors.Black,
                Padding = new Thickness(10)
            };
            
            // Set up control grid columns and rows
            for (int i = 0; i < 5; i++)
                controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            for (int i = 0; i < 3; i++)
                controlGrid.RowDefinitions.Add(new RowDefinition { Height = 30 });
            
            // Movement buttons
            _northButton = CreateMovementButton("↑", Colors.DarkBlue);
            _southButton = CreateMovementButton("↓", Colors.DarkBlue);
            _eastButton = CreateMovementButton("→", Colors.DarkBlue);
            _westButton = CreateMovementButton("←", Colors.DarkBlue);
            
            // Action buttons
            _interactButton = CreateActionButton("Interact", Colors.DarkRed);
            _menuButton = CreateActionButton("Menu", Colors.DarkGray);
            
            // Position buttons
            Grid.SetColumn(_northButton, 1);
            Grid.SetRow(_northButton, 0);
            controlGrid.Children.Add(_northButton);
            
            Grid.SetColumn(_westButton, 0);
            Grid.SetRow(_westButton, 1);
            controlGrid.Children.Add(_westButton);
            
            Grid.SetColumn(_interactButton, 1);
            Grid.SetRow(_interactButton, 1);
            controlGrid.Children.Add(_interactButton);
            
            Grid.SetColumn(_eastButton, 2);
            Grid.SetRow(_eastButton, 1);
            controlGrid.Children.Add(_eastButton);
            
            Grid.SetColumn(_southButton, 1);
            Grid.SetRow(_southButton, 2);
            controlGrid.Children.Add(_southButton);
            
            Grid.SetColumn(_menuButton, 4);
            Grid.SetRow(_menuButton, 0);
            controlGrid.Children.Add(_menuButton);
            
            Grid.SetRow(controlGrid, 2);
            mainLayout.Children.Add(controlGrid);
            
            Content = mainLayout;
        }
        
        private Button CreateMovementButton(string text, Color backgroundColor)
        {
            return new Button
            {
                Text = text,
                BackgroundColor = backgroundColor,
                TextColor = Colors.White,
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 5
            };
        }
        
        private Button CreateActionButton(string text, Color backgroundColor)
        {
            return new Button
            {
                Text = text,
                BackgroundColor = backgroundColor,
                TextColor = Colors.White,
                FontSize = 14,
                CornerRadius = 5
            };
        }
        
        private void SetupEventHandlers()
        {
            _northButton.Clicked += async (s, e) => await MovePlayer(MovementDirection.North);
            _southButton.Clicked += async (s, e) => await MovePlayer(MovementDirection.South);
            _eastButton.Clicked += async (s, e) => await MovePlayer(MovementDirection.East);
            _westButton.Clicked += async (s, e) => await MovePlayer(MovementDirection.West);
            
            _interactButton.Clicked += OnInteractClicked;
            _menuButton.Clicked += OnMenuClicked;
            
            // Subscribe to movement events
            _movementManager.OnMovementCompleted += OnMovementCompleted;
            _movementManager.OnMovementBlocked += OnMovementBlocked;
            
            // Subscribe to encounter events
            _encounterService.OnWildEncounter += OnWildEncounter;
        }
        
        private async Task LoadPlayerAndWorld()
        {
            try
            {
                // Get current player profile from service
                _currentPlayer = _playerProfileService.GetCurrentProfile();
                
                if (_currentPlayer == null)
                {
                    // Try to load the most recent profile
                    var profiles = await _playerProfileService.GetSavedProfiles();
                    if (profiles.Any())
                    {
                        var latestProfile = profiles.OrderByDescending(p => p.LastPlayedDate).First();
                        _currentPlayer = await _playerProfileService.LoadProfile(latestProfile.Id);
                    }
                }
                
                if (_currentPlayer == null)
                {
                    await DisplayAlert("Error", "No player profile found. Please create a character first.", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }
                
                // Initialize world if not already done
                if (_worldManager.CurrentZone == null)
                {
                    _worldManager.InitializeDefaultWorld();
                    _encounterService.InitializeEncounterTables(_worldManager);
                }
                
                // Load current zone
                _currentZone = _worldManager.GetZone(_currentPlayer.WorldPosition.ZoneId);
                if (_currentZone == null)
                {
                    // Reset to starter town if zone not found
                    _currentPlayer.WorldPosition = new WorldPosition("starter_town", 10, 10);
                    _currentZone = _worldManager.GetZone("starter_town");
                }
                
                _worldManager.ChangeZone(_currentPlayer.WorldPosition.ZoneId);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load world: {ex.Message}", "OK");
            }
        }
        
        private void UpdateDisplay()
        {
            if (_currentPlayer == null || _currentZone == null) return;
            
            // Update info labels
            _locationLabel.Text = $"{_currentZone.Name} ({_currentPlayer.WorldPosition.X}, {_currentPlayer.WorldPosition.Y})";
            _playerInfoLabel.Text = $"{_currentPlayer.PlayerName} | Bots: {_currentPlayer.TotalBots} | Gold: {_currentPlayer.Gold}";
            
            // Update map display
            RenderMap();
            
            // Update button states
            UpdateMovementButtons();
        }
        
        private void RenderMap()
        {
            _mapGrid.Children.Clear();
            _mapGrid.RowDefinitions.Clear();
            _mapGrid.ColumnDefinitions.Clear();
            
            if (_currentPlayer == null || _currentZone == null) return;
            
            var playerX = _currentPlayer.WorldPosition.X;
            var playerY = _currentPlayer.WorldPosition.Y;
            
            // Create visible area around player
            var startX = Math.Max(0, playerX - VISIBLE_RANGE);
            var endX = Math.Min(_currentZone.Width - 1, playerX + VISIBLE_RANGE);
            var startY = Math.Max(0, playerY - VISIBLE_RANGE);
            var endY = Math.Min(_currentZone.Height - 1, playerY + VISIBLE_RANGE);
            
            var mapWidth = endX - startX + 1;
            var mapHeight = endY - startY + 1;
            
            // Setup grid structure
            for (int i = 0; i < mapWidth; i++)
                _mapGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = TILE_SIZE });
            for (int i = 0; i < mapHeight; i++)
                _mapGrid.RowDefinitions.Add(new RowDefinition { Height = TILE_SIZE });
            
            // Render tiles
            for (int worldX = startX; worldX <= endX; worldX++)
            {
                for (int worldY = startY; worldY <= endY; worldY++)
                {
                    var gridX = worldX - startX;
                    var gridY = worldY - startY;
                    
                    var tileFrame = CreateTileFrame(worldX, worldY);
                    
                    Grid.SetColumn(tileFrame, gridX);
                    Grid.SetRow(tileFrame, gridY);
                    _mapGrid.Children.Add(tileFrame);
                }
            }
        }
        
        private Frame CreateTileFrame(int worldX, int worldY)
        {
            var tile = _currentZone?.GetTile(worldX, worldY);
            var isPlayerPosition = _currentPlayer?.WorldPosition.X == worldX && _currentPlayer?.WorldPosition.Y == worldY;
            
            Color tileColor = GetTileColor(tile?.Type ?? TileType.Grass);
            
            var frame = new Frame
            {
                BackgroundColor = tileColor,
                BorderColor = isPlayerPosition ? Colors.Red : Colors.Black,
                CornerRadius = 2,
                Padding = 2,
                HasShadow = false
            };
            
            // Add content based on what's on the tile
            var content = new Label
            {
                Text = GetTileSymbol(worldX, worldY, isPlayerPosition),
                TextColor = isPlayerPosition ? Colors.White : Colors.Black,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            
            frame.Content = content;
            return frame;
        }
        
        private Color GetTileColor(TileType tileType)
        {
            return tileType switch
            {
                TileType.Grass => Colors.LightGreen,
                TileType.Water => Colors.LightBlue,
                TileType.Mountain => Colors.Gray,
                TileType.Forest => Colors.DarkGreen,
                TileType.Desert => Colors.SandyBrown,
                TileType.Cave => Colors.DarkSlateGray,
                TileType.Building => Colors.Brown,
                TileType.Road => Colors.LightGray,
                TileType.Bridge => Colors.BurlyWood,
                TileType.Obstacle => Colors.DarkRed,
                _ => Colors.LightGreen
            };
        }
        
        private string GetTileSymbol(int worldX, int worldY, bool isPlayerPosition)
        {
            if (isPlayerPosition)
                return "@";
                
            // Check for objects on this tile
            var objects = _currentZone?.GetObjectsAt(worldX, worldY);
            if (objects?.Any() == true)
            {
                var obj = objects.First();
                return obj.Type switch
                {
                    WorldObjectType.NPC => "N",
                    WorldObjectType.Trainer => "T",
                    WorldObjectType.Item => "I",
                    WorldObjectType.Shop => "$",
                    WorldObjectType.HealingCenter => "H",
                    WorldObjectType.Obstacle => "#",
                    WorldObjectType.Gym => "G",
                    _ => "?"
                };
            }
            
            var tile = _currentZone?.GetTile(worldX, worldY);
            return tile?.Type switch
            {
                TileType.Water => "~",
                TileType.Mountain => "^",
                TileType.Forest => "T",
                TileType.Desert => ".",
                TileType.Cave => "C",
                TileType.Building => "B",
                TileType.Road => "=",
                TileType.Obstacle => "X",
                _ => " "
            };
        }
        
        private async Task MovePlayer(MovementDirection direction)
        {
            if (_currentPlayer == null) return;
            
            var success = await _movementManager.MovePlayerAsync(_currentPlayer, direction);
            if (success)
            {
                // Check for random encounters after movement
                await _encounterService.CheckForWildEncounterAsync(_currentPlayer);
                
                // Update display
                UpdateDisplay();
                
                // Save player position
                await _playerProfileService.SaveCurrentProfile();
            }
        }
        
        private void UpdateMovementButtons()
        {
            if (_currentPlayer == null) return;
            
            _northButton.IsEnabled = _movementManager.CanMove(_currentPlayer, MovementDirection.North);
            _southButton.IsEnabled = _movementManager.CanMove(_currentPlayer, MovementDirection.South);
            _eastButton.IsEnabled = _movementManager.CanMove(_currentPlayer, MovementDirection.East);
            _westButton.IsEnabled = _movementManager.CanMove(_currentPlayer, MovementDirection.West);
        }
        
        private void OnMovementCompleted(WorldPosition newPosition)
        {
            // Update current zone if it changed
            if (_currentPlayer?.WorldPosition.ZoneId != newPosition.ZoneId)
            {
                _currentZone = _worldManager.GetZone(newPosition.ZoneId);
            }
            
            UpdateDisplay();
        }
        
        private async void OnMovementBlocked(WorldPosition blockedPosition, string reason)
        {
            await DisplayAlert("Cannot Move", reason, "OK");
        }
        
        private async void OnWildEncounter(PlayerProfile player, GladiatorBot wildGladiator, string zoneName)
        {
            var result = await DisplayAlert("Wild Encounter!", 
                $"A wild {wildGladiator.Name} appeared in {zoneName}!\\n\\nDo you want to battle?", 
                "Battle", "Run Away");
                
            if (result)
            {
                // Get player's current battle bot
                var playerBot = player.BotRoster.FirstOrDefault(b => b.CurrentHealth > 0);
                if (playerBot == null)
                {
                    await DisplayAlert("No Battle-Ready Bots", "All your Gladiators are defeated! Visit a Healing Center.", "OK");
                    return;
                }
                
                try
                {
                    // Navigate to battle page with the encounter
                    var battlePage = new BattlePage(playerBot, wildGladiator);
                    await Navigation.PushAsync(battlePage);
                    
                    // When returning from battle, refresh the display
                    UpdateDisplay();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Battle Error", $"Failed to start battle: {ex.Message}", "OK");
                }
            }
        }
        
        private async void OnInteractClicked(object? sender, EventArgs e)
        {
            if (_currentPlayer == null || _currentZone == null) return;
            
            // Check for interactable objects at current position
            var objects = _currentZone.GetObjectsAt(_currentPlayer.WorldPosition.X, _currentPlayer.WorldPosition.Y);
            var interactableObjects = objects.Where(obj => obj.IsInteractable && obj.CanInteract(_currentPlayer)).ToList();
            
            if (!interactableObjects.Any())
            {
                await DisplayAlert("Interact", "There's nothing to interact with here.", "OK");
                return;
            }
            
            var firstObject = interactableObjects.First();
            await HandleObjectInteraction(firstObject);
        }
        
        private async Task HandleObjectInteraction(WorldObject obj)
        {
            switch (obj.Type)
            {
                case WorldObjectType.NPC:
                    await DisplayAlert(obj.Name, "Hello there! Welcome to the world of Gladiator battles!", "OK");
                    break;
                    
                case WorldObjectType.HealingCenter:
                    if (_currentPlayer != null)
                    {
                        // Heal all bots
                        foreach (var bot in _currentPlayer.BotRoster)
                        {
                            bot.CurrentHealth = bot.MaxHealth;
                        }
                        await DisplayAlert("Healing Center", "Your Gladiators have been fully healed!", "OK");
                        await _playerProfileService.SaveCurrentProfile();
                    }
                    break;
                    
                case WorldObjectType.Shop:
                    await DisplayAlert("Shop", "Welcome to the Gladiator Shop!\\n\\n(Shop system to be implemented)", "OK");
                    break;
                    
                case WorldObjectType.Trainer:
                    await DisplayAlert("Trainer Challenge", $"{obj.Name} wants to battle!\\n\\n(Trainer battle system to be implemented)", "Accept");
                    break;
                    
                default:
                    await DisplayAlert(obj.Name, obj.Description, "OK");
                    break;
            }
        }
        
        private async void OnMenuClicked(object? sender, EventArgs e)
        {
            var action = await DisplayActionSheet("Menu", "Cancel", null, 
                "Gladiator Roster", "Inventory", "Save Game", "Return to Main Menu");
                
            switch (action)
            {
                case "Gladiator Roster":
                    await Shell.Current.GoToAsync("//BotRoster");
                    break;
                    
                case "Inventory":
                    await Shell.Current.GoToAsync("//Inventory");
                    break;
                    
                case "Save Game":
                    if (_currentPlayer != null)
                    {
                        await _playerProfileService.SaveCurrentProfile();
                        await DisplayAlert("Saved", "Game saved successfully!", "OK");
                    }
                    break;
                    
                case "Return to Main Menu":
                    await Shell.Current.GoToAsync("//MainMenu");
                    break;
            }
        }
    }
}