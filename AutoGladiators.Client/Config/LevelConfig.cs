using System.Collections.Generic;


namespace AutoGladiators.Client.Config
{
    public class LevelConfig
    {
        public string LevelName { get; set; }
        public string Mode { get; set; } // "Battle", "Race", "Training"
        public string Environment { get; set; }

        public List<string> Modifiers { get; set; }
        public BotSettings BotSettings { get; set; }
        public LevelRules Rules { get; set; }
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
        public string VictoryCondition { get; set; }
    }
}
