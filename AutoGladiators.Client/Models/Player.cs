using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.Models
{
    /// <summary>
    /// Represents a player in the game.
    /// </summary>
    public class Player
    {
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
        public string PlayerName { get; set; } = "New Player";
        public string PlayerId { get; set; } = Guid.NewGuid().ToString(); // Unique ID for the player
        public int Experience { get; set; } = 0;
        public int Gold { get; set; } = 0;
        public int Gems { get; set; } = 0;
        public GladiatorBot StarterBot { get; set; } = null!;
        public List<GladiatorBot> Roster { get; set; } = new();
        public List<Item> Inventory { get; set; } = new();
        public PlayerLocation CurrentLocation { get; set; } = null!;
        public Dictionary<string, bool> StoryFlags { get; set; } = new();
    }
}
