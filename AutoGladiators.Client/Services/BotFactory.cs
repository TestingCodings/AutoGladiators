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
                Endurance =  10,
                Luck =  10,
                ElementalCore = Elements[_rand.Next(Elements.Count)],
                CriticalHitChance = 1,
                HasOwner = false,            };

            return bot;
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
                Energy = 100 + level * 5,
                Endurance = 10 + level,
                Luck = 10 + level,
                ElementalCore = Elements[_rand.Next(Elements.Count)],
                CriticalHitChance = 1,
                HasOwner = false,
                Attack = 10 * level,
                Defense = 5 * level,
                Speed = 5 * level,
            };
        }
    }
}
