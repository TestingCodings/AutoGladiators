using System;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Core.Services
{
    public class FusionService
    {
        private static readonly Microsoft.Extensions.Logging.ILogger Log = (Microsoft.Extensions.Logging.ILogger)AppLog.For<FusionService>();

        public static FusionResult TryFuseBots(GladiatorBot bot1, GladiatorBot bot2)
        {
            if (bot1 == null || bot2 == null)
                throw new ArgumentNullException("Both bots must be provided for fusion.");

            if (Equals(bot1.Id, bot2.Id))
                return new FusionResult(false, "Cannot fuse the same bot.");

            // Simple fusion logic: average stats, make a new bot
            var fusedBot = new GladiatorBot
            {
                // If Id must be unique int, derive one deterministically from names (or use your factory)
                Id = Math.Abs($"{bot1.Name}-{bot2.Name}-{DateTime.UtcNow.Ticks}".GetHashCode()),
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
        private static readonly Microsoft.Extensions.Logging.ILogger Log = (Microsoft.Extensions.Logging.ILogger)AppLog.For<FusionService>();

            public bool Success { get; }
            public string Message { get; }
            public GladiatorBot? FusedBot { get; }

            public FusionResult(bool success, string message, GladiatorBot? fusedBot = null)
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
}

