using System;
using System.Collections.Generic;
using SQLite;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Models;

namespace AutoGladiators.Core.Models
{
    [Table("PlayerProfiles")]
    public class PlayerProfile
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public string playerName { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public string StartingRegion { get; set; } = "TestZone";

        // Reference to PlayerLocation (stored as JSON for SQLite compatibility)
        [Ignore]
        public PlayerLocation? CurrentLocation { get; set; } = new PlayerLocation();
        
        // JSON representation for database storage
        public string CurrentLocationData { get; set; } = "{}";

        // Story flags for game progress tracking
        [Ignore]
        public Dictionary<string, bool>? StoryFlags { get; set; } = new Dictionary<string, bool>();

        // Optional: future extensions (stored as JSON for SQLite)
        public string CompletedQuestsData { get; set; } = "[]";
        public string StoryFlagsData { get; set; } = "{}";

        // Properties for tests
        public string Name { get; set; } = string.Empty;
        public int Gold { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastPlayed { get; set; } = DateTime.UtcNow;
    }
}
