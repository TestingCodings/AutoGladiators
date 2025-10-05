using System;
using System.Collections.Generic;
using System.Linq;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services.Logging;
using Microsoft.Extensions.Logging;
using AutoGladiators.Core.Enums;

namespace AutoGladiators.Core.Services
{
    public static class BotFactory
    {
        private static readonly IAppLogger Log = AppLog.For("BotFactory");

        private static readonly Random _rand = new();
        private static Dictionary<string, BotTemplate> _botTemplates = new();

        private static ElementalCore ParseElement(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return ElementalCore.None;
            return Enum.TryParse<ElementalCore>(value, ignoreCase: true, out var e)
                ? e
                : ElementalCore.None;
        }

        // Align with your Enums.cs: None, Fire, Water, Electric, Earth, Wind, Metal, Plasma, Ice
        private static readonly List<string> Elements = new()
        {
            "Fire", "Water", "Electric", "Earth", "Wind", "Metal", "Plasma", "Ice"
        };

        public static GladiatorBot GenerateWildBot(string region, int playerLevel)
        {
            var level = Math.Max(1, playerLevel + _rand.Next(-1, 2));

            var bot = new GladiatorBot
            {
                Name = $"WildBot_{Guid.NewGuid().ToString()[..5]}",
                Level = level,
                MaxHealth = 50 + level * 10,
                CurrentHealth = 50 + level * 10,
                Energy = 100,
                Endurance = 10,
                Luck = 10,
                // FIX 1: use parser
                ElementalCore = ParseElement(Elements[_rand.Next(Elements.Count)]),
                CriticalHitChance = 1,
                HasOwner = false,
            };

            return bot;
        }

        public static GladiatorBot CreateBot(string botId, int level)
        {
            return new GladiatorBot
            {
                Name = botId,
                Level = level,
                MaxHealth = 50 + level * 10,
                CurrentHealth = 50 + level * 10,
                Energy = 100 + level * 5,
                Endurance = 10 + level,
                Luck = 10 + level,
                // FIX 2: use parser
                ElementalCore = ParseElement(Elements[_rand.Next(Elements.Count)]),
                CriticalHitChance = 1,
                HasOwner = false,
                AttackPower = 10 * level,
                Defense = 5 * level,
                Speed = 5 * level,
            };
        }

        public static void LoadBotTemplates(List<BotTemplate> templates)
        {
            _botTemplates = templates.ToDictionary(t => t.Id, t => t);
        }

        public static GladiatorBot CreateBotFromTemplate(string templateId, int level)
        {
            if (!_botTemplates.TryGetValue(templateId, out var template))
                throw new Exception($"BotTemplate '{templateId}' not found.");

            return new GladiatorBot
            {
                Name = template.Name,
                // FIX 3: template.Element is a string → parse to enum
                ElementalCore = ParseElement(template.Element),
                Level = level,
                MaxHealth = template.BaseHealth + level * 10,
                CurrentHealth = template.BaseHealth + level * 10,
                AttackPower = template.BaseAttack + level,
                Defense = template.BaseDefense + level,
                Speed = template.BaseSpeed + level,
                Luck = template.BaseLuck + level,
                CriticalHitChance = template.CriticalHitChance,
                Energy = 100,
                MaxEnergy = 100,
                Moveset = new List<string>(template.MoveIds),
                HasOwner = false
            };
        }
    }
}
