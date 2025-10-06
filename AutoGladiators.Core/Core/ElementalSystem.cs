using AutoGladiators.Core.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AutoGladiators.Core.Core
{
    /// <summary>
    /// Comprehensive elemental type system for AutoGladiators
    /// Handles type advantages, resistances, immunities, and special interactions
    /// </summary>
    public static class ElementalSystem
    {
        #region Type Effectiveness Chart
        
        /// <summary>
        /// Type effectiveness chart: [Attacker][Defender] = Multiplier
        /// 2.0 = Super Effective, 1.0 = Normal, 0.5 = Not Very Effective, 0.25 = Resistant, 0.0 = Immune
        /// </summary>
        private static readonly Dictionary<ElementalCore, Dictionary<ElementalCore, double>> TypeChart = new()
        {
            [ElementalCore.Fire] = new()
            {
                [ElementalCore.Grass] = 2.0,  // Fire burns Grass
                [ElementalCore.Ice] = 2.0,    // Fire melts Ice
                [ElementalCore.Metal] = 2.0,  // Fire forges Metal
                [ElementalCore.Water] = 0.5,  // Water extinguishes Fire
                [ElementalCore.Fire] = 0.5,   // Fire resists Fire
                [ElementalCore.Earth] = 0.5,  // Earth smothers Fire
                [ElementalCore.Electric] = 1.0,
                [ElementalCore.Wind] = 0.5,   // Wind can extinguish or fan Fire
                [ElementalCore.Plasma] = 1.0,
                [ElementalCore.None] = 1.0
            },
            
            [ElementalCore.Water] = new()
            {
                [ElementalCore.Fire] = 2.0,   // Water extinguishes Fire
                [ElementalCore.Earth] = 2.0,  // Water erodes Earth
                [ElementalCore.Metal] = 2.0,  // Water rusts Metal
                [ElementalCore.Grass] = 0.5,  // Grass absorbs Water
                [ElementalCore.Water] = 0.5,  // Water resists Water
                [ElementalCore.Ice] = 0.5,    // Ice is frozen Water
                [ElementalCore.Electric] = 0.25, // Water conducts electricity (takes more damage)
                [ElementalCore.Wind] = 1.0,
                [ElementalCore.Plasma] = 1.0,
                [ElementalCore.None] = 1.0
            },
            
            [ElementalCore.Electric] = new()
            {
                [ElementalCore.Water] = 2.0,  // Electricity conducts through Water
                [ElementalCore.Metal] = 2.0,  // Electricity conducts through Metal
                [ElementalCore.Wind] = 2.0,   // Lightning travels through air
                [ElementalCore.Earth] = 0.0,  // Earth grounds electricity (immune)
                [ElementalCore.Electric] = 0.5, // Electric resists Electric
                [ElementalCore.Grass] = 0.5,  // Grass is poor conductor
                [ElementalCore.Fire] = 1.0,
                [ElementalCore.Ice] = 1.0,
                [ElementalCore.Plasma] = 0.5, // Plasma is electrical in nature
                [ElementalCore.None] = 1.0
            },
            
            [ElementalCore.Grass] = new()
            {
                [ElementalCore.Water] = 2.0,  // Plants absorb water
                [ElementalCore.Earth] = 2.0,  // Plants grow from earth
                [ElementalCore.Electric] = 2.0, // Plants ground electricity
                [ElementalCore.Fire] = 0.25,  // Fire burns plants easily
                [ElementalCore.Ice] = 0.5,    // Cold damages plants
                [ElementalCore.Metal] = 0.5,  // Metal tools cut plants
                [ElementalCore.Wind] = 0.5,   // Wind can uproot plants
                [ElementalCore.Grass] = 0.5,  // Nature resists nature
                [ElementalCore.Plasma] = 0.5,
                [ElementalCore.None] = 1.0
            },
            
            [ElementalCore.Ice] = new()
            {
                [ElementalCore.Grass] = 2.0,  // Ice freezes plants
                [ElementalCore.Water] = 2.0,  // Ice freezes water
                [ElementalCore.Earth] = 2.0,  // Ice can shatter earth
                [ElementalCore.Wind] = 2.0,   // Ice storm effects
                [ElementalCore.Fire] = 0.25,  // Fire easily melts ice
                [ElementalCore.Metal] = 0.5,  // Metal conducts cold
                [ElementalCore.Ice] = 0.5,    // Ice resists ice
                [ElementalCore.Electric] = 1.0,
                [ElementalCore.Plasma] = 0.25, // Plasma is extremely hot
                [ElementalCore.None] = 1.0
            },
            
            [ElementalCore.Earth] = new()
            {
                [ElementalCore.Fire] = 2.0,   // Earth smothers fire
                [ElementalCore.Electric] = 2.0, // Earth grounds electricity
                [ElementalCore.Metal] = 2.0,  // Earth contains metal ores
                [ElementalCore.Wind] = 2.0,   // Earth is solid against wind
                [ElementalCore.Water] = 0.5,  // Water erodes earth
                [ElementalCore.Grass] = 0.5,  // Plants break through earth
                [ElementalCore.Ice] = 0.5,    // Ice can crack earth
                [ElementalCore.Earth] = 0.5,  // Earth resists earth
                [ElementalCore.Plasma] = 1.0,
                [ElementalCore.None] = 1.0
            },
            
            [ElementalCore.Metal] = new()
            {
                [ElementalCore.Ice] = 2.0,    // Metal conducts cold efficiently
                [ElementalCore.Earth] = 2.0,  // Metal tools dig earth
                [ElementalCore.Grass] = 2.0,  // Metal tools cut plants
                [ElementalCore.Fire] = 0.5,   // Fire forges metal
                [ElementalCore.Water] = 0.5,  // Water rusts metal
                [ElementalCore.Electric] = 0.5, // Metal conducts electricity
                [ElementalCore.Metal] = 0.5,  // Metal resists metal
                [ElementalCore.Wind] = 1.0,
                [ElementalCore.Plasma] = 0.25, // Plasma cuts through metal
                [ElementalCore.None] = 1.0
            },
            
            [ElementalCore.Wind] = new()
            {
                [ElementalCore.Fire] = 2.0,   // Wind fans flames
                [ElementalCore.Grass] = 2.0,  // Wind can uproot plants
                [ElementalCore.Ice] = 0.5,    // Cold air is denser
                [ElementalCore.Earth] = 0.5,  // Earth blocks wind
                [ElementalCore.Electric] = 0.5, // Lightning travels through air
                [ElementalCore.Metal] = 1.0,
                [ElementalCore.Water] = 1.0,
                [ElementalCore.Wind] = 0.5,   // Wind resists wind
                [ElementalCore.Plasma] = 1.0,
                [ElementalCore.None] = 1.0
            },
            
            [ElementalCore.Plasma] = new()
            {
                [ElementalCore.Metal] = 2.0,  // Plasma cuts through metal
                [ElementalCore.Ice] = 2.0,    // Plasma melts ice instantly
                [ElementalCore.Water] = 2.0,  // Plasma vaporizes water
                [ElementalCore.Earth] = 2.0,  // Plasma can melt earth/rock
                [ElementalCore.Fire] = 1.5,   // Plasma is hotter than fire
                [ElementalCore.Electric] = 0.5, // Plasma is ionized matter
                [ElementalCore.Wind] = 1.0,
                [ElementalCore.Grass] = 2.0,  // Plasma incinerates plants
                [ElementalCore.Plasma] = 0.5, // Plasma resists plasma
                [ElementalCore.None] = 1.0
            },
            
            [ElementalCore.None] = new()
            {
                [ElementalCore.Fire] = 1.0,
                [ElementalCore.Water] = 1.0,
                [ElementalCore.Electric] = 1.0,
                [ElementalCore.Grass] = 1.0,
                [ElementalCore.Ice] = 1.0,
                [ElementalCore.Earth] = 1.0,
                [ElementalCore.Metal] = 1.0,
                [ElementalCore.Wind] = 1.0,
                [ElementalCore.Plasma] = 1.0,
                [ElementalCore.None] = 1.0
            }
        };
        
        #endregion
        
        #region Core Functionality
        
        /// <summary>
        /// Gets the type effectiveness multiplier for attacker vs defender
        /// </summary>
        public static double GetTypeEffectiveness(ElementalCore attacker, ElementalCore defender)
        {
            if (TypeChart.TryGetValue(attacker, out var attackerChart) &&
                attackerChart.TryGetValue(defender, out var effectiveness))
            {
                return effectiveness;
            }
            
            return 1.0; // Default neutral effectiveness
        }
        
        /// <summary>
        /// Legacy method for backward compatibility
        /// </summary>
        public static double GetModifier(ElementalCore attacker, ElementalCore defender) 
            => GetTypeEffectiveness(attacker, defender);
        
        /// <summary>
        /// Gets the damage modifier including type effectiveness and random variance
        /// </summary>
        public static double GetDamageModifier(ElementalCore attackerType, ElementalCore defenderType, 
            bool includeRandomness = false, Random? rng = null)
        {
            double baseModifier = GetTypeEffectiveness(attackerType, defenderType);
            
            if (includeRandomness && rng != null)
            {
                // Add 10% random variance to make battles less predictable
                double variance = 0.9 + (rng.NextDouble() * 0.2); // 0.9 to 1.1
                baseModifier *= variance;
            }
            
            return baseModifier;
        }
        
        /// <summary>
        /// Determines if an attack is immune (deals no damage)
        /// </summary>
        public static bool IsImmune(ElementalCore attackerType, ElementalCore defenderType)
        {
            return GetTypeEffectiveness(attackerType, defenderType) <= 0.0;
        }
        
        /// <summary>
        /// Determines if an attack is super effective (> 1.5x damage)
        /// </summary>
        public static bool IsSuperEffective(ElementalCore attackerType, ElementalCore defenderType)
        {
            return GetTypeEffectiveness(attackerType, defenderType) >= 1.5;
        }
        
        /// <summary>
        /// Determines if an attack is not very effective (< 0.75x damage)
        /// </summary>
        public static bool IsNotVeryEffective(ElementalCore attackerType, ElementalCore defenderType)
        {
            double effectiveness = GetTypeEffectiveness(attackerType, defenderType);
            return effectiveness > 0.0 && effectiveness < 0.75;
        }
        
        #endregion
        
        #region Element Properties
        
        /// <summary>
        /// Gets the display properties for an elemental type
        /// </summary>
        public static ElementalProperties GetElementalProperties(ElementalCore element)
        {
            return element switch
            {
                ElementalCore.Fire => new ElementalProperties
                {
                    Name = "Fire",
                    Color = "#FF4500",
                    Icon = "🔥",
                    Description = "Aggressive and powerful, excels against nature and ice",
                    StatBonuses = new() { { "AttackPower", 1.2 }, { "Speed", 1.1 } },
                    WeaknessCount = 3,
                    ResistanceCount = 3
                },
                ElementalCore.Water => new ElementalProperties
                {
                    Name = "Water",
                    Color = "#1E90FF", 
                    Icon = "💧",
                    Description = "Versatile and flowing, dominates fire and earth",
                    StatBonuses = new() { { "Defense", 1.1 }, { "MaxHealth", 1.15 } },
                    WeaknessCount = 2,
                    ResistanceCount = 3
                },
                ElementalCore.Electric => new ElementalProperties
                {
                    Name = "Electric",
                    Color = "#FFD700",
                    Icon = "⚡",
                    Description = "Fast and shocking, excellent against water and metal",
                    StatBonuses = new() { { "Speed", 1.3 }, { "CriticalHitChance", 1.2 } },
                    WeaknessCount = 1,
                    ResistanceCount = 3
                },
                ElementalCore.Grass => new ElementalProperties
                {
                    Name = "Grass",
                    Color = "#32CD32",
                    Icon = "🌿",
                    Description = "Natural and resilient, absorbs water and grounds electricity",
                    StatBonuses = new() { { "Defense", 1.2 }, { "MaxHealth", 1.1 } },
                    WeaknessCount = 4,
                    ResistanceCount = 4
                },
                ElementalCore.Ice => new ElementalProperties
                {
                    Name = "Ice",
                    Color = "#87CEEB",
                    Icon = "❄️",
                    Description = "Precise and freezing, effective against nature and water",
                    StatBonuses = new() { { "AttackPower", 1.1 }, { "Defense", 1.15 } },
                    WeaknessCount = 3,
                    ResistanceCount = 2
                },
                ElementalCore.Earth => new ElementalProperties
                {
                    Name = "Earth",
                    Color = "#8B4513",
                    Icon = "🪨",
                    Description = "Solid and dependable, immune to electricity",
                    StatBonuses = new() { { "Defense", 1.25 }, { "MaxHealth", 1.2 } },
                    WeaknessCount = 3,
                    ResistanceCount = 4
                },
                ElementalCore.Metal => new ElementalProperties
                {
                    Name = "Metal",
                    Color = "#C0C0C0",
                    Icon = "⚔️",
                    Description = "Sharp and durable, cuts through nature and ice",
                    StatBonuses = new() { { "AttackPower", 1.15 }, { "Defense", 1.2 } },
                    WeaknessCount = 4,
                    ResistanceCount = 4
                },
                ElementalCore.Wind => new ElementalProperties
                {
                    Name = "Wind",
                    Color = "#E6E6FA",
                    Icon = "💨",
                    Description = "Swift and evasive, fans flames and moves freely",
                    StatBonuses = new() { { "Speed", 1.25 }, { "Evasion", 1.3 } },
                    WeaknessCount = 3,
                    ResistanceCount = 3
                },
                ElementalCore.Plasma => new ElementalProperties
                {
                    Name = "Plasma",
                    Color = "#9400D3",
                    Icon = "⭐",
                    Description = "Rare and devastating, the fourth state of matter",
                    StatBonuses = new() { { "AttackPower", 1.3 }, { "Speed", 1.1 }, { "Energy", 1.2 } },
                    WeaknessCount = 1,
                    ResistanceCount = 2
                },
                _ => new ElementalProperties
                {
                    Name = "Neutral",
                    Color = "#808080",
                    Icon = "⚪",
                    Description = "Balanced and adaptable, no special advantages",
                    StatBonuses = new(),
                    WeaknessCount = 0,
                    ResistanceCount = 0
                }
            };
        }
        
        /// <summary>
        /// Parses an elemental type from string (case-insensitive) - Legacy method
        /// </summary>
        public static ElementalCore GetElementalCore(string type) => ParseElementalCore(type);
        
        /// <summary>
        /// Parses an elemental type from string (case-insensitive)
        /// </summary>
        public static ElementalCore ParseElementalCore(string? elementString)
        {
            if (string.IsNullOrWhiteSpace(elementString))
                return ElementalCore.None;
                
            return elementString.ToLowerInvariant().Trim() switch
            {
                "fire" or "flame" or "burn" => ElementalCore.Fire,
                "water" or "aqua" or "hydro" => ElementalCore.Water,
                "electric" or "lightning" or "thunder" or "shock" => ElementalCore.Electric,
                "grass" or "nature" or "plant" or "leaf" => ElementalCore.Grass,
                "ice" or "frost" or "freeze" or "cold" => ElementalCore.Ice,
                "earth" or "ground" or "rock" or "stone" => ElementalCore.Earth,
                "metal" or "steel" or "iron" => ElementalCore.Metal,
                "wind" or "air" or "breeze" or "gust" => ElementalCore.Wind,
                "plasma" or "energy" or "cosmic" => ElementalCore.Plasma,
                "none" or "neutral" or "normal" => ElementalCore.None,
                _ => ElementalCore.None
            };
        }
        
        /// <summary>
        /// Gets all elemental types except None
        /// </summary>
        public static ReadOnlyCollection<ElementalCore> GetAllElements()
        {
            return new ReadOnlyCollection<ElementalCore>(new[]
            {
                ElementalCore.Fire, ElementalCore.Water, ElementalCore.Electric,
                ElementalCore.Grass, ElementalCore.Ice, ElementalCore.Earth,
                ElementalCore.Metal, ElementalCore.Wind, ElementalCore.Plasma
            });
        }
        
        /// <summary>
        /// Gets elements that are strong against the specified element
        /// </summary>
        public static List<ElementalCore> GetStrongAgainst(ElementalCore element)
        {
            var strongElements = new List<ElementalCore>();
            
            foreach (var attackerType in GetAllElements())
            {
                if (GetTypeEffectiveness(attackerType, element) >= 1.5)
                {
                    strongElements.Add(attackerType);
                }
            }
            
            return strongElements;
        }
        
        /// <summary>
        /// Gets elements that are weak against the specified element
        /// </summary>
        public static List<ElementalCore> GetWeakAgainst(ElementalCore element)
        {
            var weakElements = new List<ElementalCore>();
            
            foreach (var defenderType in GetAllElements())
            {
                if (GetTypeEffectiveness(element, defenderType) >= 1.5)
                {
                    weakElements.Add(defenderType);
                }
            }
            
            return weakElements;
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Gets a user-friendly effectiveness description
        /// </summary>
        public static string GetEffectivenessDescription(ElementalCore attacker, ElementalCore defender)
        {
            double effectiveness = GetTypeEffectiveness(attacker, defender);
            
            return effectiveness switch
            {
                0.0 => "has no effect on",
                <= 0.25 => "is barely effective against",
                <= 0.5 => "is not very effective against", 
                < 1.0 => "is somewhat effective against",
                1.0 => "is normally effective against",
                < 1.5 => "is quite effective against",
                < 2.0 => "is super effective against",
                _ => "is devastatingly effective against"
            };
        }
        
        /// <summary>
        /// Calculates the overall defensive typing quality (lower is better defensively)
        /// </summary>
        public static double GetDefensiveRating(ElementalCore element)
        {
            double totalDamage = 0;
            int typeCount = 0;
            
            foreach (var attackerType in GetAllElements())
            {
                totalDamage += GetTypeEffectiveness(attackerType, element);
                typeCount++;
            }
            
            return totalDamage / typeCount; // Average damage taken
        }
        
        /// <summary>
        /// Calculates the overall offensive typing quality (higher is better offensively)
        /// </summary>
        public static double GetOffensiveRating(ElementalCore element)
        {
            double totalDamage = 0;
            int typeCount = 0;
            
            foreach (var defenderType in GetAllElements())
            {
                totalDamage += GetTypeEffectiveness(element, defenderType);
                typeCount++;
            }
            
            return totalDamage / typeCount; // Average damage dealt
        }
        
        #endregion
    }
    
    #region Supporting Classes
    
    /// <summary>
    /// Represents the properties and characteristics of an elemental type
    /// </summary>
    public class ElementalProperties
    {
        public string Name { get; set; } = "";
        public string Color { get; set; } = "#808080";
        public string Icon { get; set; } = "⚪";
        public string Description { get; set; } = "";
        public Dictionary<string, double> StatBonuses { get; set; } = new();
        public int WeaknessCount { get; set; }
        public int ResistanceCount { get; set; }
        
        /// <summary>
        /// Applies stat bonuses to a bot based on its elemental type
        /// </summary>
        public void ApplyStatBonuses(GladiatorBot bot)
        {
            foreach (var bonus in StatBonuses)
            {
                switch (bonus.Key.ToLowerInvariant())
                {
                    case "attackpower":
                        bot.AttackPower = (int)(bot.AttackPower * bonus.Value);
                        break;
                    case "defense":
                        bot.Defense = (int)(bot.Defense * bonus.Value);
                        break;
                    case "maxhealth":
                        int newMaxHealth = (int)(bot.MaxHealth * bonus.Value);
                        int healthDiff = newMaxHealth - bot.MaxHealth;
                        bot.MaxHealth = newMaxHealth;
                        bot.CurrentHealth += healthDiff; // Maintain health percentage
                        break;
                    case "speed":
                        bot.Speed = (int)(bot.Speed * bonus.Value);
                        break;
                    case "criticalhitchance":
                        bot.CriticalHitChance *= bonus.Value;
                        break;
                    case "energy":
                        bot.MaxEnergy = (int)(bot.MaxEnergy * bonus.Value);
                        bot.Energy = (int)(bot.Energy * bonus.Value);
                        break;
                }
            }
        }
    }
    
    #endregion
}


