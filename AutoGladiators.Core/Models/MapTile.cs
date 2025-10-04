using AutoGladiators.Core.Enums;

namespace AutoGladiators.Core.Models
{
    public class MapTile
    {
        public int X { get; set; }
        public int Y { get; set; }
        public TerrainType Terrain { get; set; }
        public string Icon { get; set; } = "🌍";
        public string Name { get; set; } = "Unknown";
        public string Description { get; set; } = "An unexplored area";
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Easy;
        public bool IsExplored { get; set; } = false;
        public bool HasEncounter { get; set; } = false;
        
        // Color for the tile background
        public string BackgroundColor => Terrain switch
        {
            TerrainType.ScrapYards => "#2A4A2A",      // Dark green
            TerrainType.ElectricWastes => "#4A4A2A",  // Dark yellow
            TerrainType.VolcanicDepths => "#4A2A2A",  // Dark red
            TerrainType.CrystalCaverns => "#3A2A4A",  // Dark purple
            TerrainType.StartingArea => "#2A3A4A",    // Dark blue
            _ => "#2A2A2A"                            // Dark gray
        };
        
        // Border color for current/visited states
        public string BorderColor => IsExplored ? "#FFD700" : "#666666";
    }

    public enum TerrainType
    {
        StartingArea,
        ScrapYards,
        ElectricWastes,
        VolcanicDepths,
        CrystalCaverns,
        Empty
    }
    
    public enum DifficultyLevel
    {
        Safe,     // Starting area
        Easy,     // 1 star
        Medium,   // 2 stars
        Hard,     // 3 stars
        Extreme   // 4 stars
    }
}