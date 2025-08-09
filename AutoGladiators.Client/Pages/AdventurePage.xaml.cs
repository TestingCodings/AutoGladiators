using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.Pages
{
    public partial class AdventurePage : ContentPage
    {
        public class AdventureLocation
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }

        public AdventurePage()
        {
            InitializeComponent();

            var locations = new List<AdventureLocation>
            {
                new AdventureLocation { Name = "Scrap Canyon", Description = "An abandoned junkyard full of hostile bots." },
                new AdventureLocation { Name = "Techspire City", Description = "Meet NPCs, shop, and upgrade bots." },
                new AdventureLocation { Name = "Core Mine", Description = "A dangerous area rich in elemental chips." },
                new AdventureLocation { Name = "Arena Gateway", Description = "Enter battle simulations for EXP and loot." },
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
                        // TODO: Replace 'playerBot' and 'enemyBot' with actual GladiatorBot instances
                        GladiatorBot playerBot = null;/* get or create player GladiatorBot */
                        GladiatorBot enemyBot = null; // TODO: get or create enemy GladiatorBot
                        await Navigation.PushAsync(new BattlePage(playerBot, enemyBot));
                        break;
                    case "Techspire City":
                        // When navigating to NPCDialoguePage, pass the required npcId argument:
                        string npcId = "1"; // TODO: Get the actual NPC ID based on the selected location or other logic
                        await Navigation.PushAsync(new NPCDialoguePage(npcId));
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
