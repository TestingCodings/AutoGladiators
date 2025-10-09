using System;
using System.Collections.Generic;
using AutoGladiators.Core.Enums;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Services.Exploration;

namespace AutoGladiators.Core.Models.Exploration
{
    /// <summary>
    /// Represents environmental data for a biome/region
    /// </summary>
    public class BiomeData
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double BaseEncounterRate { get; set; } = 0.1;
        public List<WildBotEncounter> WildBots { get; set; } = new();
        
        // Time-based encounter modifiers
        public Dictionary<TimeOfDay, double> TimeModifiers { get; set; } = new();
        
        // Weather-based encounter modifiers
        public Dictionary<WeatherCondition, double> WeatherModifiers { get; set; } = new();
        
        // Special conditions
        public List<string> SpecialConditions { get; set; } = new();
        public Dictionary<string, List<string>> VisibleEncounters { get; set; } = new(); // Position -> Bot IDs
        
        // Environmental hazards
        public List<EnvironmentalHazard> Hazards { get; set; } = new();
        
        /// <summary>
        /// Gets the current encounter rate based on time and weather
        /// </summary>
        public double GetCurrentEncounterRate(TimeOfDay timeOfDay, WeatherCondition weather)
        {
            double rate = BaseEncounterRate;
            
            if (TimeModifiers.ContainsKey(timeOfDay))
                rate *= TimeModifiers[timeOfDay];
                
            if (WeatherModifiers.ContainsKey(weather))
                rate *= WeatherModifiers[weather];
                
            return Math.Min(1.0, Math.Max(0.0, rate)); // Clamp between 0 and 1
        }
        
        /// <summary>
        /// Gets encounter pool filtered by current conditions
        /// </summary>
        public List<WildBotEncounter> GetFilteredEncounters(TimeOfDay timeOfDay, WeatherCondition weather)
        {
            var filtered = new List<WildBotEncounter>();
            
            foreach (var encounter in WildBots)
            {
                // Check time preferences
                if (encounter.PreferredTimes.Count > 0 && !encounter.PreferredTimes.Contains(timeOfDay))
                {
                    // Reduce chance but don't eliminate completely
                    if (new Random().NextDouble() > 0.3) continue;
                }
                
                // Check weather preferences
                if (encounter.PreferredWeather.Count > 0 && !encounter.PreferredWeather.Contains(weather))
                {
                    if (new Random().NextDouble() > 0.2) continue;
                }
                
                filtered.Add(encounter);
            }
            
            return filtered.Count > 0 ? filtered : WildBots; // Fallback to all if none match
        }
    }
    
    /// <summary>
    /// Time of day affecting encounters and bot behavior
    /// </summary>
    public enum TimeOfDay
    {
        Dawn,    // 5-7 AM
        Day,     // 7 AM - 6 PM
        Dusk,    // 6-8 PM
        Night    // 8 PM - 5 AM
    }
    
    /// <summary>
    /// Weather conditions affecting encounters
    /// </summary>
    public enum WeatherCondition
    {
        Clear,
        Rain,
        Storm,
        Snow,
        Blizzard,
        Fog,
        Sandstorm,
        ExtremeHeat,
        ElectricStorm,
        AcidRain,
        ToxicFog
    }
    
    /// <summary>
    /// Environmental hazards that can affect exploration or battles
    /// </summary>
    public class EnvironmentalHazard
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public HazardType Type { get; set; }
        public int Damage { get; set; } // Per turn or per trigger
        public StatusEffectType? StatusEffect { get; set; }
        public double TriggerChance { get; set; } = 1.0; // 0-1 probability
        public List<ElementalCore> AffectedTypes { get; set; } = new(); // Empty = affects all
        
        /// <summary>
        /// Checks if hazard affects the given bot
        /// </summary>
        public bool Affects(GladiatorBot bot)
        {
            if (AffectedTypes.Count == 0) return true;
            return AffectedTypes.Contains(bot.ElementalCore);
        }
    }
    
    /// <summary>
    /// Types of environmental hazards
    /// </summary>
    public enum HazardType
    {
        Damage,         // Direct HP damage
        Status,         // Applies status effect
        Movement,       // Restricts movement
        StatDebuff,     // Temporary stat reduction
        EnergyDrain,    // Reduces energy/PP
        VisibilityReduction // Affects accuracy
    }
}