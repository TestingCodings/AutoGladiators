using AutoGladiators.Core.Models;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Services.Storage;
using Microsoft.Maui.Controls;
using AutoGladiators.Core.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

namespace AutoGladiators.Client.Pages
{
    public partial class BotDetailPage : ContentPage, INotifyPropertyChanged
    {
        private GladiatorBot _bot;
        private readonly DatabaseService? _databaseService;
        private int _currentExp = 850;
        private int _expToNext = 1000;
        private int _powerRating = 1850;

        public required GladiatorBot Bot 
        { 
            get => _bot; 
            set 
            { 
                _bot = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(BotLevel));
                OnPropertyChanged(nameof(HealthText));
                OnPropertyChanged(nameof(HealthPercent));
                OnPropertyChanged(nameof(AttackPercent));
                OnPropertyChanged(nameof(DefensePercent));
                OnPropertyChanged(nameof(SpeedPercent));
            } 
        }

        // Level and Experience Properties
        public string BotLevel => $"LV.{Bot.Level}";
        public string ExpText => $"{_currentExp}/{_expToNext} EXP";
        public double ExpProgress => (double)_currentExp / _expToNext;
        
        // Power Rating
        public int PowerRating
        {
            get => _powerRating;
            set { _powerRating = value; OnPropertyChanged(); }
        }

        // Rarity System
        public string BotRarity => DetermineBotRarity();
        
        // Health Display
        public string HealthText => $"{Bot.CurrentHealth}/{Bot.MaxHealth}";
        public double HealthPercent => Bot.MaxHealth > 0 ? Math.Min(1.0, (double)Bot.CurrentHealth / Bot.MaxHealth) : 0;
        
        // Stat Percentages (normalized to typical max values)
        public double AttackPercent => Math.Min(1.0, (double)Bot.AttackPower / 100);
        public double DefensePercent => Math.Min(1.0, (double)Bot.Defense / 100);
        public double SpeedPercent => Math.Min(1.0, (double)Bot.Speed / 100);

        // Status Information
        public string StatusEffectsText => Bot.ActiveStatusEffects?.Count > 0 ? 
            $"{string.Join(", ", Bot.ActiveStatusEffects.Select(e => e.Type))}" : 
            "No Active Effects";

        public string BattleStatusText => DetermineBattleStatus();
        public Color BattleStatusColor => GetBattleStatusColor();
        public string LastBattleText => "Last Battle: Victory vs Thunder Bot (2 min ago)";

        // Bot Moves
        public List<BotMove> BotMoves => GetBotMoves();

        // UI Colors
        public string ElementIcon => Bot.ElementalCore switch
        {
            AutoGladiators.Core.Enums.ElementalCore.Fire => "🔥",
            AutoGladiators.Core.Enums.ElementalCore.Ice => "❄️",
            AutoGladiators.Core.Enums.ElementalCore.Electric => "⚡",
            AutoGladiators.Core.Enums.ElementalCore.Earth => "🌍",
            AutoGladiators.Core.Enums.ElementalCore.Wind => "💨",
            AutoGladiators.Core.Enums.ElementalCore.Water => "💧",
            AutoGladiators.Core.Enums.ElementalCore.Metal => "⚙️",
            _ => "⚪"
        };

        public Color ElementColor => Bot.ElementalCore switch
        {
            AutoGladiators.Core.Enums.ElementalCore.Fire => Color.FromArgb("#FF4757"),
            AutoGladiators.Core.Enums.ElementalCore.Ice => Color.FromArgb("#4A9EFF"),
            AutoGladiators.Core.Enums.ElementalCore.Electric => Color.FromArgb("#FFD700"),
            AutoGladiators.Core.Enums.ElementalCore.Earth => Color.FromArgb("#8B4513"),
            AutoGladiators.Core.Enums.ElementalCore.Wind => Color.FromArgb("#B0B0B0"),
            AutoGladiators.Core.Enums.ElementalCore.Water => Color.FromArgb("#00CED1"),
            AutoGladiators.Core.Enums.ElementalCore.Metal => Color.FromArgb("#C0C0C0"),
            _ => Color.FromArgb("#6C757D")
        };

        public Color RarityColor => BotRarity switch
        {
            "Common" => Color.FromArgb("#6C757D"),
            "Rare" => Color.FromArgb("#007BFF"),
            "Epic" => Color.FromArgb("#6F42C1"),
            "Legendary" => Color.FromArgb("#FFC107"),
            _ => Color.FromArgb("#6C757D")
        };

        // Equipment Slot Colors
        public Color WeaponSlotColor => HasWeapon() ? Color.FromArgb("#FF4757") : Color.FromArgb("#2A2A3E");
        public Color ArmorSlotColor => HasArmor() ? Color.FromArgb("#4A9EFF") : Color.FromArgb("#2A2A3E");
        public Color AccessorySlotColor => HasAccessory() ? Color.FromArgb("#FFD700") : Color.FromArgb("#2A2A3E");
        public Color ModuleSlotColor => HasModule() ? Color.FromArgb("#00FF88") : Color.FromArgb("#2A2A3E");

        public BotDetailPage(GladiatorBot bot, IAppStorage storage)
        {
            InitializeComponent();
            Bot = bot;
            _databaseService = new DatabaseService(storage);
            CalculatePowerRating();
            BindingContext = this;
        }

        public BotDetailPage(GladiatorBot bot)
        {
            InitializeComponent();
            Bot = bot;
            _databaseService = null;
            CalculatePowerRating();
            BindingContext = this;
        }

        private string DetermineBotRarity()
        {
            var totalStats = Bot.AttackPower + Bot.Defense + Bot.Speed + Bot.MaxHealth + Bot.Energy;
            return totalStats switch
            {
                >= 400 => "Legendary",
                >= 300 => "Epic", 
                >= 200 => "Rare",
                _ => "Common"
            };
        }

        private void CalculatePowerRating()
        {
            PowerRating = (Bot.AttackPower * 2) + Bot.Defense + Bot.Speed + (Bot.MaxHealth / 2) + 
                         (Bot.Energy / 10) + (Bot.Level * 50);
        }

        private string DetermineBattleStatus()
        {
            var healthPercent = (double)Bot.CurrentHealth / Bot.MaxHealth;
            return healthPercent switch
            {
                >= 0.8 => "Ready for Battle",
                >= 0.5 => "Combat Ready",
                >= 0.2 => "Needs Rest",
                _ => "Critically Injured"
            };
        }

        private Color GetBattleStatusColor()
        {
            var healthPercent = (double)Bot.CurrentHealth / Bot.MaxHealth;
            return healthPercent switch
            {
                >= 0.8 => Color.FromArgb("#28A745"),
                >= 0.5 => Color.FromArgb("#FFC107"),
                >= 0.2 => Color.FromArgb("#FF8C00"),
                _ => Color.FromArgb("#DC3545")
            };
        }

        private List<BotMove> GetBotMoves()
        {
            // Generate sample moves based on element and level
            var moves = new List<BotMove>();
            
            switch (Bot.ElementalCore)
            {
                case AutoGladiators.Core.Enums.ElementalCore.Fire:
                    moves.Add(new BotMove { Name = "Flame Strike", Icon = "🔥", Power = "85" });
                    moves.Add(new BotMove { Name = "Burning Aura", Icon = "💥", Power = "60" });
                    if (Bot.Level >= 5) moves.Add(new BotMove { Name = "Inferno Blast", Icon = "🌋", Power = "120" });
                    break;
                case AutoGladiators.Core.Enums.ElementalCore.Ice:
                    moves.Add(new BotMove { Name = "Frost Bolt", Icon = "❄️", Power = "70" });
                    moves.Add(new BotMove { Name = "Ice Shield", Icon = "🛡️", Power = "50" });
                    if (Bot.Level >= 5) moves.Add(new BotMove { Name = "Blizzard", Icon = "🌨️", Power = "110" });
                    break;
                case AutoGladiators.Core.Enums.ElementalCore.Electric:
                    moves.Add(new BotMove { Name = "Thunder Bolt", Icon = "⚡", Power = "90" });
                    moves.Add(new BotMove { Name = "Static Field", Icon = "💫", Power = "65" });
                    if (Bot.Level >= 5) moves.Add(new BotMove { Name = "Lightning Storm", Icon = "🌩️", Power = "125" });
                    break;
                default:
                    moves.Add(new BotMove { Name = "Basic Attack", Icon = "👊", Power = "50" });
                    moves.Add(new BotMove { Name = "Defense Mode", Icon = "🛡️", Power = "40" });
                    break;
            }

            return moves;
        }

        private bool HasWeapon() => Bot.Level >= 2; // Sample logic
        private bool HasArmor() => Bot.Level >= 3;
        private bool HasAccessory() => Bot.Level >= 5;
        private bool HasModule() => Bot.Level >= 4;

        private async void OnLevelUpClicked(object sender, EventArgs e)
        {
            if (_currentExp >= _expToNext)
            {
                Bot.Level++;
                _currentExp = 0;
                _expToNext = Bot.Level * 100;
                
                // Increase stats on level up
                Bot.MaxHealth += 20;
                Bot.CurrentHealth = Bot.MaxHealth;
                Bot.AttackPower += 5;
                Bot.Defense += 3;
                Bot.Speed += 2;
                
                CalculatePowerRating();
                
                await DisplayAlert("Level Up!", $"{Bot.Name} reached Level {Bot.Level}!\n+20 HP, +5 ATK, +3 DEF, +2 SPD", "Awesome!");
                
                OnPropertyChanged(nameof(BotLevel));
                OnPropertyChanged(nameof(ExpText));
                OnPropertyChanged(nameof(ExpProgress));
                OnPropertyChanged(nameof(HealthText));
                OnPropertyChanged(nameof(BotMoves));
            }
            else
            {
                await DisplayAlert("Not Ready", $"Need {_expToNext - _currentExp} more EXP to level up.", "OK");
            }
        }

        private async void OnBattleTestClicked(object sender, EventArgs e)
        {
            // Create a test enemy for battle
            var enemyBot = new GladiatorBot
            {
                Name = "Training Dummy",
                ElementalCore = AutoGladiators.Core.Enums.ElementalCore.Metal,
                Description = "A practice bot for testing",
                MaxHealth = Math.Max(80, Bot.MaxHealth - 20),
                CurrentHealth = Math.Max(80, Bot.MaxHealth - 20),
                AttackPower = Math.Max(10, Bot.AttackPower - 10),
                Defense = Math.Max(5, Bot.Defense - 5),
                Speed = Math.Max(5, Bot.Speed - 5),
                Energy = 50,
                Endurance = 30,
                Luck = 5,
                Level = Math.Max(1, Bot.Level - 1)
            };

            await Navigation.PushAsync(new BattlePage(Bot, enemyBot));
        }

        private async void OnCustomizeClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Customization", $"Bot customization options for {Bot.Name} coming soon!", "OK");
        }

        private async void OnEquipmentSlotClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string slotType)
            {
                await DisplayAlert("Equipment", $"{slotType} equipment management coming soon!", "OK");
            }
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
        protected new virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BotMove
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Power { get; set; }
    }
}
