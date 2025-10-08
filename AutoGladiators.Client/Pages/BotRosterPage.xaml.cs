using Microsoft.Maui.Controls;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services;
using AutoGladiators.Client.Pages;
using AutoGladiators.Client.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.ComponentModel;

namespace AutoGladiators.Client.Pages
{
    public partial class BotRosterPage : ContentPage, INotifyPropertyChanged
    {
        private readonly SpriteManager? _spriteManager;
        private readonly AnimationManager? _animationManager;
        private ObservableCollection<BotSummary> _allBots = new();
        private ObservableCollection<BotSummary> _filteredBots = new();
        private string _totalBots = "0";
        private string _avgLevel = "0";
        private string _rarePlusCount = "0";

        public ObservableCollection<BotSummary> FilteredBots
        {
            get => _filteredBots;
            set
            {
                _filteredBots = value;
                OnPropertyChanged();
            }
        }

        public string TotalBots
        {
            get => _totalBots;
            set
            {
                _totalBots = value;
                OnPropertyChanged();
            }
        }

        public string AvgLevel
        {
            get => _avgLevel;
            set
            {
                _avgLevel = value;
                OnPropertyChanged();
            }
        }

        public string RarePlusCount
        {
            get => _rarePlusCount;
            set
            {
                _rarePlusCount = value;
                OnPropertyChanged();
            }
        }

        // Parameterless constructor for direct instantiation (temporarily)
        public BotRosterPage()
        {
            InitializeComponent();
        }

        public BotRosterPage(SpriteManager spriteManager, AnimationManager animationManager)
        {
            _spriteManager = spriteManager;
            _animationManager = animationManager;
            
            InitializeComponent();
            LoadBots();
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            if (_animationManager != null)
            {
                await _animationManager.AnimatePageEntry(this);
            }
        }

        private void LoadBots()
        {
            try
            {
                // Get real bot data from GameStateService
                var gameState = GameStateService.Instance;
                var realBots = gameState.BotRoster ?? new List<GladiatorBot>();

                _allBots = new ObservableCollection<BotSummary>();
                
                // Convert real bots to display format
                foreach (var bot in realBots)
                {
                    var summary = new BotSummary
                    {
                        Name = bot.Name ?? "Unknown",
                        ElementalCore = bot.ElementalCore.ToString(),
                        BotClass = "Gladiator",
                        Level = bot.Level,
                        MaxHealth = bot.MaxHealth,
                        AttackPower = bot.AttackPower,
                        Defense = bot.Defense,
                        Rarity = "Common",
                        Description = bot.Description ?? "No description available",
                        ExpProgress = bot.Experience / Math.Max(1.0, (bot.Level + 1) * 100.0), // XP progress to next level
                        PowerRating = CalculatePowerRating(bot),
                        StatusText = GetStatusText(bot),
                        HasWeapon = false,
                        HasArmor = false,
                        HasAccessory = false,
                        HasModule = false
                    };
                    _allBots.Add(summary);
                }

                // If no real bots found (for testing), add a fallback message
                if (_allBots.Count == 0)
                {
                    _allBots.Add(new BotSummary
                    {
                        Name = "No Bots Found",
                        ElementalCore = "None",
                        BotClass = "N/A",
                        Level = 0,
                        MaxHealth = 0,
                        AttackPower = 0,
                        Defense = 0,
                        Rarity = "None",
                        Description = "No gladiator bots in roster. Start a new game or visit the Debug Menu to add bots.",
                        ExpProgress = 0,
                        PowerRating = 0,
                        StatusText = "Missing",
                        HasWeapon = false,
                        HasArmor = false,
                        HasAccessory = false,
                        HasModule = false
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading bots: {ex.Message}");
                
                // Fallback to empty collection
                _allBots = new ObservableCollection<BotSummary>
                {
                    new BotSummary
                    {
                        Name = "Error Loading Bots",
                        ElementalCore = "Error",
                        BotClass = "Error",
                        Level = 0,
                        MaxHealth = 0,
                        AttackPower = 0,
                        Defense = 0,
                        Rarity = "Error",
                        Description = $"Error loading bot roster: {ex.Message}",
                        ExpProgress = 0,
                        PowerRating = 0,
                        StatusText = "Error",
                        HasWeapon = false,
                        HasArmor = false,
                        HasAccessory = false,
                        HasModule = false
                    }
                };
            }
            
            FilteredBots = new ObservableCollection<BotSummary>(_allBots);
            UpdateCollectionStats();
        }
        
        private int CalculatePowerRating(GladiatorBot bot)
        {
            return (bot.Level * 100) + bot.AttackPower * 5 + bot.Defense * 3 + bot.MaxHealth;
        }
        
        private string GetStatusText(GladiatorBot bot)
        {
            if (bot.CurrentHealth <= 0)
                return "Defeated";
            
            if (bot.CurrentHealth < bot.MaxHealth * 0.3)
                return "Critical";
            
            if (bot.CurrentHealth < bot.MaxHealth * 0.7)
                return "Injured";
                
            return "Ready for Battle";
        }

        private void UpdateCollectionStats()
        {
            TotalBots = _allBots.Count.ToString();
            AvgLevel = _allBots.Any() ? ((int)_allBots.Average(b => b.Level)).ToString() : "0";
            RarePlusCount = _allBots.Count(b => b.Rarity == "Rare" || b.Rarity == "Epic" || b.Rarity == "Legendary").ToString();
        }

        private void OnSortPickerChanged(object sender, EventArgs e)
        {
            if (sender is Picker picker && picker.SelectedItem is string sortOption)
            {
                ApplyFilters();
            }
        }

        private void OnFilterPickerChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filtered = _allBots.AsEnumerable();

            // TODO: Add filtering when UI controls are connected
            // Apply sorting by level for now
            filtered = filtered.OrderByDescending(b => b.Level);

            FilteredBots.Clear();
            foreach (var bot in filtered)
            {
                FilteredBots.Add(bot);
            }
        }

        private int GetRarityValue(string rarity)
        {
            return rarity switch
            {
                "Common" => 1,
                "Rare" => 2,
                "Epic" => 3,
                "Legendary" => 4,
                _ => 0
            };
        }

        private async void OnBotCardTapped(object sender, TappedEventArgs e)
        {
            if (sender is Frame frame && _animationManager != null)
            {
                await _animationManager.AnimateBotCardTap(frame);
                
                if (frame.BindingContext is BotSummary selectedBot)
                {
                    var gladiatorBot = new AutoGladiators.Core.Core.GladiatorBot
                    {
                        Name = selectedBot.Name,
                        ElementalCore = (AutoGladiators.Core.Enums.ElementalCore)Enum.Parse(typeof(AutoGladiators.Core.Enums.ElementalCore), selectedBot.ElementalCore),
                        Description = selectedBot.Description
                    };
                    await Navigation.PushAsync(new BotDetailPage(gladiatorBot) { Bot = gladiatorBot });
                }
            }
        }

        private async void OnBotSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is BotSummary selectedBot)
            {
                await DisplayAlert("Bot Selected", $"{selectedBot.Name} - {selectedBot.ElementalCore}", "OK");
                var gladiatorBot = new AutoGladiators.Core.Core.GladiatorBot
                {
                    Name = selectedBot.Name,
                    ElementalCore = (AutoGladiators.Core.Enums.ElementalCore)Enum.Parse(typeof(AutoGladiators.Core.Enums.ElementalCore), selectedBot.ElementalCore),
                    Description = selectedBot.Description
                };
                await Navigation.PushAsync(new BotDetailPage(gladiatorBot) { Bot = gladiatorBot });
                ((CollectionView)sender).SelectedItem = null;
            }
        }

        private async void OnViewDetailsClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is BotSummary bot)
            {
                var gladiatorBot = new AutoGladiators.Core.Core.GladiatorBot
                {
                    Name = bot.Name,
                    ElementalCore = (AutoGladiators.Core.Enums.ElementalCore)Enum.Parse(typeof(AutoGladiators.Core.Enums.ElementalCore), bot.ElementalCore),
                    Description = bot.Description
                };
                await Navigation.PushAsync(new BotDetailPage(gladiatorBot) { Bot = gladiatorBot });
            }
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
        protected new virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BotSummary
    {
        public string Name { get; set; } = string.Empty;
        public string ElementalCore { get; set; } = string.Empty;
        public string BotClass { get; set; } = string.Empty;
        public int Level { get; set; }
        public int MaxHealth { get; set; }
        public int AttackPower { get; set; }
        public int Defense { get; set; }
        public string Rarity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double ExpProgress { get; set; }
        public int PowerRating { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public bool HasWeapon { get; set; }
        public bool HasArmor { get; set; }
        public bool HasAccessory { get; set; }
        public bool HasModule { get; set; }

        // UI Binding Properties
        public string ElementIcon => ElementalCore switch
        {
            "Fire" => "🔥",
            "Ice" => "❄️",
            "Electric" => "⚡",
            "Earth" => "🌍",
            "Wind" => "💨",
            "Water" => "💧",
            "Metal" => "⚙️",
            _ => "⚪"
        };

        public Color ElementColor => ElementalCore switch
        {
            "Fire" => Color.FromArgb("#FF4757"),
            "Ice" => Color.FromArgb("#4A9EFF"),
            "Electric" => Color.FromArgb("#FFD700"),
            "Earth" => Color.FromArgb("#8B4513"),
            "Wind" => Color.FromArgb("#B0B0B0"),
            "Water" => Color.FromArgb("#00CED1"),
            "Metal" => Color.FromArgb("#C0C0C0"),
            _ => Color.FromArgb("#6C757D")
        };

        public Color RarityColor => Rarity switch
        {
            "Common" => Color.FromArgb("#6C757D"),
            "Rare" => Color.FromArgb("#007BFF"),
            "Epic" => Color.FromArgb("#6F42C1"),
            "Legendary" => Color.FromArgb("#FFC107"),
            _ => Color.FromArgb("#6C757D")
        };

        public Color StatusColor => StatusText switch
        {
            "Ready for Battle" => Color.FromArgb("#28A745"),
            "Training" => Color.FromArgb("#FFC107"),
            "Injured" => Color.FromArgb("#DC3545"),
            "Resting" => Color.FromArgb("#17A2B8"),
            _ => Color.FromArgb("#6C757D")
        };

        public double HealthPercent => MaxHealth > 0 ? Math.Min(1.0, MaxHealth / 200.0) : 0;
        public double AttackPercent => AttackPower > 0 ? Math.Min(1.0, AttackPower / 50.0) : 0;
        public double DefensePercent => Defense > 0 ? Math.Min(1.0, Defense / 150.0) : 0;

        public Color WeaponSlotColor => HasWeapon ? Color.FromArgb("#FF4757") : Color.FromArgb("#2A2A3E");
        public Color ArmorSlotColor => HasArmor ? Color.FromArgb("#4A9EFF") : Color.FromArgb("#2A2A3E");
        public Color AccessorySlotColor => HasAccessory ? Color.FromArgb("#FFD700") : Color.FromArgb("#2A2A3E");
        public Color ModuleSlotColor => HasModule ? Color.FromArgb("#00FF88") : Color.FromArgb("#2A2A3E");
    }
}
