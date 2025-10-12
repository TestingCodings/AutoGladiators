namespace AutoGladiators.Core.Models
{
    public class EncounterTable
    {
        public string Zone { get; set; } = string.Empty;
        public List<EncounterEntry> Encounters { get; set; } = new();
    }

    public class EncounterEntry
    {
        public string BotName { get; set; } = string.Empty;
        public double EncounterRate { get; set; } // 0.0 - 1.0

        public string Description { get; set; } = string.Empty;
        public string ElementalCore { get; set; } = string.Empty; // e.g., "Fire", "Water", "Electric"
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
        public string EncounterType { get; set; } = string.Empty; // e.g., "Wild", "Boss", "Event"
        public string EncounterId { get; set; } = string.Empty; // Unique identifier for the encounter
        public string EncounterImage { get; set; } = string.Empty; // Path to encounter image asset
        public string EncounterSound { get; set; } = string.Empty; // Path to encounter sound asset
        public string EncounterMusic { get; set; } = string.Empty; // Path to encounter music asset
        public string EncounterReward { get; set; } = string.Empty; // Description of rewards for winning the encounter
        public string EncounterFailure { get; set; } = string.Empty; // Description of consequences for losing the encounter
        public string EncounterStrategy { get; set; } = string.Empty; // Tips or strategies for players to win the encounter
        public string EncounterLore { get; set; } = string.Empty; // Background lore for the encounter
        public string EncounterLocation { get; set; } = string.Empty; // Location where the encounter takes place
        public string EncounterDifficulty { get; set; } = string.Empty; // Difficulty level of the encounter (e.g., Easy, Medium, Hard)
        public string EncounterTags { get; set; } = string.Empty; // Tags for filtering encounters (e.g., "PvE", "PvP", "Event")
        public string EncounterNotes { get; set; } = string.Empty; // Additional notes or comments about the encounter
        public string EncounterRewards { get; set; } = string.Empty; // Rewards for completing the encounter
        public string EncounterConditions { get; set; } = string.Empty; // Conditions that must be met to trigger the encounter
    }
}
