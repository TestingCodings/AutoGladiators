using System;
using System.Collections.Generic;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.Services
{
    public static class BotFactory
    {
        private static readonly Random _rand = new();

        private static readonly List<string> Elements = new()
        {
            "Fire", "Water", "Electric", "Steel", "Grass"
        };

        public static GladiatorBot GenerateWildBot(int playerLevel, string region)
        {
            var level = Math.Max(1, playerLevel + _rand.Next(-1, 2));
            var element = Elements[_rand.Next(Elements.Count)];

            var bot = new GladiatorBot
            {
                Name = $"WildBot_{Guid.NewGuid().ToString()[..5]}",
                Level = level,
                MaxHealth = 50 + level * 10,
                Health = 50 + level * 10,
                Energy = 100,
                Endurance = _rand.NextDouble() * 10,
                Luck = _rand.NextDouble() * 10,
                ElementalCore = element,
                CriticalHitChance = 0.1 + (_rand.NextDouble() * 0.15),
                IsWild = true,
                IsCaptured = false
            };

            return bot;
        }

        public static GladiatorBot CloneBot(GladiatorBot bot)
        {
            return new GladiatorBot
            {
                Name = bot.Name,
                Level = bot.Level,
                MaxHealth = bot.MaxHealth,
                Health = bot.Health,
                Energy = bot.Energy,
                Endurance = bot.Endurance,
                Luck = bot.Luck,
                ElementalCore = bot.ElementalCore,
                CriticalHitChance = bot.CriticalHitChance,
                IsWild = bot.IsWild,
                IsCaptured = bot.IsCaptured
            };
        }
    }
}
