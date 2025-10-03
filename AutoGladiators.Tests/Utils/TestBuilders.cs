using AutoGladiators.Core.Core;
using AutoGladiators.Core.Enums;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Logic;
using AutoGladiators.Core.Rng;

namespace AutoGladiators.Tests.Utils;

public static class TestBuilders
{
    public static GladiatorBot MakeBot(
        string name = "Bot",
        int level = 10,
        int attack = 50,
        int defense = 25,
        double CriticalHitChance = 1.0,
        int speed = 40,
        int maxHp = 100,
        int energy = 100,
        ElementalCore core = ElementalCore.None)
    {
        var bot = new GladiatorBot
        {
            Name = name,
            Level = level,
            AttackPower = attack,  // Using AttackPower property
            Strength = attack,     // Keep Strength for compatibility
            Defense = defense,
            CriticalHitChance = CriticalHitChance,
            Speed = speed,
            MaxHealth = maxHp,
            CurrentHealth = maxHp,  // Start at full health
            Energy = energy,
            MaxEnergy = energy,
            ElementalCore = core,
            IsFainted = false      // Explicitly initialize as not fainted
        };
        
        // Apply level scaling to reflect the proper stats for the level
        bot.ApplyLevelScaling();
        
        // If maxHp was explicitly set, override the scaled value
        if (maxHp != 100) // 100 is the default
        {
            bot.MaxHealth = maxHp;
        }
        
        bot.CurrentHealth = bot.MaxHealth; // Ensure full health after scaling
        
        return bot;
    }

    public static AutoGladiators.Core.Models.Move MakeMove(
        string name = "Move",
        int basePower = 20,
        double accuracy = 1.0,
        double CriticalHitChance = 5.0,
        int energyCost = 0,
        int priority = 0,
        AutoGladiators.Core.Enums.ElementalCore element = AutoGladiators.Core.Enums.ElementalCore.None,
        AutoGladiators.Core.Enums.MoveCategory category = AutoGladiators.Core.Enums.MoveCategory.Attack)
    {
        return new AutoGladiators.Core.Models.Move
        {
            Name = name,
            Power = basePower,
            Accuracy = accuracy,
            EnergyCost = energyCost,
            Priority = priority,
            Element = element.ToString(),
            Category = category,
            Type = AutoGladiators.Core.Enums.MoveType.Attack
        };
    }

    public static (GladiatorBot User, Move Move) Choice(GladiatorBot user, Move move) => (user, move);
}
