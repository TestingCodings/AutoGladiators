using AutoGladiators.Client.Models;
using AutoGladiators.Client.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Client.Services
{
    public static class MapService
    {
        private static readonly Microsoft.Extensions.Logging.ILogger Log = (Microsoft.Extensions.Logging.ILogger)AppLog.For("MapService");
        public static Dictionary<string, MapLocation> AllLocations = new()
        {
            ["home_base"] = new MapLocation
            {
                Id = "home_base",
                Name = "Home Base",
                Description = "Your central hub.",
                Region = "Sanctuary",
                Rarity = "None",
                SpritePath = "home_base.png",
                ConnectedLocations = new() { "training_field", "scrapyard" },
                NPCId = "zara_mechanic"
            },
            ["training_field"] = new MapLocation
            {
                Id = "training_field",
                Name = "Training Field",
                Description = "Practice and prepare your bots.",
                Region = "Grasslands",
                Rarity = "Common",
                SpritePath = "training_field.png",
                ConnectedLocations = new() { "home_base" }
            },
            ["scrapyard"] = new MapLocation
            {
                Id = "scrapyard",
                Name = "Old Scrapyard",
                Description = "A dangerous place filled with wild bots.",
                Region = "Wastelands",
                Rarity = "Uncommon",
                SpritePath = "scrapyard.png",
                ConnectedLocations = new() { "home_base" },
                EncounterTableId = "wild_scrapyard"
            }
        };

        public static MapLocation Get(string id) => AllLocations.GetValueOrDefault(id);
    }
}

