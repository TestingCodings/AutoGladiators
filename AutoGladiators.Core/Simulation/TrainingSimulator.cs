using AutoGladiators.Core.Models;
using AutoGladiators.Core.Core;
using System;

namespace AutoGladiators.Core.Simulation
{
    public static class TrainingSimulator
    {
        public static SimulationResult SimulateTraining(GladiatorBot bot, string skill)
        {
            int gain = new Random().Next(1, 4);
            switch (skill.ToLower())
            {
                case "attack": bot.AttackPower += gain; break;
                case "defense": bot.Defense += gain; break;
                case "speed": bot.Speed += gain; break;
                default: return new SimulationResult(bot, $"Unknown skill: {skill}");
            }

            return new SimulationResult(
                bot,
                $"{bot.Name} trained {skill}, gained {gain} points!"
            );
        }
    }
}
