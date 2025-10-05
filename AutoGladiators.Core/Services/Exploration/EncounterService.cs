using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Models.Exploration;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Rng;

namespace AutoGladiators.Core.Services.Exploration
{
    /// <summary>
    /// Manages wild gladiator encounters and trainer battles
    /// </summary>
    public class EncounterService
    {
        private readonly IRng _rng;
        private readonly WorldManager _worldManager;
        
        // Event for when a wild encounter is triggered
        public event Action<PlayerProfile, GladiatorBot, string>? OnWildEncounter;
        
        // Event for when a trainer battle is initiated
        public event Action<PlayerProfile, WorldObject>? OnTrainerBattle;
        
        public EncounterService(IRng rng, WorldManager worldManager)
        {
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
            _worldManager = worldManager ?? throw new ArgumentNullException(nameof(worldManager));
        }
        
        /// <summary>
        /// Attempts to trigger a random wild encounter based on current tile
        /// </summary>
        public async Task<bool> CheckForWildEncounterAsync(PlayerProfile player)
        {
            var zone = _worldManager.GetZone(player.WorldPosition.ZoneId);
            if (zone == null) return false;
            
            var tile = zone.GetTile(player.WorldPosition.X, player.WorldPosition.Y);
            if (tile == null || !tile.HasEncounters || tile.AvailableGladiators.Count == 0)
                return false;
            
            // Check encounter rate
            var encounterRoll = _rng.NextDouble();
            if (encounterRoll > tile.EncounterRate)
                return false;
            
            // Select a random gladiator from available encounters
            var gladiatorId = tile.AvailableGladiators[_rng.Next(tile.AvailableGladiators.Count)];
            var wildGladiator = await GenerateWildGladiator(gladiatorId, GetEncounterLevel(player));
            
            if (wildGladiator == null) return false;
            
            // Trigger the encounter
            OnWildEncounter?.Invoke(player, wildGladiator, zone.Name);
            return true;
        }
        
        /// <summary>
        /// Generates a wild gladiator for encounters
        /// </summary>
        public async Task<GladiatorBot?> GenerateWildGladiator(string gladiatorId, int level)
        {
            try
            {
                // Create a wild version of the gladiator
                var wildBot = BotFactory.CreateBot(gladiatorId, level);
                if (wildBot == null) return null;
                
                // Adjust stats based on encounter level
                AdjustWildGladiatorStats(wildBot, level);
                
                // Mark as wild (not owned by player)
                wildBot.HasOwner = false;
                
                return wildBot;
            }
            catch (Exception ex)
            {
                // Log error and return null
                Console.WriteLine($"Error generating wild gladiator {gladiatorId}: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Adjusts wild gladiator stats based on encounter level
        /// </summary>
        private void AdjustWildGladiatorStats(GladiatorBot bot, int encounterLevel)
        {
            // Apply level-based scaling
            var levelMultiplier = 1.0 + (encounterLevel - 1) * 0.1;
            
            bot.CurrentHealth = (int)(bot.CurrentHealth * levelMultiplier);
            bot.MaxHealth = bot.CurrentHealth;
            bot.AttackPower = (int)(bot.AttackPower * levelMultiplier);
            bot.Defense = (int)(bot.Defense * levelMultiplier);
            
            // Add some randomness to wild gladiator stats
            var varianceRange = 0.2; // ±20% variance
            
            bot.AttackPower = ApplyStatVariance(bot.AttackPower, varianceRange);
            bot.Defense = ApplyStatVariance(bot.Defense, varianceRange);
            bot.CurrentHealth = ApplyStatVariance(bot.CurrentHealth, varianceRange);
            bot.MaxHealth = bot.CurrentHealth;
            
            // Occasionally give wild gladiators special abilities or enhanced stats
            if (_rng.NextDouble() < 0.1) // 10% chance for "alpha" variants
            {
                bot.AttackPower = (int)(bot.AttackPower * 1.5);
                bot.Defense = (int)(bot.Defense * 1.3);
                bot.CurrentHealth = (int)(bot.CurrentHealth * 1.4);
                bot.MaxHealth = bot.CurrentHealth;
                
                // Could add special move or effect here
                bot.Name = $"Alpha {bot.Name}";
            }
        }
        
        /// <summary>
        /// Applies random variance to a stat value
        /// </summary>
        private int ApplyStatVariance(int baseStat, double variance)
        {
            var modifier = 1.0 + (_rng.NextDouble() - 0.5) * 2.0 * variance;
            return Math.Max(1, (int)(baseStat * modifier));
        }
        
        /// <summary>
        /// Determines appropriate encounter level based on player progress
        /// </summary>
        private int GetEncounterLevel(PlayerProfile player)
        {
            // Base level on player's strongest bot
            var strongestBot = player.BotRoster.OrderByDescending(b => b.AttackPower + b.Defense).FirstOrDefault();
            if (strongestBot == null) return 1;
            
            // Calculate effective level from bot stats
            var effectiveLevel = Math.Max(1, (strongestBot.AttackPower + strongestBot.Defense) / 20);
            
            // Add zone-based level scaling
            var zoneLevel = GetZoneLevel(player.WorldPosition.ZoneId);
            
            // Combine player level and zone level with some randomness
            var baseLevel = (effectiveLevel + zoneLevel) / 2;
            var variance = _rng.Next(-2, 3); // ±2 level variance
            
            return Math.Max(1, baseLevel + variance);
        }
        
        /// <summary>
        /// Gets the base level for encounters in a specific zone
        /// </summary>
        private int GetZoneLevel(string zoneId)
        {
            return zoneId switch
            {
                "starter_town" => 1,
                "route_1" => 3,
                "forest_path" => 8,
                "mountain_trail" => 15,
                "desert_ruins" => 25,
                "final_dungeon" => 40,
                _ => 5 // Default level
            };
        }
        
        /// <summary>
        /// Initiates a trainer battle with an NPC
        /// </summary>
        public void InitiateTrainerBattle(PlayerProfile player, WorldObject trainer)
        {
            if (trainer.Type != WorldObjectType.Trainer)
                return;
                
            OnTrainerBattle?.Invoke(player, trainer);
        }
        
        /// <summary>
        /// Checks if the player can engage in encounters (has at least one healthy bot)
        /// </summary>
        public bool CanEngageInEncounters(PlayerProfile player)
        {
            return player.BotRoster.Any(bot => bot.CurrentHealth > 0);
        }
        
        /// <summary>
        /// Creates encounter tables for different zones
        /// </summary>
        public Dictionary<string, List<string>> GetDefaultEncounterTables()
        {
            return new Dictionary<string, List<string>>
            {
                ["starter_town"] = new List<string>(), // No encounters in town
                ["route_1"] = new List<string> { "warrior", "guardian", "scout" },
                ["forest_path"] = new List<string> { "guardian", "assassin", "berserker" },
                ["mountain_trail"] = new List<string> { "berserker", "tank", "warrior" },
                ["desert_ruins"] = new List<string> { "assassin", "mage", "scout" },
                ["final_dungeon"] = new List<string> { "tank", "mage", "berserker", "assassin" }
            };
        }
        
        /// <summary>
        /// Sets up encounter tables for all zones in the world
        /// </summary>
        public void InitializeEncounterTables(WorldManager worldManager)
        {
            var encounterTables = GetDefaultEncounterTables();
            
            foreach (var zone in worldManager.GetAllZones())
            {
                if (encounterTables.TryGetValue(zone.Id, out var gladiatorList))
                {
                    // Add encounters to grass tiles in this zone
                    for (int x = 0; x < zone.Width; x++)
                    {
                        for (int y = 0; y < zone.Height; y++)
                        {
                            var tile = zone.GetTile(x, y);
                            if (tile?.Type == TileType.Grass && gladiatorList.Count > 0)
                            {
                                foreach (var gladiatorId in gladiatorList)
                                {
                                    tile.AddEncounterGladiator(gladiatorId);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}