using AutoGladiators.Core.Core;
using AutoGladiators.Core.Enums;
using AutoGladiators.Core.Rng;
using System;

namespace AutoGladiators.Core.Models
{
    public enum MoveTier
    {
        Basic,      // No MP cost, always available
        Advanced,   // Low MP cost (5-15 MP)
        Special,    // Medium MP cost (20-35 MP)
        Ultimate    // High MP cost (40+ MP), requires unlock conditions
    }

    public class EnhancedMove : Move
    {
        public int MpCost { get; set; }
        public MoveTier Tier { get; set; }
        public int MinLevel { get; set; } = 1;
        public int UsesPerBattle { get; set; } = -1; // -1 = unlimited
        public int RemainingUses { get; set; } = -1;
        public bool RequiresComboSetup { get; set; } = false;
        public string? ComboRequirement { get; set; } // Move name that must be used first
        public bool IsUnlocked { get; set; } = true;

        // Enhanced visual and audio feedback
        public string? AnimationEffect { get; set; }
        public string? SoundEffect { get; set; }
        public string? ParticleEffect { get; set; }

        public EnhancedMove()
        {
            RemainingUses = UsesPerBattle;
        }

        public bool CanUse(GladiatorBot attacker)
        {
            // Check MP
            if (attacker.CurrentMP < MpCost)
                return false;

            // Check if unlocked
            if (!IsUnlocked)
                return false;

            // Check level requirement
            if (attacker.Level < MinLevel)
                return false;

            // Check remaining uses
            if (UsesPerBattle > 0 && RemainingUses <= 0)
                return false;

            // Check energy cost (from base Move class)
            if (attacker.Energy < EnergyCost)
                return false;

            return true;
        }

        public new string Use(GladiatorBot attacker, GladiatorBot defender)
        {
            if (!CanUse(attacker))
            {
                if (attacker.CurrentMP < MpCost)
                    return $"{attacker.Name} doesn't have enough MP to use {Name}! (Need {MpCost} MP)";
                if (!IsUnlocked)
                    return $"{Name} is locked! Meet the requirements to unlock it.";
                if (attacker.Level < MinLevel)
                    return $"{attacker.Name} needs to reach level {MinLevel} to use {Name}!";
                if (UsesPerBattle > 0 && RemainingUses <= 0)
                    return $"{Name} cannot be used again this battle!";
                
                return $"{attacker.Name} cannot use {Name} right now.";
            }

            // Consume MP and uses
            attacker.CurrentMP -= MpCost;
            if (UsesPerBattle > 0)
                RemainingUses--;

            // Use base energy cost as well
            attacker.UseEnergy(EnergyCost);

            var rng = new DefaultRng();
            if (!CheckAccuracy(rng))
                return $"{attacker.Name} used {Name}, but it missed!";

            int damage = 0;
            string effectDescription = "";

            if (Type == MoveType.Attack)
            {
                damage = CalculateEnhancedDamage(attacker, defender, rng);
                effectDescription = ApplySpecialEffects(attacker, defender, rng);
            }
            else if (Type == MoveType.Defense || Category == MoveCategory.Buff)
            {
                effectDescription = ApplyBuffEffects(attacker, rng);
            }

            if (damage > 0)
            {
                defender.ReceiveDamage(damage);
            }

            string result = $"{attacker.Name} used {GetMoveDisplayName()}!";
            
            if (damage > 0)
            {
                result += $" It dealt {damage} damage to {defender.Name}!";
                if (defender.CurrentHealth <= 0)
                {
                    result += $" {defender.Name} has been defeated!";
                }
            }

            if (!string.IsNullOrEmpty(effectDescription))
            {
                result += $" {effectDescription}";
            }

            return result;
        }

        private int CalculateEnhancedDamage(GladiatorBot attacker, GladiatorBot defender, IRng rng)
        {
            // Enhanced damage calculation based on tier
            double tierMultiplier = Tier switch
            {
                MoveTier.Basic => 1.0,
                MoveTier.Advanced => 1.3,
                MoveTier.Special => 1.7,
                MoveTier.Ultimate => 2.2,
                _ => 1.0
            };

            int attackPower = attacker.AttackPower;
            int defense = defender.Defense;
            
            // Base damage with tier scaling
            double baseDamage = (attackPower + Power) * 0.7 * tierMultiplier;
            
            // Defense reduces damage
            baseDamage = Math.Max(baseDamage - (defense * 0.5), baseDamage * 0.1);
            
            // Apply random factor (85% to 115% for more variance)
            double randomFactor = rng.Next(85, 116) / 100.0;
            baseDamage *= randomFactor;
            
            // Enhanced critical hit system based on tier
            double critChance = Tier switch
            {
                MoveTier.Basic => 0.05,      // 5%
                MoveTier.Advanced => 0.10,   // 10%
                MoveTier.Special => 0.15,    // 15%
                MoveTier.Ultimate => 0.25,   // 25%
                _ => 0.05
            };

            if (rng.NextDouble() < critChance)
            {
                baseDamage *= 2.0; // Critical hits do 2x damage for enhanced moves
            }

            return Math.Max(1, (int)baseDamage);
        }

        private string ApplySpecialEffects(GladiatorBot attacker, GladiatorBot defender, IRng rng)
        {
            var effects = new List<string>();

            // Status effect application
            if (InflictsStatus.HasValue && rng.NextDouble() < (StatusChance / 100.0))
            {
                // Apply status effect (simplified for now)
                effects.Add($"{defender.Name} is affected by {InflictsStatus}!");
            }

            // Tier-based special effects
            switch (Tier)
            {
                case MoveTier.Special:
                    if (rng.NextDouble() < 0.3) // 30% chance
                    {
                        effects.Add("The attack resonates with special energy!");
                    }
                    break;

                case MoveTier.Ultimate:
                    if (rng.NextDouble() < 0.5) // 50% chance
                    {
                        int bonusDamage = Power / 4;
                        defender.ReceiveDamage(bonusDamage);
                        effects.Add($"Ultimate power surge deals {bonusDamage} additional damage!");
                    }
                    break;
            }

            return string.Join(" ", effects);
        }

        private string ApplyBuffEffects(GladiatorBot attacker, IRng rng)
        {
            var effects = new List<string>();

            // MP recovery for defensive moves
            if (Category == MoveCategory.Buff && Tier >= MoveTier.Advanced)
            {
                int mpRecovery = MpCost / 2;
                attacker.CurrentMP = Math.Min(attacker.MaxMP, attacker.CurrentMP + mpRecovery);
                effects.Add($"Recovered {mpRecovery} MP from focused technique!");
            }

            return string.Join(" ", effects);
        }

        private string GetMoveDisplayName()
        {
            string tierIcon = Tier switch
            {
                MoveTier.Basic => "⚪",
                MoveTier.Advanced => "🟡",
                MoveTier.Special => "🟠",
                MoveTier.Ultimate => "🔴",
                _ => ""
            };

            return $"{tierIcon} {Name}";
        }

        public void ResetBattleUses()
        {
            RemainingUses = UsesPerBattle;
        }

        public static List<EnhancedMove> CreateEnhancedMoveSet()
        {
            return new List<EnhancedMove>
            {
                // Basic Tier - No MP cost, always available
                new EnhancedMove
                {
                    Name = "Quick Strike",
                    Description = "A fast, basic attack",
                    Type = MoveType.Attack,
                    Power = 25,
                    Accuracy = 95.0,
                    EnergyCost = 0,
                    MpCost = 0,
                    Tier = MoveTier.Basic,
                    Category = MoveCategory.Attack,
                    AnimationEffect = "quick_slash"
                },

                new EnhancedMove
                {
                    Name = "Guard Stance",
                    Description = "Defensive posture that reduces incoming damage",
                    Type = MoveType.Defense,
                    Power = 0,
                    Accuracy = 100.0,
                    EnergyCost = 0,
                    MpCost = 0,
                    Tier = MoveTier.Basic,
                    Category = MoveCategory.Buff,
                    AnimationEffect = "defensive_glow"
                },

                // Advanced Tier - Low MP cost
                new EnhancedMove
                {
                    Name = "Power Strike",
                    Description = "An enhanced attack with more force",
                    Type = MoveType.Attack,
                    Power = 45,
                    Accuracy = 90.0,
                    EnergyCost = 5,
                    MpCost = 8,
                    Tier = MoveTier.Advanced,
                    MinLevel = 3,
                    Category = MoveCategory.Attack,
                    AnimationEffect = "power_burst"
                },

                new EnhancedMove
                {
                    Name = "Energy Surge",
                    Description = "Restores energy and slightly boosts attack",
                    Type = MoveType.Buff,
                    Power = 0,
                    Accuracy = 100.0,
                    EnergyCost = 0,
                    MpCost = 12,
                    Tier = MoveTier.Advanced,
                    MinLevel = 4,
                    Category = MoveCategory.Buff,
                    AnimationEffect = "energy_swirl"
                },

                // Special Tier - Medium MP cost
                new EnhancedMove
                {
                    Name = "Elemental Blast",
                    Description = "A devastating elemental attack",
                    Type = MoveType.Attack,
                    Power = 70,
                    Accuracy = 85.0,
                    EnergyCost = 10,
                    MpCost = 25,
                    Tier = MoveTier.Special,
                    MinLevel = 8,
                    UsesPerBattle = 3,
                    Category = MoveCategory.Attack,
                    InflictsStatus = StatusEffectType.Stun,
                    StatusChance = 20.0,
                    AnimationEffect = "elemental_explosion"
                },

                // Ultimate Tier - High MP cost, limited uses
                new EnhancedMove
                {
                    Name = "Omega Strike",
                    Description = "The ultimate technique - devastating but costly",
                    Type = MoveType.Attack,
                    Power = 120,
                    Accuracy = 80.0,
                    EnergyCost = 20,
                    MpCost = 45,
                    Tier = MoveTier.Ultimate,
                    MinLevel = 15,
                    UsesPerBattle = 1,
                    Category = MoveCategory.Attack,
                    RequiresComboSetup = true,
                    ComboRequirement = "Power Strike",
                    AnimationEffect = "omega_devastation",
                    IsUnlocked = false // Must be unlocked through gameplay
                }
            };
        }
    }
}