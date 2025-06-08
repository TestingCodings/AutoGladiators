namespace AutoGladiators.Client.Models
{
    public class EncounterTable
    {
        public string Zone { get; set; }
        public List<EncounterEntry> Encounters { get; set; }
    }

    public class EncounterEntry
    {
        public string BotName { get; set; }
        public double EncounterRate { get; set; } // 0.0 - 1.0

        public string Description { get; set; }
        public string ElementalCore { get; set; } // e.g., "Fire", "Water", "Electric"
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
        public string EncounterType { get; set; } // e.g., "Wild", "Boss", "Event"
        public string EncounterId { get; set; } // Unique identifier for the encounter
        public string EncounterImage { get; set; } // Path to encounter image asset
        public string EncounterSound { get; set; } // Path to encounter sound asset
        public string EncounterMusic { get; set; } // Path to encounter music asset
        public string EncounterReward { get; set; } // Description of rewards for winning the encounter
        public string EncounterFailure { get; set; } // Description of consequences for losing the encounter
        public string EncounterStrategy { get; set; } // Tips or strategies for players to win the encounter
        public string EncounterLore { get; set; } // Background lore for the encounter
        public string EncounterLocation { get; set; } // Location where the encounter takes place
        public string EncounterDifficulty { get; set; } // Difficulty level of the encounter (e.g., Easy, Medium, Hard)
        public string EncounterTags { get; set; } // Tags for filtering encounters (e.g., "PvE", "PvP", "Event")
        public string EncounterNotes { get; set; } // Additional notes or comments about the encounter
        public string EncounterRewards { get; set; } // Rewards for completing the encounter
        public string EncounterConditions { get; set; } // Conditions that must be met to trigger the encounter
    }
}