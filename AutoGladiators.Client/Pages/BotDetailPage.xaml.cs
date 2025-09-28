using AutoGladiators.Core.Models;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Services.Storage;
using Microsoft.Maui.Controls;
using AutoGladiators.Core.Core;
using System;
using System.Linq;

namespace AutoGladiators.Client.Pages
{
    public partial class BotDetailPage : ContentPage
    {
        public required GladiatorBot Bot { get; set; }
        public string BotStats => $"Health: {Bot.CurrentHealth}/{Bot.MaxHealth}\nEnergy: {Bot.Energy}\nEndurance: {Bot.Endurance}\nLuck: {Bot.Luck}\nElement: {Bot.ElementalCore}\nAttack: {Bot.AttackPower}\nDefense: {Bot.Defense}\nSpeed: {Bot.Speed}\nCrit Chance: {Bot.CriticalHitChance:P1}";

        public string BotLevel => $"Level {Bot.Level}";
        public string StatusEffectsText => Bot.ActiveStatusEffects?.Count > 0 ? 
            $"Active Effects: {string.Join(", ", Bot.ActiveStatusEffects.Select(e => e.Type))}" : 
            "No Active Effects";
        public string MovesetText => Bot.Moveset?.Count > 0 ? 
            $"Known Moves: {string.Join(", ", Bot.Moveset)}" : 
            "No Moves Learned";

        private readonly DatabaseService? _databaseService;

        public BotDetailPage(GladiatorBot bot, IAppStorage storage)
        {
            InitializeComponent();
            Bot = bot;
            _databaseService = new DatabaseService(storage);
            BindingContext = this;
        }

        public BotDetailPage(GladiatorBot bot)
        {
            InitializeComponent();
            Bot = bot;
            _databaseService = null;
            BindingContext = this;
        }

        private async void OnAddToInventoryClicked(object sender, EventArgs e)
        {
            if (_databaseService != null)
            {
                await _databaseService.AddBotAsync(Bot);
                await DisplayAlert("Success", $"{Bot.Name} added to inventory.", "OK");
            }
            else
            {
                await DisplayAlert("Info", $"{Bot.Name} details viewed (no storage available).", "OK");
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
                MaxHealth = 100,
                CurrentHealth = 100,
                AttackPower = 15,
                Defense = 10,
                Speed = 10,
                Energy = 50,
                Endurance = 30,
                Luck = 5
            };

            await Navigation.PushAsync(new BattlePage(Bot, enemyBot));
        }
    }
}
