using AutoGladiators.Core.Models;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Core.Services
{
    public class EncounterGenerator
    {
        private static readonly IAppLogger Log = AppLog.For<EncounterGenerator>();
        private readonly Random _rng = new();

        // MVP: Always generates an encounter for the core loop
        public GladiatorBot? GenerateWildEncounter(string location)
        {
            var playerLevel = GameStateService.Instance.CurrentPlayer?.Level ?? 1;
            
            // Create deterministic enemy bots based on location
            var enemyTemplates = new[]
            {
                new { Name = "Scrapling", Element = AutoGladiators.Core.Enums.ElementalCore.Metal, Hp = 60 },
                new { Name = "Rustbeast", Element = AutoGladiators.Core.Enums.ElementalCore.Fire, Hp = 70 },
                new { Name = "Voltaic Drone", Element = AutoGladiators.Core.Enums.ElementalCore.Electric, Hp = 55 }
            };
            
            var template = enemyTemplates[_rng.Next(enemyTemplates.Length)];
            
            var enemy = new GladiatorBot
            {
                Id = _rng.Next(1000, 9999),
                Name = template.Name,
                Level = Math.Max(1, playerLevel + _rng.Next(-1, 2)), // ±1 level variance
                ElementalCore = template.Element,
                Description = $"A wild {template.Name} encountered in {location}",
                
                // Combat stats
                MaxHealth = template.Hp + (playerLevel * 5),
                CurrentHealth = template.Hp + (playerLevel * 5),
                AttackPower = 15 + (playerLevel * 2),
                Defense = 10 + playerLevel,
                Speed = 8 + _rng.Next(0, 5),
                MaxEnergy = 100 + (playerLevel * 5),
                Energy = 100 + (playerLevel * 5),
                
                // Basic moves
                Moveset = new System.Collections.Generic.List<string> { "Tackle", "Guard" },
                LearnableMoves = new System.Collections.Generic.List<string> { "Tackle", "Guard", "Power Strike" }
            };
            
            Log.Info($"Generated wild encounter: {enemy.Name} (Lv{enemy.Level}) in {location}");
            return enemy;
        }
    }
}

