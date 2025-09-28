using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System;

namespace AutoGladiators.Client.Pages
{
    public partial class BotRosterPage : ContentPage
    {
        public class BotSummary
        {
            public required string Name { get; set; }
            public required string ElementalCore { get; set; }
            public required string Description { get; set; }
            public string Level { get; set; } = "1";
            public string Health { get; set; } = "100/100";
            public string AttackPower { get; set; } = "20";
            public string BotClass { get; set; } = "Balanced";
            public string Rarity { get; set; } = "Common";
        }

        public BotRosterPage()
        {
            InitializeComponent();

            BotList.ItemsSource = new List<BotSummary>
            {
                new BotSummary 
                { 
                    Name = "AegisCore", 
                    ElementalCore = "Metal", 
                    Description = "Heavy armored defender bot with superior defensive capabilities.", 
                    Level = "5",
                    Health = "150/150",
                    AttackPower = "18",
                    BotClass = "Tank",
                    Rarity = "Rare"
                },
                new BotSummary 
                { 
                    Name = "FlareByte", 
                    ElementalCore = "Fire", 
                    Description = "Fast and aggressive striker bot with burning attacks.", 
                    Level = "3",
                    Health = "85/85",
                    AttackPower = "28",
                    BotClass = "Brawler",
                    Rarity = "Uncommon"
                },
                new BotSummary 
                { 
                    Name = "VoltZerker", 
                    ElementalCore = "Electric", 
                    Description = "Precision ranged attacker with shocking speed.", 
                    Level = "4",
                    Health = "95/95",
                    AttackPower = "25",
                    BotClass = "Sniper",
                    Rarity = "Rare"
                },
                new BotSummary 
                { 
                    Name = "CrystalGuard", 
                    ElementalCore = "Ice", 
                    Description = "Defensive ice bot with freezing abilities.", 
                    Level = "2",
                    Health = "110/110",
                    AttackPower = "16",
                    BotClass = "Support",
                    Rarity = "Common"
                },
                new BotSummary 
                { 
                    Name = "StormBreaker", 
                    ElementalCore = "Wind", 
                    Description = "Lightning-fast wind element assassin bot.", 
                    Level = "6",
                    Health = "75/75",
                    AttackPower = "32",
                    BotClass = "Assassin",
                    Rarity = "Epic"
                }
            };
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
                BotList.SelectedItem = null;
            }
        }
    }
}
