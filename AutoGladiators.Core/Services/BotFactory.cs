using System;
using System.Collections.Generic;
using System.Linq;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services.Logging;
using Microsoft.Extensions.Logging;
using AutoGladiators.Core.Enums;

namespace AutoGladiators.Core.Services
{
    /// <summary>
    /// Bot rarity determines stat multipliers and encounter chances
    /// </summary>
    public enum BotRarity 
    { 
        Common,     // 1.0x base stats, 50% chance
        Uncommon,   // 1.2x base stats, 30% chance  
        Rare,       // 1.5x base stats, 15% chance
        Epic,       // 2.0x base stats, 4% chance
        Legendary   // 2.5x base stats, 1% chance
    }

    /// <summary>
    /// Complete bot species definition for catalog and spawning
    /// </summary>
    public class BotSpecies
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ElementalCore Element { get; set; }
        public string Description { get; set; }
        public int BaseHealth { get; set; }
        public int BaseAttack { get; set; }
        public int BaseDefense { get; set; }
        public int BaseSpeed { get; set; }
        public int BaseLuck { get; set; }
        public double BaseCritChance { get; set; }
        public List<string> NaturalMoves { get; set; } = new();
        public List<string> AvailableRegions { get; set; } = new();
        public bool IsStarter { get; set; } = false;
        public string Lore { get; set; } = "";

        public BotSpecies(string id, string name, ElementalCore element, 
            int health, int attack, int defense, int speed, int luck = 10, double critChance = 5.0)
        {
            Id = id;
            Name = name;
            Element = element;
            Description = $"A {element} type bot with balanced combat abilities.";
            BaseHealth = health;
            BaseAttack = attack;
            BaseDefense = defense;
            BaseSpeed = speed;
            BaseLuck = luck;
            BaseCritChance = critChance;
            NaturalMoves = new List<string>();
            AvailableRegions = new List<string>();
            Lore = $"The {name} is known for its {element.ToString().ToLower()} affinity and tactical prowess.";
        }
    }

    public static class BotFactory
    {
        private static readonly IAppLogger Log = AppLog.For("BotFactory");

        private static readonly Random _rand = new();
        private static Dictionary<string, BotTemplate> _botTemplates = new();
        private static Dictionary<string, BotSpecies> _botSpecies = new();

        static BotFactory()
        {
            InitializeBotSpecies();
        }

        private static ElementalCore ParseElement(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return ElementalCore.None;
            return Enum.TryParse<ElementalCore>(value, ignoreCase: true, out var e)
                ? e
                : ElementalCore.None;
        }

        /// <summary>
        /// Generate a random rarity based on weighted chances
        /// </summary>
        public static BotRarity GenerateRarity()
        {
            var roll = _rand.NextDouble() * 100;
            return roll switch
            {
                <= 1.0 => BotRarity.Legendary,  // 1%
                <= 5.0 => BotRarity.Epic,       // 4% 
                <= 20.0 => BotRarity.Rare,      // 15%
                <= 50.0 => BotRarity.Uncommon,  // 30%
                _ => BotRarity.Common            // 50%
            };
        }

        /// <summary>
        /// Get stat multiplier for rarity
        /// </summary>
        public static double GetRarityMultiplier(BotRarity rarity)
        {
            return rarity switch
            {
                BotRarity.Common => 1.0,
                BotRarity.Uncommon => 1.2,
                BotRarity.Rare => 1.5,
                BotRarity.Epic => 2.0,
                BotRarity.Legendary => 2.5,
                _ => 1.0
            };
        }

        /// <summary>
        /// Initialize comprehensive bot species catalog
        /// </summary>
        private static void InitializeBotSpecies()
        {
            // STARTER BOTS - Balanced stats for new players
            RegisterStarter("flame_warrior", "Flame Warrior", ElementalCore.Fire, 
                85, 12, 10, 8, moves: new[] { "Tackle", "Ember", "Guard" },
                lore: "A loyal fire bot, perfect for beginning gladiators seeking reliable combat strength.");

            RegisterStarter("aqua_guardian", "Aqua Guardian", ElementalCore.Water, 
                90, 10, 12, 8, moves: new[] { "Tackle", "Water Pulse", "Shield" },
                lore: "A defensive water bot known for its protective nature and healing abilities.");

            RegisterStarter("volt_striker", "Volt Striker", ElementalCore.Electric, 
                80, 11, 9, 12, moves: new[] { "Tackle", "Thunder Shock", "Quick Attack" },
                lore: "A speedy electric bot that excels at hit-and-run tactics in battle.");

            // COMMON WILD BOTS - Found in early areas
            RegisterSpecies("scrap_drone", "Scrap Drone", ElementalCore.Metal, 
                70, 8, 12, 6, regions: new[] { "route_1", "starter_town" },
                moves: new[] { "Tackle", "Metal Claw" },
                lore: "Cobbled together from discarded parts, these hardy bots roam the outskirts.");

            RegisterSpecies("wind_wisp", "Wind Wisp", ElementalCore.Wind, 
                65, 9, 7, 14, regions: new[] { "route_1", "forest_path" },
                moves: new[] { "Gust", "Quick Attack" },
                lore: "Ethereal beings that dance on air currents, surprisingly nimble in combat.");

            RegisterSpecies("earth_golem", "Earth Golem", ElementalCore.Earth, 
                110, 11, 15, 4, regions: new[] { "mountain_trail", "desert_ruins" },
                moves: new[] { "Rock Throw", "Harden", "Slam" },
                lore: "Ancient constructs of stone and earth, slow but incredibly resilient.");

            RegisterSpecies("crystal_shard", "Crystal Shard", ElementalCore.Ice, 
                75, 10, 8, 9, regions: new[] { "mountain_trail", "final_dungeon" },
                moves: new[] { "Ice Shard", "Freeze", "Tackle" },
                lore: "Crystalline entities that reflect light beautifully while inflicting frostbite.");

            // UNCOMMON BOTS - Mid-tier encounters
            RegisterSpecies("plasma_hunter", "Plasma Hunter", ElementalCore.Plasma, 
                85, 14, 9, 11, regions: new[] { "desert_ruins", "final_dungeon" },
                moves: new[] { "Plasma Beam", "Energy Drain", "Phase Shift" },
                lore: "Elite energy beings that hunt using concentrated plasma weaponry.");

            RegisterSpecies("inferno_knight", "Inferno Knight", ElementalCore.Fire, 
                95, 16, 13, 7, regions: new[] { "forest_path", "final_dungeon" },
                moves: new[] { "Flame Sword", "Fire Shield", "Burning Charge" },
                lore: "Armored warriors wreathed in eternal flame, honorable but deadly opponents.");

            RegisterSpecies("tsunami_leviathan", "Tsunami Leviathan", ElementalCore.Water, 
                120, 13, 11, 8, regions: new[] { "desert_ruins" }, // Oasis encounters
                moves: new[] { "Tidal Wave", "Aqua Barrier", "Crush" },
                lore: "Massive aquatic beasts capable of summoning devastating water attacks.");

            RegisterSpecies("thunder_titan", "Thunder Titan", ElementalCore.Electric, 
                100, 15, 10, 10, regions: new[] { "mountain_trail", "final_dungeon" },
                moves: new[] { "Lightning Strike", "Thunder Roar", "Electric Field" },
                lore: "Colossal beings of pure electrical energy that command the power of storms.");

            // RARE BOTS - Powerful late-game encounters
            RegisterSpecies("void_assassin", "Void Assassin", ElementalCore.None, 
                90, 18, 8, 16, regions: new[] { "final_dungeon" },
                moves: new[] { "Shadow Strike", "Vanish", "Critical Hit", "Void Slash" },
                lore: "Mysterious entities from beyond reality, masters of stealth and precision strikes.");

            RegisterSpecies("metal_emperor", "Metal Emperor", ElementalCore.Metal, 
                140, 17, 20, 6, regions: new[] { "final_dungeon" },
                moves: new[] { "Steel Fortress", "Metal Storm", "Armor Break", "Iron Will" },
                lore: "Legendary mechanical rulers forged from the finest alloys in existence.");

            RegisterSpecies("wind_sovereign", "Wind Sovereign", ElementalCore.Wind, 
                105, 16, 12, 18, regions: new[] { "final_dungeon" },
                moves: new[] { "Hurricane", "Wind Barrier", "Aerial Dance", "Storm Call" },
                lore: "Majestic aerial beings that rule the skies with unmatched speed and grace.");

            // EPIC BOTS - Extremely rare encounters
            RegisterSpecies("crimson_phoenix", "Crimson Phoenix", ElementalCore.Fire, 
                130, 22, 15, 14, regions: new[] { "final_dungeon" },
                moves: new[] { "Phoenix Rising", "Inferno Blast", "Regenerate", "Solar Flare" },
                lore: "Mythical birds of fire that can resurrect from their own ashes, embodying eternal rebirth.");

            RegisterSpecies("arctic_wyrm", "Arctic Wyrm", ElementalCore.Ice, 
                150, 19, 18, 10, regions: new[] { "final_dungeon" },
                moves: new[] { "Absolute Zero", "Ice Prison", "Frost Bite", "Blizzard" },
                lore: "Ancient ice dragons that can freeze entire battlefields with their breath.");

            // LEGENDARY BOTS - Ultimate encounters
            RegisterSpecies("omega_prime", "Omega Prime", ElementalCore.Plasma, 
                200, 25, 20, 15, regions: new[] { "final_dungeon" },
                moves: new[] { "Omega Cannon", "Reality Warp", "Energy Shield", "Prime Directive" },
                lore: "The ultimate fusion of technology and energy, said to be the prototype for all bots.");

            RegisterSpecies("elemental_fusion", "Elemental Fusion", ElementalCore.None, 
                175, 23, 22, 18, regions: new[] { "final_dungeon" },
                moves: new[] { "Elemental Storm", "Fusion Beam", "Adaptive Shield", "Harmony Strike" },
                lore: "A perfect synthesis of all elemental forces, representing balance and ultimate power.");

            Log.Info($"Initialized {_botSpecies.Count} bot species in the catalog");
        }

        private static void RegisterStarter(string id, string name, ElementalCore element, 
            int health, int attack, int defense, int speed, string[] moves, string lore)
        {
            var species = new BotSpecies(id, name, element, health, attack, defense, speed)
            {
                IsStarter = true,
                Lore = lore,
                AvailableRegions = new List<string> { "starter_town" }
            };
            species.NaturalMoves.AddRange(moves);
            _botSpecies[id] = species;
        }

        private static void RegisterSpecies(string id, string name, ElementalCore element, 
            int health, int attack, int defense, int speed, string[] regions, 
            string[] moves, string lore)
        {
            var species = new BotSpecies(id, name, element, health, attack, defense, speed)
            {
                Lore = lore
            };
            species.AvailableRegions.AddRange(regions);
            species.NaturalMoves.AddRange(moves);
            _botSpecies[id] = species;
        }

        /// <summary>
        /// Generate a wild bot encounter for a specific region
        /// </summary>
        public static GladiatorBot GenerateWildBot(string region, int playerLevel)
        {
            // Get available species for this region
            var availableSpecies = _botSpecies.Values
                .Where(s => s.AvailableRegions.Contains(region) && !s.IsStarter)
                .ToList();

            if (!availableSpecies.Any())
            {
                Log.Warn($"No bot species found for region {region}, using fallback");
                return CreateFallbackBot(playerLevel);
            }

            // Select random species
            var species = availableSpecies[_rand.Next(availableSpecies.Count)];
            
            // Generate level slightly around player level
            var level = Math.Max(1, playerLevel + _rand.Next(-1, 3));
            
            // Generate rarity
            var rarity = GenerateRarity();
            
            return CreateBotFromSpecies(species, level, rarity, isWild: true);
        }

        /// <summary>
        /// Create a bot by species ID (for starters or specific spawns)
        /// </summary>
        public static GladiatorBot CreateBot(string speciesId, int level)
        {
            if (_botSpecies.TryGetValue(speciesId, out var species))
            {
                var rarity = species.IsStarter ? BotRarity.Common : GenerateRarity();
                return CreateBotFromSpecies(species, level, rarity, isWild: false);
            }

            // Fallback for legacy bot IDs not in our species catalog
            Log.Warn($"Species {speciesId} not found in catalog, creating fallback bot");
            return CreateFallbackBot(level, speciesId);
        }

        /// <summary>
        /// Create a bot from a species with rarity modifiers applied
        /// </summary>
        private static GladiatorBot CreateBotFromSpecies(BotSpecies species, int level, BotRarity rarity, bool isWild = false)
        {
            var rarityMultiplier = GetRarityMultiplier(rarity);
            var levelMultiplier = Math.Max(1, level);

            var bot = new GladiatorBot
            {
                Name = isWild ? $"Wild {species.Name}" : species.Name,
                ElementalCore = species.Element,
                Level = level,
                HasOwner = !isWild,
                
                // Apply rarity multipliers to base stats
                MaxHealth = (int)(species.BaseHealth * rarityMultiplier * levelMultiplier),
                AttackPower = (int)(species.BaseAttack * rarityMultiplier + (level - 1) * 2),
                Defense = (int)(species.BaseDefense * rarityMultiplier + (level - 1)),
                Speed = (int)(species.BaseSpeed * rarityMultiplier + (level - 1)),
                Luck = (int)(species.BaseLuck * rarityMultiplier + (level - 1)),
                CriticalHitChance = species.BaseCritChance * rarityMultiplier,

                // Energy and other stats
                Energy = 100 + (level * 5),
                MaxEnergy = 100 + (level * 5),
                Endurance = 10 + level,
                
                // Copy moveset
                Moveset = new List<string>(species.NaturalMoves),
                
                // Set rarity and description
                Rarity = rarity.ToString(),
                Description = $"{rarity} {species.Description}"
            };

            // Set current health to max
            bot.CurrentHealth = bot.MaxHealth;

            Log.Info($"Created {rarity} {species.Name} (Level {level}, {species.Element})");
            return bot;
        }

        /// <summary>
        /// Create a fallback bot when species isn't found
        /// </summary>
        private static GladiatorBot CreateFallbackBot(int level, string name = "Unknown Bot")
        {
            var bot = new GladiatorBot
            {
                Name = name,
                Level = level,
                ElementalCore = ElementalCore.None,
                MaxHealth = 60 + (level * 8),
                AttackPower = 8 + level,
                Defense = 6 + level,
                Speed = 7 + level,
                Luck = 10 + level,
                CriticalHitChance = 5.0,
                Energy = 100,
                MaxEnergy = 100,
                Endurance = 10,
                Moveset = new List<string> { "Tackle" },
                HasOwner = false,
                Description = "A basic bot with unknown origins."
            };

            bot.CurrentHealth = bot.MaxHealth;
            return bot;
        }

        private static List<string> GetBasicMovesetForElement(ElementalCore element)
        {
            // For now, use only moves that exist in MoveDatabase
            // TODO: Expand MoveDatabase with element-specific moves
            return element switch
            {
                ElementalCore.Metal => new List<string> { "Tackle", "Metal Strike", "Guard" },
                _ => new List<string> { "Tackle", "Metal Strike", "Guard" }
            };
        }

        public static void LoadBotTemplates(List<BotTemplate> templates)
        {
            _botTemplates = templates.ToDictionary(t => t.Id, t => t);
        }

        public static GladiatorBot CreateBotFromTemplate(string templateId, int level)
        {
            if (!_botTemplates.TryGetValue(templateId, out var template))
                throw new Exception($"BotTemplate '{templateId}' not found.");

            return new GladiatorBot
            {
                Name = template.Name,
                // FIX 3: template.Element is a string → parse to enum
                ElementalCore = ParseElement(template.Element),
                Level = level,
                MaxHealth = template.BaseHealth + level * 10,
                CurrentHealth = template.BaseHealth + level * 10,
                AttackPower = template.BaseAttack + level,
                Defense = template.BaseDefense + level,
                Speed = template.BaseSpeed + level,
                Luck = template.BaseLuck + level,
                CriticalHitChance = template.CriticalHitChance,
                Energy = 100,
                MaxEnergy = 100,
                Moveset = new List<string>(template.MoveIds),
                HasOwner = false
            };
        }

        /// <summary>
        /// Get all available starter species
        /// </summary>
        public static List<BotSpecies> GetStarterSpecies()
        {
            return _botSpecies.Values.Where(s => s.IsStarter).ToList();
        }

        /// <summary>
        /// Get all available species for a region
        /// </summary>
        public static List<BotSpecies> GetSpeciesForRegion(string region)
        {
            return _botSpecies.Values.Where(s => s.AvailableRegions.Contains(region)).ToList();
        }

        /// <summary>
        /// Get comprehensive bot catalog for display
        /// </summary>
        public static Dictionary<string, List<BotSpecies>> GetBotCatalog()
        {
            var catalog = new Dictionary<string, List<BotSpecies>>();
            
            // Group by regions
            foreach (var species in _botSpecies.Values)
            {
                if (species.IsStarter)
                {
                    if (!catalog.ContainsKey("Starter Bots"))
                        catalog["Starter Bots"] = new List<BotSpecies>();
                    catalog["Starter Bots"].Add(species);
                }
                else
                {
                    foreach (var region in species.AvailableRegions)
                    {
                        if (!catalog.ContainsKey(region))
                            catalog[region] = new List<BotSpecies>();
                        catalog[region].Add(species);
                    }
                }
            }

            return catalog;
        }

        /// <summary>
        /// Get species by element type
        /// </summary>
        public static List<BotSpecies> GetSpeciesByElement(ElementalCore element)
        {
            return _botSpecies.Values.Where(s => s.Element == element).ToList();
        }

        /// <summary>
        /// Get total number of species in catalog
        /// </summary>
        public static int GetTotalSpeciesCount() => _botSpecies.Count;

        /// <summary>
        /// Check if a species exists
        /// </summary>
        public static bool SpeciesExists(string speciesId) => _botSpecies.ContainsKey(speciesId);

        /// <summary>
        /// Get species info by ID
        /// </summary>
        public static BotSpecies? GetSpeciesById(string speciesId) => 
            _botSpecies.TryGetValue(speciesId, out var species) ? species : null;
    }
}
