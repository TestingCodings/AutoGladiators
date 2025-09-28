using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Enums;

namespace AutoGladiators.Core.Core
{
    public class GladiatorBot
    {
        // Identity and Persistence
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ElementalCore ElementalCore { get; set; } = ElementalCore.None;
        public string Description { get; set; } = "A mysterious bot with unknown capabilities.";


        // Stats
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Defense { get; set; }
        public int MaxHealth { get; set; } = 100;
        public int CurrentHealth
        {
            get => MaxHealth - damageTaken;
            set
            {
                if (value < 0) value = 0;
                damageTaken = MaxHealth - value;
            }
        }
        private int damageTaken; // Tracks how much damage has been taken from MaxHealth

        // Optional: create a shorthand

        public int Energy { get; set; }

        public int Strength { get; set; }
        public int MaxEnergy { get; set; }
        public int AttackPower { get; set; }
        public int Speed { get; set; }
        public int Luck { get; set; }
        public double CriticalHitChance { get; set; } // Percentage chance of a critical hit

        public int Endurance { get; set; } // Represents how long the bot can fight before tiring

        // Status
        public bool IsBroken { get; set; }

        public bool IsFainted { get; set; }
        public bool HasOwner { get; set; } // Indicates if the bot is a wild creature or a trained gladiator

        public string StatusCondition { get; set; } = string.Empty; // e.g., "Poisoned", "Stunned"

        // Ownership
        public string OwnerId { get; set; } = string.Empty; // Optional: assign to a player

        // Skills / Moves (could be tied to a separate Skill class)
        public List<string> Moveset { get; set; } = new();

        public List<string> LearnableMoves { get; set; } = new();

        // Add missing battle stages
        public int AttackStage { get; set; }
        public int DefenseStage { get; set; }
        public int SpecialAttackStage { get; set; }
        public int SpecialDefenseStage { get; set; }
        public int SpeedStage { get; set; }

        // Add missing status effect fields
        public StatusEffectType? CurrentStatus { get; set; }
        public int StatusTurnsRemaining { get; set; }

        // Add HP property for compatibility
        public int HP
        {
            get => CurrentHealth;
            set => CurrentHealth = value;
        }

        // Status Effects System
        public List<StatusEffect> ActiveStatusEffects { get; set; } = new();

        public void ApplyStatusEffect(StatusEffect effect)
        {
            // Check if effect of same type already exists
            var existing = ActiveStatusEffects.FirstOrDefault(e => e.Type == effect.Type);
            if (existing != null)
            {
                // Refresh duration (take the longer duration)
                existing.Duration = Math.Max(existing.Duration, effect.Duration);
                existing.Intensity = Math.Max(existing.Intensity, effect.Intensity);
            }
            else
            {
                ActiveStatusEffects.Add(effect.Clone());
            }
        }

        public bool HasStatus(StatusEffectType type)
        {
            return ActiveStatusEffects.Any(e => e.Type == type && !e.IsExpired);
        }

        public StatusEffect? GetStatus(StatusEffectType type)
        {
            return ActiveStatusEffects.FirstOrDefault(e => e.Type == type && !e.IsExpired);
        }

        public void TickStatusEffects()
        {
            foreach (var effect in ActiveStatusEffects.ToList())
            {
                effect.Tick();
                if (effect.IsExpired)
                {
                    ActiveStatusEffects.Remove(effect);
                }
            }
        }

        public void Cleanse()
        {
            ActiveStatusEffects.Clear();
        }


        // Progression logic
        public void GainExperience(int xp)
        {
            Experience += xp;
            if (Experience >= GetXpThreshold(Level))
            {
                Level++;
                Experience = 0;
                MaxHealth += 10;
                MaxEnergy += 5;
                AttackPower += 2;
                Defense += 1;
                Speed += 1;
            }
        }

        public int GetXpThreshold(int level) => 100 + (level * 20);

        // Battle utilities
        public void ReceiveDamage(int amount)
        {
            int damageTaken = Math.Max(0, amount - Defense);
            CurrentHealth -= damageTaken;
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                IsFainted = true;
                StatusCondition = "Disabled";
            }
        }

        public string Attack(GladiatorBot target)
        {
            if (target == null) return $"{Name} attacks wildly at nothing...";

            Random rng = new Random();

            // 1. Base damage formula
            int baseAttack = this.Strength;
            int baseDefense = target.Defense;
            int damage = Math.Max(1, (baseAttack * 2 - baseDefense)); // Minimum 1 damage

            // 2. Elemental effectiveness
            //double elementMultiplier = ElementalSystem.GetModifier(this.ElementalCore, target.ElementalCore);
            //damage = (int)(damage * elementMultiplier);

            // 3. Critical hit check
            bool isCrit = rng.NextDouble() < (this.CriticalHitChance / 100.0);
            if (isCrit) damage *= 2;

            // 4. Apply damage
            target.CurrentHealth -= damage;
            if (target.CurrentHealth < 0) target.CurrentHealth = 0;

            // 5. Build combat message
            string result = $"{Name} attacks {target.Name} for {damage} damage.";
            if (isCrit) result += " Critical hit!";
            //if (elementMultiplier > 1.0) result += " It's super effective!";
            //if (elementMultiplier < 1.0) result += " It's not very effective...";
            if (target.CurrentHealth == 0) result += $" {target.Name} has been defeated!";

            return result;
        }
        public void UseEnergy(int amount)
        {
            Energy = Math.Max(0, Energy - amount);
            if (Energy <= 0)
            {
                IsBroken = true;
                StatusCondition = "Exhausted";
            }
        }

        public void Heal(int amount)
        {
            if (IsFainted) return; // Cannot heal fainted bots
            CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
        }

        public void Repair()
        {
            if (IsBroken)
            {
                CurrentHealth = MaxHealth / 2;
                IsBroken = false;
                StatusCondition = string.Empty;
            }
        }

        public bool IsAlive => CurrentHealth > 0 && !IsBroken;
        public void ResetStages()
        {
            AttackStage = 0;
            DefenseStage = 0;
            SpecialAttackStage = 0;
            SpecialDefenseStage = 0;
            SpeedStage = 0;
        }

        public void ApplyStatus(StatusEffectType status, int duration)
        {
            CurrentStatus = status;
            StatusTurnsRemaining = duration;
        }

        public void TickStatus()
        {
            if (CurrentStatus != null)
            {
                StatusTurnsRemaining--;
                if (StatusTurnsRemaining <= 0)
                    CurrentStatus = null;
            }
        }

        public int GetEffectiveStat(int baseValue, int stage)
        {
            float[] stageMultipliers = { 0.25f, 0.29f, 0.33f, 0.4f, 0.5f, 0.66f, 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f }; // 0â€“12 mapped to -6 to +6
            int index = stage + 6;
            index = index < 0 ? 0 : (index > 12 ? 12 : index);
            return (int)(baseValue * stageMultipliers[index]);
        }

    }
    public class PlayerBot : GladiatorBot
    {
        public string PlayerId { get; set; } = string.Empty; // ID of the player who owns this bot
        public DateTime LastBattleTime { get; set; } // Timestamp of the last battle this bot participated in

        // Additional player-specific properties can be added here
    }
    public class EnemyBot : GladiatorBot
    {

        // Additional enemy-specific properties can be added here
    }

}
