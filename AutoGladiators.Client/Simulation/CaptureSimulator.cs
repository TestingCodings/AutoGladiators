using AutoGladiators.Client.Core;
using AutoGladiators.Client.Models;
using System;

namespace AutoGladiators.Client.Simulation
{
    public static class CaptureSimulator
    {
        public static SimulationResult TryCapture(GladiatorBot bot, int captureChance)
        {
            int roll = new Random().Next(0, 100);
            bool success = roll < captureChance;

            return new SimulationResult
            {
                Summary = success ? $"Successfully captured {bot.Name}!" : $"{bot.Name} escaped!",
                WinnerBot = success ? bot.Name : null
            };
        }
    }
}