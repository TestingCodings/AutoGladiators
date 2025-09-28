using System.Collections.Generic;

namespace AutoGladiators.Core.Models
{
    public class BotTemplate
    {
        public string Id { get; set; }                // Unique internal ID (e.g., "VoltWasp")
        public string Name { get; set; }              // Display name
        public string Element { get; set; }           // ElementalCore (Fire, Water, etc.)
        
        public int BaseHealth { get; set; }           // Base MaxHealth
        public int BaseAttack { get; set; }           // Base AttackPower
        public int BaseDefense { get; set; }          // Base Defense
        public int BaseSpeed { get; set; }            // Base Speed
        public int BaseLuck { get; set; }             // Base Luck
        public double CriticalHitChance { get; set; }        // Base CriticalHitChance

        public List<string> MoveIds { get; set; }     // List of move IDs this bot knows
        public string Description { get; set; }       // Flavor text or lore
    }
}
