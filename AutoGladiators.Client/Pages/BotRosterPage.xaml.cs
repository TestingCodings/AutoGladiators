using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System;

namespace AutoGladiators.Client.Pages
{
    public partial class BotRosterPage : ContentPage
    {
        public class BotSummary
        {
            public string Name { get; set; }
            public string ElementalCore { get; set; }
            public string Description { get; set; }
        }

        public BotRosterPage()
        {
            InitializeComponent();

            BotList.ItemsSource = new List<BotSummary>
            {
                new BotSummary { Name = "AegisCore", ElementalCore = "Steel", Description = "Heavy armored defender bot." },
                new BotSummary { Name = "FlareByte", ElementalCore = "Fire", Description = "Fast and aggressive striker bot." },
                new BotSummary { Name = "VoltZerker", ElementalCore = "Electric", Description = "Precision ranged attacker." }
            };
        }

        private async void OnBotSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is BotSummary selectedBot)
            {
                await DisplayAlert("Bot Selected", $"{selectedBot.Name} - {selectedBot.ElementalCore}", "OK");
                await Navigation.PushAsync(new BotDetailPage(selectedBot));
                BotList.SelectedItem = null;
            }
        }
    }
}
