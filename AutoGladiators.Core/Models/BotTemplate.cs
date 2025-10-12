using System.Collections.Generic;

namespace AutoGladiators.Core.Models
{
    public class BotTemplate
    {
        public string Id { get; set; } = string.Empty;                // Unique internal ID (e.g., "VoltWasp")
        public string Name { get; set; } = string.Empty;              // Display name
        public string Element { get; set; } = string.Empty;           // ElementalCore (Fire, Water, etc.)
        
        public int BaseHealth { get; set; }           // Base MaxHealth
        public int BaseAttack { get; set; }           // Base AttackPower
        public int BaseDefense { get; set; }          // Base Defense
        public int BaseSpeed { get; set; }            // Base Speed
        public int BaseLuck { get; set; }             // Base Luck
        public double CriticalHitChance { get; set; }        // Base CriticalHitChance

        public List<string> MoveIds { get; set; } = new();     // List of move IDs this bot knows
        public string Description { get; set; } = string.Empty;       // Flavor text or lore
    }
}
