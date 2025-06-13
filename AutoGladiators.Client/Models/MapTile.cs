
namespace AutoGladiators.Client.Models
{
    public class MapTile
    {
        public string Region { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string TileType { get; set; } // e.g., Grass, Road, NPC, EncounterZone
        public bool IsEncounterZone { get; set; }
        public string NPCId { get; set; } // Optional: ID of NPC if applicable
    }
}
