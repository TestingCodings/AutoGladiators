using AutoGladiators.Client.Models;
using AutoGladiators.Client.Services;
using Microsoft.Maui.Controls;
using AutoGladiators.Client.Core;
using System;

namespace AutoGladiators.Client.Pages
{
    public partial class BotDetailPage : ContentPage
    {
        public GladiatorBot Bot { get; set; }
        public string BotStats => $"Health: {Bot.Health}\nEnergy: {Bot.Energy}\nEndurance: {Bot.Endurance}\nLuck: {Bot.Luck}\nElement: {Bot.ElementalCore}\nCrit Chance: {Bot.CriticalHitChance}";

        public BotDetailPage(GladiatorBot bot)
        {
            InitializeComponent();
            Bot = bot;
            BindingContext = this;
        }

        private async void OnAddToInventoryClicked(object sender, EventArgs e)
        {
            await DatabaseService.AddBotAsync(Bot);
            await DisplayAlert("Success", $"{Bot.Name} added to inventory.", "OK");
        }
    }
}
