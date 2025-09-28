using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Logic;


namespace AutoGladiators.Client.Pages
{
    public partial class AdventurePage : ContentPage
    {
        public class AdventureLocation
        {
            public required string Name { get; set; }
            public required string Description { get; set; }
            public string Region { get; set; } = "Unknown";
            public string Rarity { get; set; } = "Common";
        }

        public AdventurePage()
        {
            InitializeComponent();

            var locations = new List<AdventureLocation>
            {
                new AdventureLocation { Name = "Scrap Canyon", Description = "An abandoned junkyard full of hostile bots.", Region = "Wasteland", Rarity = "Common" },
                new AdventureLocation { Name = "Techspire City", Description = "Meet NPCs, shop, and upgrade bots.", Region = "Urban", Rarity = "Safe" },
                new AdventureLocation { Name = "Core Mine", Description = "A dangerous area rich in elemental chips.", Region = "Underground", Rarity = "Rare" },
                new AdventureLocation { Name = "Arena Gateway", Description = "Enter battle simulations for EXP and loot.", Region = "Arena District", Rarity = "Epic" },
                new AdventureLocation { Name = "Plasma Foundry", Description = "Advanced manufacturing facility with plasma bots.", Region = "Industrial", Rarity = "Legendary" },
                new AdventureLocation { Name = "Frozen Peaks", Description = "Icy mountains inhabited by ice-element bots.", Region = "Arctic", Rarity = "Rare" }
            };

            LocationList.ItemsSource = locations;
        }

        private async void OnLocationSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is AdventureLocation location)
            {
                await DisplayAlert("Traveling to:", location.Name, "Continue");

                switch (location.Name)
                {
                    case "Scrap Canyon":
                        // Create default player and enemy bots for battle
                        var playerBot = new GladiatorBot
                        {
                            Name = "Player Bot",
                            ElementalCore = AutoGladiators.Core.Enums.ElementalCore.Metal,
                            Description = "Your trusty battle bot",
                            MaxHealth = 100,
                            CurrentHealth = 100,
                            AttackPower = 25,
                            Defense = 20,
                            Speed = 15,
                            Energy = 100,
                            Endurance = 50,
                            Luck = 10
                        };
                        var enemyBot = new GladiatorBot
                        {
                            Name = "Scrap Warrior",
                            ElementalCore = AutoGladiators.Core.Enums.ElementalCore.Fire,
                            Description = "A hostile bot from the junkyard",
                            MaxHealth = 85,
                            CurrentHealth = 85,
                            AttackPower = 22,
                            Defense = 18,
                            Speed = 20,
                            Energy = 80,
                            Endurance = 40,
                            Luck = 8
                        };
                        await Navigation.PushAsync(new BattlePage(playerBot, enemyBot));
                        break;
                    case "Techspire City":
                        string npcId = "1"; // Shop keeper or city guide NPC
                        await Navigation.PushAsync(new NPCDialoguePage(npcId));
                        break;
                    case "Plasma Foundry":
                        var plasmaPlayerBot = new GladiatorBot
                        {
                            Name = "Player Bot",
                            ElementalCore = AutoGladiators.Core.Enums.ElementalCore.Metal,
                            Description = "Your trusty battle bot",
                            MaxHealth = 100,
                            CurrentHealth = 100,
                            AttackPower = 25,
                            Defense = 20,
                            Speed = 15
                        };
                        var plasmaBoss = new GladiatorBot
                        {
                            Name = "Plasma Forge",
                            ElementalCore = AutoGladiators.Core.Enums.ElementalCore.Plasma,
                            Description = "A powerful plasma-enhanced manufacturing bot",
                            MaxHealth = 150,
                            CurrentHealth = 150,
                            AttackPower = 35,
                            Defense = 25,
                            Speed = 12
                        };
                        await Navigation.PushAsync(new BattlePage(plasmaPlayerBot, plasmaBoss));
                        break;
                    case "Frozen Peaks":
                        var icePlayerBot = new GladiatorBot
                        {
                            Name = "Player Bot",
                            ElementalCore = AutoGladiators.Core.Enums.ElementalCore.Metal,
                            Description = "Your trusty battle bot",
                            MaxHealth = 100,
                            CurrentHealth = 100,
                            AttackPower = 25,
                            Defense = 20,
                            Speed = 15
                        };
                        var iceBot = new GladiatorBot
                        {
                            Name = "Frost Guardian",
                            ElementalCore = AutoGladiators.Core.Enums.ElementalCore.Ice,
                            Description = "An ancient ice-element guardian of the peaks",
                            MaxHealth = 120,
                            CurrentHealth = 120,
                            AttackPower = 28,
                            Defense = 30,
                            Speed = 8
                        };
                        await Navigation.PushAsync(new BattlePage(icePlayerBot, iceBot));
                        break;
                    default:
                        await DisplayAlert("Coming Soon", $"{location.Name} is under development.", "OK");
                        break;
                }

                LocationList.SelectedItem = null; // Deselect
            }
        }
    }
}
