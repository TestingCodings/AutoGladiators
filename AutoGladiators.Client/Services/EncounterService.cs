using System;
using System.Collections.Generic;
using System.Linq;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.Tables;


namespace AutoGladiators.Client.Services
{
    public class EncounterService
    {
        private readonly Dictionary<string, List<WildBotEncounter>> _regionEncounters;

        public EncounterService()
        {
            // Predefined encounters by region and rarity
            _regionEncounters = new Dictionary<string, List<WildBotEncounter>>
            {
                ["ScrapFields"] = new List<WildBotEncounter>
                {
                    new WildBotEncounter("RustyCharger", Rarity.Common, 1, 5),
                    new WildBotEncounter("ClawScout", Rarity.Uncommon, 3, 7),
                    new WildBotEncounter("SparkWolf", Rarity.Rare, 5, 10)
                },
                ["VoltanRuins"] = new List<WildBotEncounter>
                {
                    new WildBotEncounter("VoltWasp", Rarity.Common, 4, 8),
                    new WildBotEncounter("PlasmaOx", Rarity.Uncommon, 6, 10),
                    new WildBotEncounter("QuantumHydra", Rarity.Legendary, 10, 15)
                }
            };
        }

        public GladiatorBot GenerateWildBot(string region, int playerLevel)
        {
            if (!_regionEncounters.ContainsKey(region))
                throw new ArgumentException("Invalid region");

            var pool = _regionEncounters[region]
                .Where(e => playerLevel >= e.MinLevel && playerLevel <= e.MaxLevel)
                .ToList();

            if (!pool.Any()) return null;

            var weightedPool = pool.SelectMany(e => Enumerable.Repeat(e, GetWeight(e.Rarity))).ToList();
            var chosen = weightedPool[new Random().Next(weightedPool.Count)];

            return BotFactory.CreateBot(chosen.BotId, GetBotLevel(playerLevel, chosen));
        }

        private int GetWeight(Rarity rarity) => rarity switch
        {
            Rarity.Common => 10,
            Rarity.Uncommon => 5,
            Rarity.Rare => 2,
            Rarity.Legendary => 1,
            _ => 1
        };

        private int GetBotLevel(int playerLevel, WildBotEncounter e) =>
            Math.Clamp(playerLevel + new Random().Next(-1, 2), e.MinLevel, e.MaxLevel);
    }

    public enum Rarity { Common, Uncommon, Rare, Legendary }

    public class WildBotEncounter
    {
        public string BotId;
        public Rarity Rarity;
        public int MinLevel;
        public int MaxLevel;

        public WildBotEncounter(string botId, Rarity rarity, int minLevel, int maxLevel)
        {
            BotId = botId;
            Rarity = rarity;
            MinLevel = minLevel;
            MaxLevel = maxLevel;
        }
    }
}
