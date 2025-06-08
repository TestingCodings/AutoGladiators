namespace AutoGladiators.Client.Models
{
    public class PlayerProfile
    {
        public string PlayerName { get; set; }
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public string StartingRegion { get; set; } = "TestZone";
    }
}
