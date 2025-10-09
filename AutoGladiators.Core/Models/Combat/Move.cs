using System;
using System.Collections.Generic;
using System.Linq;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Enums;

namespace AutoGladiators.Core.Models.Combat
{
    /// <summary>
    /// Represents a move/skill that bots can use in battle
    /// </summary>
    public class Move
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public MoveType Type { get; }
        public ElementalCore Element { get; }
        public MoveCategory Category { get; }
        
        // Battle stats
        public int BasePower { get; }
        public int Accuracy { get; } // Out of 100
        public int PP { get; } // Power Points (uses)
        public int Priority { get; } // Speed priority
        
        // Effects
        public List<MoveEffect> Effects { get; }
        public StatusEffectType? InflictedStatus { get; }
        public int StatusChance { get; } // Percentage chance to inflict status
        
        // Targeting
        public TargetType TargetType { get; }
        
        // Learning requirements
        public int LevelLearned { get; }
        public bool IsTM { get; } // Can be learned via Technical Module
        public List<BotClass> CompatibleClasses { get; }
        
        public Move(string id, string name, string description, MoveType type, ElementalCore element, 
                   MoveCategory category, int basePower, int accuracy, int pp, int priority = 0,
                   StatusEffectType? inflictedStatus = null, int statusChance = 0, 
                   TargetType targetType = TargetType.SingleEnemy, int levelLearned = 1, bool isTM = false)
        {
            Id = id;
            Name = name;
            Description = description;
            Type = type;
            Element = element;
            Category = category;
            BasePower = basePower;
            Accuracy = accuracy;
            PP = pp;
            Priority = priority;
            Effects = new List<MoveEffect>();
            InflictedStatus = inflictedStatus;
            StatusChance = statusChance;
            TargetType = targetType;
            LevelLearned = levelLearned;
            IsTM = isTM;
            CompatibleClasses = new List<BotClass>();
        }
        
        /// <summary>
        /// Calculates damage for this move
        /// </summary>
        public int CalculateDamage(GladiatorBot attacker, GladiatorBot defender)
        {
            if (Category == MoveCategory.Status) return 0;
            
            // Get attack and defense stats based on move category
            int attack = Category == MoveCategory.Physical 
                ? attacker.GetEffectiveStat(attacker.AttackPower, attacker.AttackStage)
                : attacker.GetEffectiveStat(attacker.AttackPower, attacker.SpecialAttackStage); // Using AttackPower as special attack for now
                
            int defense = Category == MoveCategory.Physical
                ? defender.GetEffectiveStat(defender.Defense, defender.DefenseStage)
                : defender.GetEffectiveStat(defender.Defense, defender.SpecialDefenseStage); // Using Defense as special defense for now
            
            // Base damage formula
            double damage = ((2 * attacker.Level + 10) / 250.0) * (attack / (double)defense) * BasePower + 2;
            
            // STAB (Same Type Attack Bonus)
            if (attacker.ElementalCore == Element)
                damage *= 1.5;
            
            return Math.Max(1, (int)damage);
        }
        
        /// <summary>
        /// Checks if move hits based on accuracy
        /// </summary>
        public bool CheckAccuracy(GladiatorBot attacker, GladiatorBot defender, Random rng)
        {
            // TODO: Factor in accuracy/evasion stages
            return rng.Next(100) < Accuracy;
        }
        
        /// <summary>
        /// Applies move effects (status, stat changes, etc.)
        /// </summary>
        public void ApplyEffects(GladiatorBot user, GladiatorBot target, Random rng)
        {
            // Apply status effect if any
            if (InflictedStatus.HasValue && rng.Next(100) < StatusChance)
            {
                target.ApplyStatus(InflictedStatus.Value, GetStatusDuration(InflictedStatus.Value));
            }
            
            // Apply custom effects
            foreach (var effect in Effects)
            {
                effect.Apply(user, target, rng);
            }
        }
        
        private int GetStatusDuration(StatusEffectType status)
        {
            return status switch
            {
                StatusEffectType.Paralysis => 4,
                StatusEffectType.Poison => 3,
                StatusEffectType.Burn => 3,
                StatusEffectType.Freeze => 2,
                StatusEffectType.Stun => 1,
                _ => 2
            };
        }
    }
    
    /// <summary>
    /// Categories of moves
    /// </summary>
    public enum MoveCategory
    {
        Physical,   // Uses Attack vs Defense
        Special,    // Uses Special Attack vs Special Defense
        Status      // No damage, applies effects
    }
    
    /// <summary>
    /// Types of moves based on what they do
    /// </summary>
    public enum MoveType
    {
        Damage,         // Pure damage
        Healing,        // Restores HP/Energy
        StatusInflict,  // Applies status effects
        StatBoost,      // Increases stats
        StatDebuff,     // Decreases stats
        Field,          // Changes battle conditions
        Utility         // Special effects
    }
    
    /// <summary>
    /// What the move can target
    /// </summary>
    public enum TargetType
    {
        SingleEnemy,    // One opponent
        AllEnemies,     // All opponents
        SingleAlly,     // One ally (including self)
        AllAllies,      // All allies
        Self,           // Only self
        Field,          // Affects the battlefield
        Any             // Any single target
    }
    
    /// <summary>
    /// Database of all moves in the game
    /// </summary>
    public static class MoveDatabase
    {
        private static readonly Dictionary<string, Move> _moves = new();
        
        static MoveDatabase()
        {
            InitializeMoves();
        }
        
        public static Move? GetMove(string id)
        {
            return _moves.TryGetValue(id, out var move) ? move : null;
        }
        
        public static List<Move> GetAllMoves()
        {
            return _moves.Values.ToList();
        }
        
        public static List<Move> GetMovesByElement(ElementalCore element)
        {
            return _moves.Values.Where(m => m.Element == element).ToList();
        }
        
        public static List<Move> GetTMMoves()
        {
            return _moves.Values.Where(m => m.IsTM).ToList();
        }
        
        private static void InitializeMoves()
        {
            // Basic attacks
            AddMove(new Move("tackle", "Tackle", "A basic physical attack", 
                MoveType.Damage, ElementalCore.None, MoveCategory.Physical, 
                40, 100, 35));
                
            AddMove(new Move("scratch", "Scratch", "Rakes the target with claws", 
                MoveType.Damage, ElementalCore.None, MoveCategory.Physical, 
                35, 100, 35));
            
            // Electric moves
            AddMove(new Move("spark", "Spark", "An electric attack that may cause paralysis", 
                MoveType.Damage, ElementalCore.Electric, MoveCategory.Special, 
                65, 100, 20, inflictedStatus: StatusEffectType.Paralysis, statusChance: 30));
                
            AddMove(new Move("thunder_bolt", "Thunder Bolt", "A powerful electric blast", 
                MoveType.Damage, ElementalCore.Electric, MoveCategory.Special, 
                90, 100, 15, inflictedStatus: StatusEffectType.Paralysis, statusChance: 10));
            
            // Fire moves
            AddMove(new Move("ember", "Ember", "A small flame that may cause burn", 
                MoveType.Damage, ElementalCore.Fire, MoveCategory.Special, 
                40, 100, 25, inflictedStatus: StatusEffectType.Burn, statusChance: 10));
                
            AddMove(new Move("flame_thrower", "Flame Thrower", "Scorches everything around it", 
                MoveType.Damage, ElementalCore.Fire, MoveCategory.Special, 
                90, 100, 15, inflictedStatus: StatusEffectType.Burn, statusChance: 10));
            
            // Water moves
            AddMove(new Move("water_gun", "Water Gun", "Squirts water to attack", 
                MoveType.Damage, ElementalCore.Water, MoveCategory.Special, 
                40, 100, 25));
                
            AddMove(new Move("hydro_pump", "Hydro Pump", "Blasts the target with a huge volume of water", 
                MoveType.Damage, ElementalCore.Water, MoveCategory.Special, 
                110, 80, 5));
            
            // Status moves
            AddMove(new Move("repair", "Repair", "Restores HP to the user", 
                MoveType.Healing, ElementalCore.None, MoveCategory.Status, 
                0, 100, 10, targetType: TargetType.Self));
                
            AddMove(new Move("harden", "Harden", "Raises the user's Defense", 
                MoveType.StatBoost, ElementalCore.None, MoveCategory.Status, 
                0, 100, 30, targetType: TargetType.Self));
                
            AddMove(new Move("screech", "Screech", "Harshly lowers the target's Defense", 
                MoveType.StatDebuff, ElementalCore.None, MoveCategory.Status, 
                0, 85, 40));
            
            // High-priority moves
            AddMove(new Move("quick_attack", "Quick Attack", "An extremely fast attack", 
                MoveType.Damage, ElementalCore.None, MoveCategory.Physical, 
                40, 100, 30, priority: 1));
            
            // Area of effect moves
            AddMove(new Move("earthquake", "Earthquake", "Hits all surrounding bots", 
                MoveType.Damage, ElementalCore.Earth, MoveCategory.Physical, 
                100, 100, 10, targetType: TargetType.AllEnemies));
        }
        
        private static void AddMove(Move move)
        {
            _moves[move.Id] = move;
        }
        
        /// <summary>
        /// Gets moves that a bot can learn at a specific level
        /// </summary>
        public static List<Move> GetMovesForLevel(BotClass botClass, int level)
        {
            return _moves.Values
                .Where(m => m.LevelLearned <= level && 
                           (m.CompatibleClasses.Count == 0 || m.CompatibleClasses.Contains(botClass)))
                .ToList();
        }
    }
    
    /// <summary>
    /// Represents a custom effect that a move can have
    /// </summary>
    public abstract class MoveEffect
    {
        public string Name { get; }
        public string Description { get; }
        
        protected MoveEffect(string name, string description)
        {
            Name = name;
            Description = description;
        }
        
        public abstract void Apply(GladiatorBot user, GladiatorBot target, Random rng);
    }
    
    /// <summary>
    /// Effect that modifies stats
    /// </summary>
    public class StatModifyEffect : MoveEffect
    {
        public StatType Stat { get; }
        public int Stages { get; }
        public bool TargetSelf { get; }
        
        public StatModifyEffect(StatType stat, int stages, bool targetSelf = false) 
            : base($"{stat} {(stages > 0 ? "Boost" : "Drop")}", 
                   $"{(stages > 0 ? "Increases" : "Decreases")} {stat} by {Math.Abs(stages)} stage(s)")
        {
            Stat = stat;
            Stages = stages;
            TargetSelf = targetSelf;
        }
        
        public override void Apply(GladiatorBot user, GladiatorBot target, Random rng)
        {
            var bot = TargetSelf ? user : target;
            
            switch (Stat)
            {
                case StatType.Attack:
                    bot.AttackStage = Math.Clamp(bot.AttackStage + Stages, -6, 6);
                    break;
                case StatType.Defense:
                    bot.DefenseStage = Math.Clamp(bot.DefenseStage + Stages, -6, 6);
                    break;
                case StatType.Speed:
                    bot.SpeedStage = Math.Clamp(bot.SpeedStage + Stages, -6, 6);
                    break;
            }
        }
    }
    
    /// <summary>
    /// Effect that heals the target
    /// </summary>
    public class HealingEffect : MoveEffect
    {
        public int Amount { get; }
        public bool IsPercentage { get; }
        
        public HealingEffect(int amount, bool isPercentage = false) 
            : base("Healing", $"Restores {amount}{(isPercentage ? "%" : "")} HP")
        {
            Amount = amount;
            IsPercentage = isPercentage;
        }
        
        public override void Apply(GladiatorBot user, GladiatorBot target, Random rng)
        {
            int healAmount = IsPercentage ? 
                (target.MaxHealth * Amount / 100) : 
                Amount;
                
            target.Heal(healAmount);
        }
    }
    
    public enum StatType
    {
        Attack,
        Defense,
        SpecialAttack,
        SpecialDefense,
        Speed,
        Accuracy,
        Evasion
    }
}
