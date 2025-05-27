using AutoGladiators.Client.Core;


namespace AutoGladiators.Client.Simulation
{
    public class RaceResult
    {
        public GladiatorBot Winner { get; set; }
        public Dictionary<GladiatorBot, double> Scores { get; set; } = new();
    }
}
