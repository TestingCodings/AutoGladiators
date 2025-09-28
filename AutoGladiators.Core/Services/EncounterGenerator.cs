using AutoGladiators.Core.Models;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Core.Services
{
    public static class EncounterGenerator
    {
        private static readonly Microsoft.Extensions.Logging.ILogger Log = (Microsoft.Extensions.Logging.ILogger)AppLog.For("EncounterGenerator");

        private static readonly EncounterService _encounterService = new();

        private static readonly Random _rng = new();

        // 40% chance to trigger an encounter
        public static bool ShouldTriggerEncounter()
        {
            double roll = _rng.NextDouble();
            Log.LogInformation($"Encounter roll: {roll:F2}");
            return roll < 0.4;
        }

        // Returns a simple wild bot (uses BotFactory if available, else fallback)
        public static GladiatorBot GenerateWildBot()
        {
            var playerLevel = GameStateService.Instance.CurrentPlayer?.Level ?? 1;
            // Use BotFactory if available
            var bot = BotFactory.CreateBot("WildBot", playerLevel); // "WildBot" is a placeholder
            if (bot == null)
            {
                bot = new GladiatorBot
                {
                    Id = _rng.Next(1000, 9999),
                    Name = "WildBot",
                    Level = playerLevel,
                    HP = 20 + playerLevel * 2,
                    Moveset = new System.Collections.Generic.List<string> { "Jab" }
                };
            }
            Log.LogInformation($"Generated wild bot: {bot.Name} (Lv{bot.Level})");
            return bot;
        }
    }
}

