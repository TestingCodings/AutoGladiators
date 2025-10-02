using System;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services.Logging;
using AutoGladiators.Core.Services.Analytics;

namespace AutoGladiators.Core.Services
{
    public interface IBotProgressionService
    {
        LevelUpResult TryLevelUp(GladiatorBot bot);
        int GetXpRequirement(int currentLevel);
        int GetXpForNextLevel(GladiatorBot bot);
        double GetLevelProgress(GladiatorBot bot);
        StatGrowth CalculateStatGrowth(GladiatorBot bot, int levelsGained);
    }

    public class BotProgressionService : IBotProgressionService
    {
        private static readonly IAppLogger _logger = AppLog.For<BotProgressionService>();
        private static readonly IBattleAnalytics _analytics = new BattleAnalytics();

        public LevelUpResult TryLevelUp(GladiatorBot bot)
        {
            var result = new LevelUpResult { Bot = bot };
            int levelsGained = 0;

            while (bot.Experience >= GetXpRequirement(bot.Level))
            {
                bot.Experience -= GetXpRequirement(bot.Level);
                bot.Level++;
                levelsGained++;
            }

            if (levelsGained > 0)
            {
                var statGrowth = CalculateStatGrowth(bot, levelsGained);
                ApplyStatGrowth(bot, statGrowth);

                result.LevelsGained = levelsGained;
                result.StatGrowth = statGrowth;
                result.NewLevel = bot.Level;

                // Log level up analytics
                _analytics.LogPlayerLevelUp(bot.Id.ToString(), bot.Level, bot.Experience);
                
                _logger.LogUserAction("BotLevelUp", bot.Id.ToString(), new Dictionary<string, object?>
                {
                    ["BotName"] = bot.Name,
                    ["NewLevel"] = bot.Level,
                    ["LevelsGained"] = levelsGained,
                    ["StatGrowth"] = statGrowth
                });
            }

            return result;
        }

        public int GetXpRequirement(int currentLevel)
        {
            // Exponential growth: base 100, increases by 25% per level
            return (int)(100 * Math.Pow(1.25, currentLevel - 1));
        }

        public int GetXpForNextLevel(GladiatorBot bot)
        {
            return GetXpRequirement(bot.Level) - bot.Experience;
        }

        public double GetLevelProgress(GladiatorBot bot)
        {
            int required = GetXpRequirement(bot.Level);
            return (double)bot.Experience / required;
        }

        public StatGrowth CalculateStatGrowth(GladiatorBot bot, int levelsGained)
        {
            // Growth rates based on elemental core and level
            var baseGrowth = GetBaseStatGrowth(bot.ElementalCore);
            var levelMultiplier = Math.Min(2.0, 1.0 + (bot.Level * 0.02)); // Slight scaling bonus

            return new StatGrowth
            {
                Health = (int)(baseGrowth.Health * levelsGained * levelMultiplier),
                AttackPower = (int)(baseGrowth.AttackPower * levelsGained * levelMultiplier),
                Defense = (int)(baseGrowth.Defense * levelsGained * levelMultiplier),
                Speed = (int)(baseGrowth.Speed * levelsGained * levelMultiplier),
                Energy = (int)(baseGrowth.Energy * levelsGained * levelMultiplier),
                Luck = Math.Max(0, levelsGained / 3) // Luck grows slower
            };
        }

        private void ApplyStatGrowth(GladiatorBot bot, StatGrowth growth)
        {
            bot.MaxHealth += growth.Health;
            bot.CurrentHealth += growth.Health; // Heal on level up
            bot.AttackPower += growth.AttackPower;
            bot.Defense += growth.Defense;
            bot.Speed += growth.Speed;
            bot.MaxEnergy += growth.Energy;
            bot.Energy = bot.MaxEnergy; // Restore energy on level up
            bot.Luck += growth.Luck;

            // Update derived stats
            bot.CriticalHitChance = Math.Min(25.0, 5.0 + (bot.Luck * 0.5)); // Max 25% crit
        }

        private StatGrowth GetBaseStatGrowth(AutoGladiators.Core.Enums.ElementalCore element)
        {
            // Different elements have different growth patterns
            return element switch
            {
                AutoGladiators.Core.Enums.ElementalCore.Fire => new StatGrowth { Health = 8, AttackPower = 4, Defense = 2, Speed = 3, Energy = 3 },
                AutoGladiators.Core.Enums.ElementalCore.Water => new StatGrowth { Health = 10, AttackPower = 2, Defense = 3, Speed = 2, Energy = 5 },
                AutoGladiators.Core.Enums.ElementalCore.Electric => new StatGrowth { Health = 6, AttackPower = 3, Defense = 2, Speed = 5, Energy = 4 },
                AutoGladiators.Core.Enums.ElementalCore.Earth => new StatGrowth { Health = 12, AttackPower = 2, Defense = 4, Speed = 1, Energy = 3 },
                AutoGladiators.Core.Enums.ElementalCore.Wind => new StatGrowth { Health = 7, AttackPower = 3, Defense = 2, Speed = 4, Energy = 4 },
                AutoGladiators.Core.Enums.ElementalCore.Metal => new StatGrowth { Health = 9, AttackPower = 3, Defense = 4, Speed = 2, Energy = 2 },
                AutoGladiators.Core.Enums.ElementalCore.Ice => new StatGrowth { Health = 9, AttackPower = 2, Defense = 3, Speed = 3, Energy = 3 },
                AutoGladiators.Core.Enums.ElementalCore.Plasma => new StatGrowth { Health = 8, AttackPower = 4, Defense = 2, Speed = 3, Energy = 5 },
                _ => new StatGrowth { Health = 8, AttackPower = 3, Defense = 3, Speed = 3, Energy = 3 } // Balanced growth
            };
        }
    }

    public class LevelUpResult
    {
        public GladiatorBot Bot { get; set; } = null!;
        public int LevelsGained { get; set; }
        public int NewLevel { get; set; }
        public StatGrowth StatGrowth { get; set; } = new();
        
        public bool HasLeveledUp => LevelsGained > 0;
    }

    public class StatGrowth
    {
        public int Health { get; set; }
        public int AttackPower { get; set; }
        public int Defense { get; set; }
        public int Speed { get; set; }
        public int Energy { get; set; }
        public int Luck { get; set; }

        public override string ToString()
        {
            var parts = new List<string>();
            if (Health > 0) parts.Add($"+{Health} HP");
            if (AttackPower > 0) parts.Add($"+{AttackPower} ATK");
            if (Defense > 0) parts.Add($"+{Defense} DEF");
            if (Speed > 0) parts.Add($"+{Speed} SPD");
            if (Energy > 0) parts.Add($"+{Energy} EN");
            if (Luck > 0) parts.Add($"+{Luck} LCK");
            
            return string.Join(", ", parts);
        }
    }
}