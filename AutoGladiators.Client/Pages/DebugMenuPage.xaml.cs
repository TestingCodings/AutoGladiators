using AutoGladiators.Core.Models;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.StateMachine.States;


namespace AutoGladiators.Client.Pages
{
    public partial class DebugMenuPage : ContentPage
    {
        public DebugMenuPage()
        {
            InitializeComponent();
        }

        private void OnAddBotClicked(object sender, EventArgs e)
        {
            var bot = BotFactory.CreateBot("RustyCharger", 5);
            // var playerRoster = GameStateService.Instance.PlayerRoster; // or your actual player roster
            // playerRoster.Add(bot);
            DisplayAlert("Success", $"Added {bot.Name} to roster.", "OK");
        }

        private void OnGiveChipsClicked(object sender, EventArgs e)
        {
            GameStateService.Instance.Inventory.AddItem(new AutoGladiators.Core.Models.Item
            {
                Name = "ControlChip",
                Description = "A chip used to control bots.",
                Type = AutoGladiators.Core.Enums.ItemType.Capture.ToString(), // Replace with the correct enum/type if different
                Effect = "Controls a wild bot" // Replace with the actual effect logic or object if needed
            }); // Replace with actual item and count
            DisplayAlert("Inventory", "Added 5 ControlChips.", "OK");
        }

        private async void OnTestBattleClicked(object sender, EventArgs e)
        {
            var encounterGenerator = new AutoGladiators.Core.Services.EncounterGenerator();
            var wildBot = encounterGenerator.GenerateWildEncounter("DebugZone");
            if (wildBot == null)
            {
                await DisplayAlert("Error", "Failed to generate wild bot.", "OK");
                return;
            }

            // Get the player's current bot
            var playerBot = GameStateService.Instance.GetCurrentBot();
            if (playerBot == null)
            {
                await DisplayAlert("Error", "No active bot to battle with.", "OK");
                return;
            }

            // Create battle setup data
            var setup = new BattleSetup(playerBot, wildBot, PlayerInitiated: true);

            // Tell the new GameLoop to transition into the BattlingState
            await GameLoop.GoToAsync(GameStateId.Battling, new StateArgs
            {
                Reason = "DebugMenuTestBattle",
                Payload = setup
            });

            // Optionally push the battle page UI if you still want that manual control
            await Navigation.PushAsync(new BattlePage(playerBot, wildBot));
        }

        private void OnSetFlagClicked(object sender, EventArgs e)
        {
            // var questFlags = GameStateService.Instance.QuestFlags; // Replace with your actual quest flags dictionary
            // questFlags["IntroQuestStarted"] = true;
            DisplayAlert("Flags", "IntroQuestStarted = true", "OK");
        }

        private async void OnShowFlagsClicked(object sender, EventArgs e)
        {
            // var questFlags = GameStateService.Instance.QuestFlags; // Replace with your actual quest flags dictionary
            // var log = questFlags.Any()
            //     ? string.Join("\n", questFlags.Select(kvp => $"{kvp.Key}: {kvp.Value}"))
            //     : "No flags set.";

            await DisplayAlert("Quest Flags", "No flags set.", "OK");
        }

        private void OnWarpClicked(object sender, EventArgs e)
        {
            var region = LocationEntry.Text?.Trim();
            if (!string.IsNullOrEmpty(region) && MapService.Get(region) is MapLocation dest)
            {
                // var currentLocationId = GameStateService.Instance.GetPlayerLocationId(); // Replace with actual method/property
                // currentLocationId = dest.Id;
                DisplayAlert("Warped", $"Now at {dest.Name}.", "OK");
            }
            else
            {
                DisplayAlert("Error", "Invalid location ID.", "OK");
            }
        }
    }
}
