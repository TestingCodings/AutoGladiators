using AutoGladiators.Client.Core;
using System.Collections.Generic;

namespace AutoGladiators.Client.Simulation
{
    public class BattleResult
    {
        public string Winner { get; set; }
        public string Outcome { get; set; } // "Win", "Loss", "Draw", "Timeout"
        public List<string> Log { get; set; } = new();
        public int TotalRounds { get; set; }
        public Dictionary<string, int> FinalHealth { get; set; } = new();
        public Dictionary<string, GladiatorAction> LastActions { get; set; } = new();
    }
}
