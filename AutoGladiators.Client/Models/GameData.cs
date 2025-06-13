using System.Collections.Generic;
using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.Models
{
    public class GameData
    {
        public PlayerProfile PlayerProfile { get; set; } = new PlayerProfile();
        public PlayerLocation PlayerLocation { get; set; } = new PlayerLocation();
        public List<GladiatorBot> OwnedBots { get; set; } = new List<GladiatorBot>();
        public List<Item> Inventory { get; set; } = new List<Item>();
        public List<string> CompletedQuests { get; set; } = new List<string>();
        public Dictionary<string, bool> EncounteredBots { get; set; } = new Dictionary<string, bool>();
        // BotRoster has been replaced by OwnedBots and EncounteredBots.
    }
}
