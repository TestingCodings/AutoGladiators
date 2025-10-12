namespace AutoGladiators.Core.Models
{
    public class MapLocation
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;
        public string SpritePath { get; set; } = string.Empty;
        public List<string> ConnectedLocations { get; set; } = new();
        public string NPCId { get; set; } = string.Empty;
        public string EncounterTableId { get; set; } = string.Empty;
    }
}
