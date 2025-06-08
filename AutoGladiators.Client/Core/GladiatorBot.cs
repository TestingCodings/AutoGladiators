using System;
using System.Collections.Generic;
using SQLite;

namespace AutoGladiators.Client.Core
{
    public class GladiatorBot
    {
        // Identity and Persistence
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string ElementalCore { get; set; } // Fire, Water, Electric, etc.

        // Stats
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Energy { get; set; }
        public int MaxEnergy { get; set; }
        public int AttackPower { get; set; }
        public int Defense { get; set; }
        public int Speed { get; set; }
        public int Luck { get; set; }
        public double CriticalHitChance { get; set; } // Percentage chance of a critical hit

        public int Endurance { get; set; } // Represents how long the bot can fight before tiring

        // Status
        public bool IsBroken { get; set; }

        public bool IsWild { get; set; } // Indicates if the bot is a wild creature or a trained gladiator
        public string StatusCondition { get; set; } // e.g., "Poisoned", "Stunned"

        // Ownership
        public string OwnerId { get; set; } // Optional: assign to a player

        // Skills / Moves (could be tied to a separate Skill class)
        public List<string> Moveset { get; set; } = new();

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
            Health -= damageTaken;
            if (Health <= 0)
            {
                Health = 0;
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
            double elementMultiplier = ElementalSystem.GetEffectiveness(this.ElementalCore, target.ElementalCore);
            damage = (int)(damage * elementMultiplier);

            // 3. Critical hit check
            bool isCrit = rng.NextDouble() < (this.CriticalHitChance / 100.0);
            if (isCrit) damage *= 2;

            // 4. Apply damage
            target.Health -= damage;
            if (target.Health < 0) target.Health = 0;

            // 5. Build combat message
            string result = $"{Name} attacks {target.Name} for {damage} damage.";
            if (isCrit) result += " Critical hit!";
            if (elementMultiplier > 1.0) result += " It's super effective!";
            if (elementMultiplier < 1.0) result += " It's not very effective...";
            if (target.Health == 0) result += $" {target.Name} has been defeated!";

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
            Health = Math.Min(MaxHealth, Health + amount);
        }

        public void Repair()
        {
            if (IsBroken)
            {
                Health = MaxHealth / 2;
                IsBroken = false;
                StatusCondition = null;
            }
        }

        public bool IsAlive => Health > 0 && !IsBroken;
    }
}