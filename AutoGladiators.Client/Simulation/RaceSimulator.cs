using System.Collections.Generic;
using System.Linq;
using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.Simulation
{
    public class RaceSimulator
    {
        public RaceResult Run(List<GladiatorBot> bots)
        {
            var scores = bots.ToDictionary(bot => bot, bot =>
            {
                var speedScore = bot.Speed;
                var reactionBonus = (1000 - bot.Behavior.ReactionTime) / 100.0;
                return speedScore + reactionBonus;
            });

            var winner = scores.OrderByDescending(kv => kv.Value).First().Key;

            return new RaceResult
            {
                Winner = winner,
                Scores = scores
            };
        }
    }
}