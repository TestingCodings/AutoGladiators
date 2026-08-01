using AutoGladiators.Core.Models;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Core.Services
{
    public class EncounterGenerator
    {
        private static readonly IAppLogger Log = AppLog.For<EncounterGenerator>();
        private readonly Random _rng = new();

        /// <summary>
        /// Generate wild encounter using new rarity-driven bot system
        /// </summary>
        public GladiatorBot? GenerateWildEncounter(string location)
        {
            var playerLevel = GameStateService.Instance.CurrentPlayer?.Level ?? 1;
            
            // Map location names to zone IDs for the new system
            var zoneId = MapLocationToZone(location);
            
            // Use new BotFactory with proper species and rarity system
            var enemy = BotFactory.GenerateWildBot(zoneId, playerLevel);
            
            if (enemy != null)
            {
                Log.Info($"Generated wild encounter: {enemy.Name} (Lv{enemy.Level}, {enemy.Rarity}) in {location}");
            }
            else
            {
                Log.Warn($"Failed to generate encounter for location: {location}");
            }
            
            return enemy;
        }
        
        /// <summary>
        /// Map legacy location names to new zone system
        /// </summary>
        private string MapLocationToZone(string location) => location?.ToLower() switch
        {
            "wilds" or "scrapyards" or "scrap yards" or "rusty outskirts" => "route_1",
            "electricwastes" or "electric wastes" or "static fields" => "forest_path", 
            "volcanicdepths" or "volcanic depths" or "lava pools" => "mountain_trail",
            "crystalcaverns" or "crystal caverns" or "crystal depths" => "final_dungeon",
            "quickbattlearena" or "arena" => "route_1", // Quick battle fallback
            _ => "route_1" // Default to early area
        };
    }
}

