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

            return new SimulationResult(
                success
                    ? $"{bot.Name} successfully captured!"
                    : $"{bot.Name} failed to capture.",
                new List<string> { $"Capture attempt: {roll} (Chance: {captureChance})" },
                success ? bot : null
            );
        }
    }
}