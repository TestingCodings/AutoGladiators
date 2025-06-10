using AutoGladiators.Client.Models;
using AutoGladiators.Client.Core;
using System;

namespace AutoGladiators.Client.Simulation
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
                default: return new SimulationResult(summary: $"Unknown skill: {skill}");
            }

            return new SimulationResult(
                $"{bot.Name} trained {skill}, gained {gain} points!",
                bot.Name
            );
        }
    }
}