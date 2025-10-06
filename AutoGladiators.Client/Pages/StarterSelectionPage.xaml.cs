using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services.Logging;
using System;
using System.Linq;
using System.ComponentModel;
using AutoGladiators.Client.Pages;

namespace AutoGladiators.Client.Pages
{
    public partial class StarterSelectionPage : ContentPage
    {
        private static readonly IAppLogger Log = AppLog.For<StarterSelectionPage>();
        
        public class StarterBotViewModel : INotifyPropertyChanged
        {
            private bool _isSelected;
            
            public string BotId { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string ElementalCore { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int AttackPower { get; set; }
            public int Defense { get; set; }
            public int Speed { get; set; }

            public bool IsSelected 
            { 
                get => _isSelected;
                set 
                { 
                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BorderColor)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectionColor)));
                }
            }

            public string ElementIcon => ElementalCore switch
            {
                "Fire" => "🔥",
                "Electric" => "⚡",
                "Ice" => "❄️", 
                "Metal" => "⚙️",
                "Wind" => "💨",
                _ => "⚪"
            };

            public Color ElementColor => ElementalCore switch
            {
                "Fire" => Color.FromArgb("#FF4444"),
                "Electric" => Color.FromArgb("#FFD700"),
                "Ice" => Color.FromArgb("#4A9EFF"),
                "Metal" => Color.FromArgb("#C0C0C0"),
                "Wind" => Color.FromArgb("#00FF88"),
                _ => Color.FromArgb("#B0B0B0")
            };

            public Color BorderColor => IsSelected ? Color.FromArgb("#FFD700") : Color.FromArgb("#2A2A3E");
            public Color SelectionColor => Color.FromArgb("#28A745");

            public event PropertyChangedEventHandler? PropertyChanged;
        }

        private readonly ObservableCollection<StarterBotViewModel> _starterBots = new();
        private StarterBotViewModel? _selectedBot;
        private readonly string _playerName;
        private readonly string _difficulty;
        private readonly PlayerProfileService _profileService;

        public StarterSelectionPage(string playerName, string difficulty, PlayerProfileService profileService)
        {
            InitializeComponent();
            _playerName = playerName;
            _difficulty = difficulty;
            _profileService = profileService;
            LoadStarterBots();
            
            StarterBotList.ItemsSource = _starterBots;
            NicknameEntry.TextChanged += OnNicknameChanged;
        }

        private void LoadStarterBots()
        {
            // Define available starter bots with balanced stats
            var starters = new[]
            {
                new StarterBotViewModel
                {
                    BotId = "FireStarter",
                    Name = "Blaze Guardian",
                    ElementalCore = "Fire",
                    Description = "A fierce warrior with powerful fire attacks. High attack, moderate defense.",
                    AttackPower = 25,
                    Defense = 15,
                    Speed = 20
                },
                new StarterBotViewModel
                {
                    BotId = "ElectricStarter", 
                    Name = "Volt Striker",
                    ElementalCore = "Electric",
                    Description = "Lightning-fast bot with stunning electric moves. High speed, moderate attack.",
                    AttackPower = 20,
                    Defense = 15,
                    Speed = 25
                },
                new StarterBotViewModel
                {
                    BotId = "IceStarter",
                    Name = "Frost Defender",
                    ElementalCore = "Ice", 
                    Description = "Sturdy ice bot with strong defensive capabilities. High defense, moderate speed.",
                    AttackPower = 15,
                    Defense = 25,
                    Speed = 20
                },
                new StarterBotViewModel
                {
                    BotId = "MetalStarter",
                    Name = "Steel Crusher",
                    ElementalCore = "Metal",
                    Description = "Heavily armored bot with crushing metal attacks. Balanced all-around stats.",
                    AttackPower = 20,
                    Defense = 20,
                    Speed = 20
                },
                new StarterBotViewModel
                {
                    BotId = "WindStarter", 
                    Name = "Gale Runner",
                    ElementalCore = "Wind",
                    Description = "Agile wind bot with evasive abilities. Highest speed, lower defense.",
                    AttackPower = 22,
                    Defense = 13,
                    Speed = 25
                }
            };

            foreach (var starter in starters)
            {
                _starterBots.Add(starter);
            }
        }

        private void OnStarterBotTapped(object sender, TappedEventArgs e)
        {
            if (sender is Frame frame && frame.BindingContext is StarterBotViewModel bot)
            {
                // Deselect previous selection
                if (_selectedBot != null)
                    _selectedBot.IsSelected = false;

                // Select new bot
                _selectedBot = bot;
                bot.IsSelected = true;

                // Update nickname hint
                NicknameHint.Text = $"Give {bot.Name} a unique nickname that shows their personality!";
                
                // Enable start button if nickname is provided
                UpdateStartButtonState();

                Log.Info($"Selected starter bot: {bot.Name}");
            }
        }

        private void OnNicknameChanged(object? sender, TextChangedEventArgs e)
        {
            UpdateStartButtonState();
        }

        private void UpdateStartButtonState()
        {
            StartGameButton.IsEnabled = _selectedBot != null && !string.IsNullOrWhiteSpace(NicknameEntry.Text);
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnStartGameClicked(object sender, EventArgs e)
        {
            if (_selectedBot == null || string.IsNullOrWhiteSpace(NicknameEntry.Text))
            {
                await DisplayAlert("Incomplete Selection", "Please select a starter bot and give it a nickname!", "OK");
                return;
            }

            try
            {
                Log.Info($"Starting new game for {_playerName} with {_selectedBot.Name}");

                // Create new player profile with selected starter
                var profile = await _profileService.CreateNewProfile(
                    _playerName, 
                    _difficulty,
                    _selectedBot.BotId, 
                    NicknameEntry.Text.Trim()
                );

                if (profile != null)
                {
                    // Set as current profile (this will sync GameStateService)
                    _profileService.SetCurrentProfile(profile);
                    
                    // Navigate to main game
                    await DisplayAlert("Adventure Begins!", 
                        $"Welcome, {_playerName}! Your journey with {NicknameEntry.Text} starts now!", "Let's Go!");

                    // Navigate directly to adventure page to start exploring
                    await Navigation.PushAsync(new AdventurePage());
                }
                else
                {
                    await DisplayAlert("Error", "Failed to create player profile.", "OK");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to start new game: {ex.Message}", ex);
                await DisplayAlert("Error", "Failed to start the game. Please try again.", "OK");
            }
        }
    }
}