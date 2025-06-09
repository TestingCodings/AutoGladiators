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
            "Fire", "Water", "Electric", "Steel", "Grass", "Ice", "Wind", "Earth", "Light", "Dark"
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
                CurrentHealth = 50 + level * 10,
                Energy = 100,
                Endurance = _rand.NextDouble() * 10,
                Luck = _rand.NextDouble() * 10,
                ElementalCore = element,
                CriticalHitChance = 0.1 + (_rand.NextDouble() * 0.15),
                HasOwner = false,            };

            return bot;
        }

        public static GladiatorBot CloneBot(GladiatorBot bot)
        {
            return new GladiatorBot
            {
                Name = bot.Name,
                Level = bot.Level,
                MaxHealth = bot.MaxHealth,
                CurrentHealth = bot.CurrentHealth,
                Energy = bot.Energy,
                Endurance = bot.Endurance,
                Attack = bot.Attack,
                Defense = bot.Defense,
                Speed = bot.Speed,
                Luck = bot.Luck,
                ElementalCore = bot.ElementalCore,
                CriticalHitChance = bot.CriticalHitChance,
                HasOwner = bot.HasOwner,
            };
        }
        public static GladiatorBot CreateBot(string botId, int level)
        {
            // This method can be expanded to create bots based on predefined templates or configurations
            return new GladiatorBot
            {
                Name = botId,
                Level = level,
                MaxHealth = 50 + level * 10,
                CurrentHealth = 50 + level * 10,
                Energy = 100,
                Endurance = _rand.NextDouble() * 10,
                Luck = _rand.NextDouble() * 10,
                ElementalCore = Elements[_rand.Next(Elements.Count)],
                CriticalHitChance = 0.1 + (_rand.NextDouble() * 0.15),
                HasOwner = false,
                Attack = 10 + level,
                Defense = 5 + level,
                Speed = 5 + level,
            };
        }
    }
}
