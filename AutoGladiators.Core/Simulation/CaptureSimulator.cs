using AutoGladiators.Core.Core;
using AutoGladiators.Core.Models;
using System;

namespace AutoGladiators.Core.Simulation
{
    public static class CaptureSimulator
    {
        public static SimulationResult TryCapture(GladiatorBot bot, int captureChance)
        {
            int roll = new Random().Next(0, 100);
            bool success = roll < captureChance;

            var result = new SimulationResult();
            result.Outcome = success
                ? $"{bot.Name} successfully captured!"
                : $"{bot.Name} failed to capture.";
            result.Log = new List<string> { $"Capture attempt: {roll} (Chance: {captureChance})" };
            result.CapturedBot = success ? bot : null;
            return result;
        }
    }
}
