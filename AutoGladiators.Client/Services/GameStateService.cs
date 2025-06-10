using AutoGladiators.Client.Models;
using System.Collections.Generic;
using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.Services
{
    public class GameStateService
    {
        public PlayerProfile CurrentPlayer { get; private set; }
        public List<GladiatorBot> BotRoster { get; private set; } = new();
        public Inventory Inventory { get; private set; } = new();
        public PlayerLocation CurrentLocation { get; private set; } = new();

        public void InitializeNewGame(string playerName)
        {
            CurrentPlayer = new PlayerProfile { playerName = playerName, Level = 1 };
            BotRoster.Clear();
            Inventory.Clear();
            CurrentLocation = new PlayerLocation { Region = "StarterZone", X = 0, Y = 0 };
        }

        public void LoadGame(GameData data)
        {
            CurrentPlayer = data.PlayerProfile;
            BotRoster = new List<GladiatorBot>(data.OwnedBots);
            Inventory.Items = new List<Item>(data.Inventory);
            CurrentLocation = data.PlayerLocation;
        }

        public GameData SaveGame()
        {
            return new GameData
            {
                PlayerProfile = CurrentPlayer,
                OwnedBots = new List<GladiatorBot>(BotRoster),
                Inventory = Inventory.Items,
                PlayerLocation = CurrentLocation
            };
        }

        public void AddBotToRoster(GladiatorBot bot)
        {
            BotRoster.Add(bot);
        }

        public void UpdateLocation(PlayerLocation location)
        {
            CurrentLocation = location;
        }
    }
}
