using System;
using System.Collections.Generic;
using System.Linq;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.Services
{
    public class EncounterService
    {
        private readonly Dictionary<string, List<WildBotEncounter>> _regionEncounters;
        private readonly Random _rng = new();

        public EncounterService()
        {
            _regionEncounters = new Dictionary<string, List<WildBotEncounter>>
            {
                ["ScrapFields"] = new List<WildBotEncounter>
                {
                    new("RustyCharger", Rarity.Common, 1, 5),
                    new("ClawScout", Rarity.Uncommon, 3, 7),
                    new("SparkWolf", Rarity.Rare, 5, 10)
                },
                ["VoltanRuins"] = new List<WildBotEncounter>
                {
                    new("VoltWasp", Rarity.Common, 4, 8),
                    new("PlasmaOx", Rarity.Uncommon, 6, 10),
                    new("QuantumHydra", Rarity.Legendary, 10, 15)
                }
            };
        }

        public bool TryTriggerEncounter(out GladiatorBot encounteredBot)
        {
            // Replace with actual logic
            encounteredBot = BotFactory.GenerateWildBot(1, "Grassland");
            return true;
        }

        public bool CheckForEncounter(PlayerLocation location)
        {
            // Basic encounter logic: Can be replaced with terrain, time, weather, etc.
            return location.X % 5 == 0 && location.Y % 3 == 0;
        }

        public bool encounterTriggered(PlayerLocation location)
        {
            // Check if the player is in a region with encounters
            if (!_regionEncounters.ContainsKey(location.Region))
                return false;

            // Random chance to trigger an encounter
            return _rng.Next(100) < 20; // 20% chance
        }

        public GladiatorBot? GenerateWildBot(string region, int playerLevel)
        {
            if (!_regionEncounters.TryGetValue(region, out var encounters))
                return null;

            var eligible = encounters
                .Where(e => playerLevel >= e.MinLevel && playerLevel <= e.MaxLevel)
                .ToList();

            if (!eligible.Any()) return null;

            var weightedPool = eligible.SelectMany(e => Enumerable.Repeat(e, GetWeight(e.Rarity))).ToList();
            var chosen = weightedPool[_rng.Next(weightedPool.Count)];

            return BotFactory.CreateBot(chosen.BotId, GetBotLevel(playerLevel, chosen));
        }

        private int GetBotLevel(int playerLevel, WildBotEncounter e)
        {
            int randomOffset = _rng.Next(-1, 2); // -1, 0, or +1
            return Math.Clamp(playerLevel + randomOffset, e.MinLevel, e.MaxLevel);
        }

        private static int GetWeight(Rarity rarity) => rarity switch
        {
            Rarity.Common => 10,
            Rarity.Uncommon => 5,
            Rarity.Rare => 2,
            Rarity.Legendary => 1,
            _ => 1
        };
    }

    public enum Rarity { Common, Uncommon, Rare, Legendary }

    public class WildBotEncounter
    {
        public string BotId { get; }
        public Rarity Rarity { get; }
        public int MinLevel { get; }
        public int MaxLevel { get; }

        public WildBotEncounter(string botId, Rarity rarity, int minLevel, int maxLevel)
        {
            BotId = botId;
            Rarity = rarity;
            MinLevel = minLevel;
            MaxLevel = maxLevel;
        }
    }
}
