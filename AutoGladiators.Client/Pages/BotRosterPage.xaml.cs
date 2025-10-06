using Microsoft.Maui.Controls;
using AutoGladiators.Core.Models;
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
        private readonly SpriteManager _spriteManager;
        private readonly AnimationManager _animationManager;
        private ObservableCollection<BotSummary> _allBots;
        private ObservableCollection<BotSummary> _filteredBots;
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
        public BotRosterPage() : this(null, null)
        {
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
            _allBots = new ObservableCollection<BotSummary>
            {
                new BotSummary
                {
                    Name = "AegisCore",
                    ElementalCore = "Metal",
                    BotClass = "Tank",
                    Level = 5,
                    MaxHealth = 150,
                    AttackPower = 18,
                    Defense = 95,
                    Rarity = "Rare",
                    Description = "Heavy armored defender bot with superior defensive capabilities.",
                    ExpProgress = 0.4,
                    PowerRating = 1250,
                    StatusText = "Ready for Battle",
                    HasWeapon = true,
                    HasArmor = true,
                    HasAccessory = false,
                    HasModule = true
                },
                new BotSummary
                {
                    Name = "FlareByte",
                    ElementalCore = "Fire",
                    BotClass = "Brawler",
                    Level = 3,
                    MaxHealth = 85,
                    AttackPower = 28,
                    Defense = 45,
                    Rarity = "Common",
                    Description = "Fast and aggressive striker bot with burning attacks.",
                    ExpProgress = 0.8,
                    PowerRating = 890,
                    StatusText = "Training",
                    HasWeapon = true,
                    HasArmor = false,
                    HasAccessory = true,
                    HasModule = false
                },
                new BotSummary
                {
                    Name = "VoltZerker",
                    ElementalCore = "Electric",
                    BotClass = "Sniper",
                    Level = 4,
                    MaxHealth = 95,
                    AttackPower = 25,
                    Defense = 55,
                    Rarity = "Rare",
                    Description = "Precision ranged attacker with shocking speed.",
                    ExpProgress = 0.2,
                    PowerRating = 1150,
                    StatusText = "Ready for Battle",
                    HasWeapon = true,
                    HasArmor = true,
                    HasAccessory = false,
                    HasModule = true
                },
                new BotSummary
                {
                    Name = "CrystalGuard",
                    ElementalCore = "Ice",
                    BotClass = "Support",
                    Level = 2,
                    MaxHealth = 110,
                    AttackPower = 16,
                    Defense = 70,
                    Rarity = "Common",
                    Description = "Defensive ice bot with freezing abilities.",
                    ExpProgress = 0.6,
                    PowerRating = 750,
                    StatusText = "Resting",
                    HasWeapon = false,
                    HasArmor = true,
                    HasAccessory = false,
                    HasModule = false
                },
                new BotSummary
                {
                    Name = "StormBreaker",
                    ElementalCore = "Wind",
                    BotClass = "Assassin",
                    Level = 6,
                    MaxHealth = 75,
                    AttackPower = 32,
                    Defense = 40,
                    Rarity = "Epic",
                    Description = "Lightning-fast wind element assassin bot.",
                    ExpProgress = 0.1,
                    PowerRating = 1480,
                    StatusText = "Ready for Battle",
                    HasWeapon = true,
                    HasArmor = false,
                    HasAccessory = true,
                    HasModule = true
                }
            };

            FilteredBots = new ObservableCollection<BotSummary>(_allBots);
            UpdateCollectionStats();
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
        public string Name { get; set; }
        public string ElementalCore { get; set; }
        public string BotClass { get; set; }
        public int Level { get; set; }
        public int MaxHealth { get; set; }
        public int AttackPower { get; set; }
        public int Defense { get; set; }
        public string Rarity { get; set; }
        public string Description { get; set; }
        public double ExpProgress { get; set; }
        public int PowerRating { get; set; }
        public string StatusText { get; set; }
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
