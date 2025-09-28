namespace AutoGladiators.Core.Models
{
    public class MapLocation
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Region { get; set; }
        public string Rarity { get; set; }
        public string SpritePath { get; set; }
        public List<string> ConnectedLocations { get; set; } = new();
        public string NPCId { get; set; }
        public string EncounterTableId { get; set; }
    }
}
