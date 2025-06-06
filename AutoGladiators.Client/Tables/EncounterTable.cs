namespace AutoGladiators.Client.Tables;    
    public class EncounterTable
    {
        public string Zone { get; set; }
        public List<EncounterEntry> Encounters { get; set; }
    }

    public class EncounterEntry
    {
        public string BotName { get; set; }
        public double EncounterRate { get; set; } // 0.0 - 1.0
    }
        public string Description { get; set; }
        public string ElementalCore { get; set; } // e.g., "Fire", "Water", "Electric"
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
