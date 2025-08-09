using System;
using System.Collections.Generic;
using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.Services;

public class FusionService
{
    public static FusionResult TryFuseBots(GladiatorBot bot1, GladiatorBot bot2)
    {
        if (bot1 == null || bot2 == null)
            throw new ArgumentNullException("Both bots must be provided for fusion.");

        if (bot1.Id == bot2.Id)
            return new FusionResult(false, "Cannot fuse the same bot.");

        // Simple fusion logic: average stats and create a new bot
        var fusedBot = new GladiatorBot
        {
            Id = Guid.NewGuid(),
            Name = $"{bot1.Name}-{bot2.Name} Fusion",
            AttackPower = (bot1.AttackPower + bot2.AttackPower) / 2,
            Defense = (bot1.Defense + bot2.Defense) / 2,
            Speed = (bot1.Speed + bot2.Speed) / 2,
            Level = Math.Max(bot1.Level, bot2.Level) + 1
        };

        return new FusionResult(true, "Fusion successful!", fusedBot);
    }
    public class FusionResult
    {
        public bool Success { get; }
        public string Message { get; }
        public GladiatorBot FusedBot { get; }

        public FusionResult(bool success, string message, GladiatorBot fusedBot = null)
        {
            Success = success;
            Message = message;
            FusedBot = fusedBot;
        }
        public void DisplayResult()
        {
            Console.WriteLine($"Fusion Result: {Message}");
            if (Success && FusedBot != null)
            {
                Console.WriteLine($"New Bot Created: {FusedBot.Name} (Level {FusedBot.Level})");
            }
        }
    }
}