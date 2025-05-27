using System;
using System.Collections.Generic;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.Config;


namespace AutoGladiators.Core.Simulation
{
    public class RaceSimulator
    {
        public GladiatorBot SimulateRace(List<GladiatorBot> bots)
        {
            var scores = bots.ToDictionary(bot => bot, bot =>
            {
                var speedScore = bot.Speed;
                var reactionBonus = (1000 - bot.Behavior.ReactionTime) / 100.0;
                return speedScore + reactionBonus;
            });

            return scores.OrderByDescending(kv => kv.Value).First().Key;
        }
    }
}
