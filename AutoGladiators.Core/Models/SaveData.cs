namespace AutoGladiators.Core.Models
{
    /// <summary>
    /// Represents the save data for the game.
    /// </summary>
    public class SaveData
    {
        public Player Player { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }
}
