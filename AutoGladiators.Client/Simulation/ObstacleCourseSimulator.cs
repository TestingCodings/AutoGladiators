using System;
using System.Collections.Generic;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.Config;


namespace AutoGladiators.Client.Simulation
{
    public class ObstacleCourseSimulator
    {
        public GladiatorBot SimulateCourse(List<GladiatorBot> bots)
        {
            var scores = bots.ToDictionary(bot => bot, bot =>
            {
                double agility = bot.Agility;
                double decisionMaking = bot.Behavior.Intelligence;
                double adaptability = bot.Behavior.Adaptability;

                return agility * 0.5 + decisionMaking * 0.3 + adaptability * 0.2;
            });

            return scores.OrderByDescending(kv => kv.Value).First().Key;
        }
    }
}
