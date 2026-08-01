using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services.Logging;
using AutoGladiators.Core.Enums;
using AutoGladiators.Core.Models.Exploration;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Core.Services.Exploration
{
    /// <summary>
    /// Enhanced encounter service supporting Pokemon-style mechanics
    /// Handles visible encounters, random encounters, weather effects, and day/night cycles
    /// </summary>
    public class EncounterService
    {
        private static readonly IAppLogger Log = AppLog.For<EncounterService>();

        private readonly Dictionary<string, BiomeData> _biomes;
        private readonly Random _rng = new();
        
        // Encounter state
        public TimeOfDay CurrentTimeOfDay { get; set; } = TimeOfDay.Day;
        public WeatherCondition CurrentWeather { get; set; } = WeatherCondition.Clear;
        
        public EncounterService()
        {
            _biomes = InitializeBiomes();
        }
        
        private Dictionary<string, BiomeData> InitializeBiomes()
        {
            return new Dictionary<string, BiomeData>
            {
                ["urban_junkyard"] = new BiomeData
                {
                    Name = "Urban Junkyard",
                    Description = "A sprawling wasteland of discarded machines and scrap metal",
                    BaseEncounterRate = 0.15,
                    WildBots = new List<WildBotEncounter>
                    {
                        new("ScrapBot", Rarity.Common, 1, 8, ElementalCore.Metal),
                        new("RustyGuard", Rarity.Common, 2, 10, ElementalCore.Metal),
                        new("JunkCrawler", Rarity.Uncommon, 5, 12, ElementalCore.Metal),
                        new("ElectroScavenger", Rarity.Uncommon, 8, 15, ElementalCore.Electric),
                        new("MetalKing", Rarity.Rare, 15, 25, ElementalCore.Metal),
                        new("TitanWrecker", Rarity.Legendary, 25, 35, ElementalCore.Metal)
                    },
                    TimeModifiers = new Dictionary<TimeOfDay, double>
                    {
                        [TimeOfDay.Day] = 1.0,
                        [TimeOfDay.Night] = 1.3, // More active at night
                        [TimeOfDay.Dawn] = 0.8,
                        [TimeOfDay.Dusk] = 1.1
                    },
                    WeatherModifiers = new Dictionary<WeatherCondition, double>
                    {
                        [WeatherCondition.Clear] = 1.0,
                        [WeatherCondition.Rain] = 1.2, // Electrical activity
                        [WeatherCondition.Storm] = 1.5,
                        [WeatherCondition.Fog] = 0.7
                    }
                },
                
                ["frozen_scrapyard"] = new BiomeData
                {
                    Name = "Frozen Scrapyard",
                    Description = "An ice-covered wasteland where cold-adapted bots roam",
                    BaseEncounterRate = 0.12,
                    WildBots = new List<WildBotEncounter>
                    {
                        new("FrostBot", Rarity.Common, 3, 10, ElementalCore.Ice),
                        new("IceCrawler", Rarity.Common, 5, 12, ElementalCore.Ice),
                        new("CryoSentry", Rarity.Uncommon, 10, 18, ElementalCore.Ice),
                        new("BlizzardHound", Rarity.Rare, 18, 28, ElementalCore.Ice),
                        new("GlacialTitan", Rarity.Legendary, 30, 40, ElementalCore.Ice)
                    },
                    TimeModifiers = new Dictionary<TimeOfDay, double>
                    {
                        [TimeOfDay.Day] = 0.8,
                        [TimeOfDay.Night] = 1.4, // Much more active in cold nights
                        [TimeOfDay.Dawn] = 1.0,
                        [TimeOfDay.Dusk] = 1.2
                    },
                    WeatherModifiers = new Dictionary<WeatherCondition, double>
                    {
                        [WeatherCondition.Clear] = 1.0,
                        [WeatherCondition.Snow] = 1.8, // Very active during snow
                        [WeatherCondition.Blizzard] = 2.0,
                        [WeatherCondition.Fog] = 1.3
                    }
                },
                
                ["desert_testing_grounds"] = new BiomeData
                {
                    Name = "Desert Testing Grounds",
                    Description = "Abandoned military testing facility with prototype war machines",
                    BaseEncounterRate = 0.10,
                    WildBots = new List<WildBotEncounter>
                    {
                        new("SandSkimmer", Rarity.Common, 8, 15, ElementalCore.Earth),
                        new("DuneRover", Rarity.Uncommon, 12, 20, ElementalCore.Earth),
                        new("WarPrototype", Rarity.Uncommon, 15, 25, ElementalCore.Metal),
                        new("MirageBot", Rarity.Rare, 20, 30, ElementalCore.Electric),
                        new("DesertCommander", Rarity.Legendary, 35, 45, ElementalCore.Metal)
                    },
                    TimeModifiers = new Dictionary<TimeOfDay, double>
                    {
                        [TimeOfDay.Day] = 0.6, // Less active in hot sun
                        [TimeOfDay.Night] = 1.6, // Much more active at night
                        [TimeOfDay.Dawn] = 1.2,
                        [TimeOfDay.Dusk] = 1.4
                    },
                    WeatherModifiers = new Dictionary<WeatherCondition, double>
                    {
                        [WeatherCondition.Clear] = 1.0,
                        [WeatherCondition.Sandstorm] = 1.8,
                        [WeatherCondition.ExtremeHeat] = 0.5
                    }
                },
                
                ["energy_plant"] = new BiomeData
                {
                    Name = "Abandoned Energy Plant",
                    Description = "A derelict power facility crackling with residual energy",
                    BaseEncounterRate = 0.18,
                    WildBots = new List<WildBotEncounter>
                    {
                        new("SparkDrone", Rarity.Common, 6, 12, ElementalCore.Electric),
                        new("VoltSpider", Rarity.Common, 8, 14, ElementalCore.Electric),
                        new("PowerCore", Rarity.Uncommon, 12, 20, ElementalCore.Electric),
                        new("ReactorGuard", Rarity.Uncommon, 18, 25, ElementalCore.Electric),
                        new("PlasmaLord", Rarity.Rare, 25, 35, ElementalCore.Electric),
                        new("OmegaGenerator", Rarity.Legendary, 40, 50, ElementalCore.Electric)
                    },
                    TimeModifiers = new Dictionary<TimeOfDay, double>
                    {
                        [TimeOfDay.Day] = 1.0,
                        [TimeOfDay.Night] = 1.2,
                        [TimeOfDay.Dawn] = 0.9,
                        [TimeOfDay.Dusk] = 0.9
                    },
                    WeatherModifiers = new Dictionary<WeatherCondition, double>
                    {
                        [WeatherCondition.Clear] = 1.0,
                        [WeatherCondition.Storm] = 2.0, // Very active during storms
                        [WeatherCondition.Rain] = 1.5,
                        [WeatherCondition.ElectricStorm] = 3.0
                    }
                }
            };
        }

        /// <summary>
        /// Attempts to trigger an encounter at the given location
        /// </summary>
        public bool TryTriggerEncounter(WorldPosition position, out GladiatorBot? encounteredBot, 
                                       List<string>? playerFlags = null)
        {
            encounteredBot = null;
            playerFlags ??= new List<string>();
            
            if (!_biomes.TryGetValue(position.ZoneId, out var biome))
                return false;
                
            // Calculate encounter chance based on conditions
            double encounterRate = biome.GetCurrentEncounterRate(CurrentTimeOfDay, CurrentWeather);
            
            if (_rng.NextDouble() > encounterRate)
                return false;
            
            // Get filtered encounter pool
            var availableEncounters = biome.GetFilteredEncounters(CurrentTimeOfDay, CurrentWeather);
            availableEncounters = availableEncounters.Where(e => e.CanEncounter(CurrentTimeOfDay, CurrentWeather, playerFlags)).ToList();
            
            if (!availableEncounters.Any())
                return false;
                
            // Select encounter based on rarity weights
            var selectedEncounter = SelectWeightedEncounter(availableEncounters);
            if (selectedEncounter == null)
                return false;
            
            // Generate the wild bot
            int playerLevel = GetPlayerLevel();
            encounteredBot = GenerateWildBot(selectedEncounter, playerLevel, position.ZoneId);
            
            Log.Info($"Wild encounter triggered: {encounteredBot?.Name} in {biome.Name}");
            return encounteredBot != null;
        }
        
        /// <summary>
        /// Checks for visible encounters at a position
        /// </summary>
        public List<VisibleEncounter> GetVisibleEncounters(WorldPosition position, int viewRadius = 3)
        {
            var visibleEncounters = new List<VisibleEncounter>();
            
            if (!_biomes.TryGetValue(position.ZoneId, out var biome))
                return visibleEncounters;
            
            // Generate visible encounters in the area
            for (int dx = -viewRadius; dx <= viewRadius; dx++)
            {
                for (int dy = -viewRadius; dy <= viewRadius; dy++)
                {
                    var checkPos = new WorldPosition(position.ZoneId, position.X + dx, position.Y + dy);
                    
                    // Use deterministic random based on position for consistent visible encounters
                    var positionSeed = HashCode.Combine(checkPos.ZoneId, checkPos.X, checkPos.Y);
                    var posRng = new Random(positionSeed);
                    
                    if (posRng.NextDouble() < 0.05) // 5% chance for visible encounter per tile
                    {
                        var encounters = biome.WildBots.Where(e => e.Type == EncounterType.Visible).ToList();
                        if (encounters.Any())
                        {
                            var encounter = encounters[posRng.Next(encounters.Count)];
                            visibleEncounters.Add(new VisibleEncounter
                            {
                                BotId = encounter.BotId,
                                Position = checkPos,
                                Behavior = encounter.SpecialBehavior,
                                CanFlee = encounter.CanFlee,
                                IsAggressive = encounter.IsAggressive
                            });
                        }
                    }
                }
            }
            
            return visibleEncounters;
        }
        
        /// <summary>
        /// Updates time of day and weather (called by game time system)
        /// </summary>
        public void UpdateConditions(TimeOfDay timeOfDay, WeatherCondition weather)
        {
            CurrentTimeOfDay = timeOfDay;
            CurrentWeather = weather;
        }
        
        /// <summary>
        /// Updates conditions from string values (for compatibility)
        /// </summary>
        public void UpdateConditions(string timeOfDay, string weather)
        {
            if (Enum.TryParse<TimeOfDay>(timeOfDay, out var tod))
                CurrentTimeOfDay = tod;
            if (Enum.TryParse<WeatherCondition>(weather, out var wc))
                CurrentWeather = wc;
        }
        
        /// <summary>
        /// Simple encounter check for MovementHandler - takes PlayerLocation and returns bool
        /// </summary>
        public bool CheckForEncounter(PlayerLocation location)
        {
            // Convert PlayerLocation to WorldPosition for biome lookup
            var worldPos = new WorldPosition(location.Region ?? "urban_junkyard", location.X, location.Y);
            
            // Try to get the biome for this zone
            if (_biomes.TryGetValue(worldPos.ZoneId, out var biome))
            {
                // Use biome's encounter rate with current conditions
                double encounterRate = biome.GetCurrentEncounterRate(CurrentTimeOfDay, CurrentWeather);
                return _rng.NextDouble() < encounterRate;
            }
            
            // Fallback to simple 5% chance if no biome found
            return _rng.NextDouble() < 0.05;
        }
        
        /// <summary>
        /// Event for when a wild encounter is triggered (compatibility with client)
        /// </summary>
        public event Action<PlayerProfile, GladiatorBot, string>? OnWildEncounter;
        
        /// <summary>
        /// Initializes encounter tables for world manager (compatibility method)
        /// </summary>
        public void InitializeEncounterTables(object worldManager)
        {
            // This method exists for compatibility with existing client code
            // The comprehensive encounter system handles encounters differently
            // but we provide this stub to prevent compilation errors
        }
        
        /// <summary>
        /// Async check for wild encounters (compatibility with client)
        /// </summary>
        public async Task<bool> CheckForWildEncounterAsync(PlayerProfile player)
        {
            return await Task.FromResult(false); // Stub for compatibility
        }
        
        /// <summary>
        /// Checks if a stealth/bait item affects encounter rate
        /// </summary>
        public double GetEncounterRateModifier(List<string> activeItems)
        {
            double modifier = 1.0;
            
            foreach (var item in activeItems)
            {
                modifier *= item switch
                {
                    "stealth_module" => 0.5,     // Reduce encounters by half
                    "noise_maker" => 2.0,        // Double encounter rate
                    "bot_bait" => 1.5,          // 50% more encounters
                    "repel_spray" => 0.1,        // Almost eliminate encounters
                    _ => 1.0
                };
            }
            
            return modifier;
        }
        
        private WildBotEncounter? SelectWeightedEncounter(List<WildBotEncounter> encounters)
        {
            var weightedPool = encounters
                .SelectMany(e => Enumerable.Repeat(e, GetWeight(e.Rarity)))
                .ToList();
                
            return weightedPool.Any() ? weightedPool[_rng.Next(weightedPool.Count)] : null;
        }
        
        private GladiatorBot? GenerateWildBot(WildBotEncounter encounter, int playerLevel, string zoneId = "route_1")
        {
            int botLevel = CalculateBotLevel(playerLevel, encounter);
            // Use new GenerateWildBot method with proper species and rarity system
            var bot = BotFactory.GenerateWildBot(zoneId, botLevel);
            
            if (bot != null)
            {
                // Apply environmental effects
                ApplyEnvironmentalEffects(bot, encounter);
                
                // Set wild bot properties
                bot.HasOwner = false;
                
                // Apply weather/time effects
                ApplyConditionEffects(bot);
            }
            
            return bot;
        }
        
        private int CalculateBotLevel(int playerLevel, WildBotEncounter encounter)
        {
            // More sophisticated level calculation
            int baseLevel = Math.Clamp(playerLevel + _rng.Next(-2, 3), encounter.MinLevel, encounter.MaxLevel);
            
            // Rare bots can be higher level
            if (encounter.Rarity == Rarity.Rare && _rng.NextDouble() < 0.3)
                baseLevel += _rng.Next(1, 4);
            else if (encounter.Rarity == Rarity.Legendary && _rng.NextDouble() < 0.5)
                baseLevel += _rng.Next(2, 6);
                
            return Math.Clamp(baseLevel, encounter.MinLevel, encounter.MaxLevel);
        }
        
        private void ApplyEnvironmentalEffects(GladiatorBot bot, WildBotEncounter encounter)
        {
            // Weather effects on stats
            switch (CurrentWeather)
            {
                case WeatherCondition.Storm when bot.ElementalCore == ElementalCore.Electric:
                    bot.AttackPower = (int)(bot.AttackPower * 1.2); // 20% boost in storms
                    break;
                    
                case WeatherCondition.Rain when bot.ElementalCore == ElementalCore.Fire:
                    bot.AttackPower = (int)(bot.AttackPower * 0.8); // 20% reduction in rain
                    break;
                    
                case WeatherCondition.Snow when bot.ElementalCore == ElementalCore.Ice:
                    bot.Speed = (int)(bot.Speed * 1.3); // Faster in snow
                    break;
            }
        }
        
        private void ApplyConditionEffects(GladiatorBot bot)
        {
            // Time-based effects
            switch (CurrentTimeOfDay)
            {
                case TimeOfDay.Night:
                    // Some bots are more aggressive at night
                    if (_rng.NextDouble() < 0.3)
                        bot.AttackPower = (int)(bot.AttackPower * 1.1);
                    break;
            }
        }
        
        private int GetPlayerLevel()
        {
            // TODO: Get from actual game state
            return GameStateService.Instance?.CurrentPlayer?.Level ?? 10;
        }

        private static int GetWeight(Rarity rarity) => rarity switch
        {
            Rarity.Common => 10,
            Rarity.Uncommon => 5,
            Rarity.Rare => 2,
            Rarity.Legendary => 1,
            _ => 1
        };
    }

    public enum Rarity { Common, Uncommon, Rare, Legendary }

    public class WildBotEncounter
    {
        private static readonly IAppLogger Log = AppLog.For<WildBotEncounter>();

        public string BotId { get; }
        public Rarity Rarity { get; }
        public int MinLevel { get; }
        public int MaxLevel { get; }
        public ElementalCore Element { get; }
        
        // New encounter preferences
        public List<TimeOfDay> PreferredTimes { get; set; } = new();
        public List<WeatherCondition> PreferredWeather { get; set; } = new();
        public EncounterType Type { get; set; } = EncounterType.Random;
        public bool CanFlee { get; set; } = true;
        public bool IsAggressive { get; set; } = false; // Approaches player
        
        // Special conditions
        public List<string> RequiredConditions { get; set; } = new(); // Quest flags, items, etc.
        public string SpecialBehavior { get; set; } = string.Empty; // "territorial", "pack_hunter", etc.

        public WildBotEncounter(string botId, Rarity rarity, int minLevel, int maxLevel, ElementalCore element = ElementalCore.None)
        {
            BotId = botId;
            Rarity = rarity;
            MinLevel = minLevel;
            MaxLevel = maxLevel;
            Element = element;
        }
        
        /// <summary>
        /// Checks if this encounter can occur under current conditions
        /// </summary>
        public bool CanEncounter(TimeOfDay timeOfDay, WeatherCondition weather, List<string> playerFlags)
        {
            // Check time preferences
            if (PreferredTimes.Count > 0 && !PreferredTimes.Contains(timeOfDay))
                return false;
                
            // Check weather preferences
            if (PreferredWeather.Count > 0 && !PreferredWeather.Contains(weather))
                return false;
                
            // Check special requirements
            foreach (var condition in RequiredConditions)
            {
                if (!playerFlags.Contains(condition))
                    return false;
            }
            
            return true;
        }

        /// <summary>
        /// Quick encounter check method for MovementHandler compatibility
        /// </summary>
        public bool CheckForEncounter(PlayerLocation location)
        {
            // Simple encounter roll - this can be enhanced with biome data
            var encounterChance = 0.05; // 5% base chance
            return new Random().NextDouble() < encounterChance;
        }
    }
    
    /// <summary>
    /// Types of encounters
    /// </summary>
    public enum EncounterType
    {
        Random,      // Traditional random encounter in grass/debris
        Visible,     // Visible on overworld map
        Stationary,  // Doesn't move, can be approached
        Patrol,      // Moves in set pattern
        Territorial, // Attacks if player gets too close
        Fleeing,     // Runs away when spotted
        Ambush       // Hidden until player is very close
    }
    
    /// <summary>
    /// Represents a visible encounter on the overworld map
    /// </summary>
    public class VisibleEncounter
    {
        public string BotId { get; set; } = string.Empty;
        public WorldPosition Position { get; set; }
        public string Behavior { get; set; } = string.Empty; // "territorial", "patrol", etc.
        public bool CanFlee { get; set; } = true;
        public bool IsAggressive { get; set; } = false;
        public MovementPattern MovementPattern { get; set; } = MovementPattern.Stationary;
        public List<WorldPosition> PatrolRoute { get; set; } = new();
        
        /// <summary>
        /// Checks if this encounter will engage the player
        /// </summary>
        public bool WillEngage(WorldPosition playerPos, int detectionRadius = 2)
        {
            double distance = Math.Sqrt(Math.Pow(Position.X - playerPos.X, 2) + Math.Pow(Position.Y - playerPos.Y, 2));
            
            if (IsAggressive && distance <= detectionRadius)
                return true;
                
            return distance <= 1; // Must be adjacent for non-aggressive encounters
        }

    }
    
    /// <summary>
    /// Movement patterns for visible encounters
    /// </summary>
    public enum MovementPattern
    {
        Stationary,     // Doesn't move
        Random,         // Moves randomly
        Patrol,         // Follows set route
        Chase,          // Moves toward player when spotted
        Flee,           // Moves away from player
        Circular,       // Moves in circles
        BackAndForth    // Moves back and forth between two points
    }
}
// This code defines an EncounterService that manages wild bot encounters in different regions.
// It includes methods to check for encounters, generate wild bots based on player level and region, and handle encounter logic.
