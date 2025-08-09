using AutoGladiators.Client.Core;
using AutoGladiators.Client.Models;

namespace AutoGladiators.Client.Models
{
    public class PlayerProfile
    {
        public string playerName { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public string StartingRegion { get; set; } = "TestZone";

        // Reference to PlayerLocation (now defined in PlayerLocation.cs)
        public PlayerLocation CurrentLocation { get; set; } = new PlayerLocation();

        // Optional: future extensions
        public List<string> CompletedQuests { get; set; } = new();
        public Dictionary<string, bool> StoryFlags { get; set; } = new();

        // New properties
        public string Name { get; set; } = string.Empty;
        public int Gold { get; set; }
    }
}
