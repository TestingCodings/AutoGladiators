using System.Collections.Generic;


namespace AutoGladiators.Client.Config
{
    public class LevelConfig
    {
        public string LevelName { get; set; } = string.Empty;
        public string Mode { get; set; } = string.Empty; // "Battle", "Race", "Training"
        public string Environment { get; set; } = string.Empty;

        public List<string> Modifiers { get; set; } = new();
        public BotSettings BotSettings { get; set; } = new();
        public LevelRules Rules { get; set; } = new();
    }

    public class BotSettings
    {
        public int MaxHealth { get; set; }
        public int StartEnergy { get; set; }
    }

    public class LevelRules
    {
        public bool AllowPowerStrikes { get; set; }
        public bool EnableEvade { get; set; }
        public int MaxTurns { get; set; }
        public string VictoryCondition { get; set; } = string.Empty;
    }
}
