using AutoGladiators.Core.Core;
using AutoGladiators.Core.Enums;
using AutoGladiators.Core.Rng;
using System;

namespace AutoGladiators.Core.Models
{

    public class Move
    {
    public string? Name { get; set; }
    public string? Description { get; set; }
    public MoveType Type { get; set; }
    // e.g. base damage
    public int EnergyCost { get; set; }
    public double Accuracy { get; set; } = 100.0; // 100.0 = 100% hit chance, allows more decimal places
    public StatusEffectType? InflictsStatus { get; set; } = null;
    public double StatusChance { get; set; } = 0.0; // e.g. 25.0 for 25%
    public int Priority { get; set; } = 0; // For quick attack style moves
    public string? Element { get; set; } // Fire, Water, Electric...

    public MoveCategory Category { get; set; } // Attack, Buff, Debuff, etc.

    public string? StatusEffect { get; set; } // Optional: Poisoned, Stunned, etc.
    public int EffectChance { get; set; } // 0â€“100
    public bool IsMultiTurn { get; set; } = false;

    public bool CheckAccuracy(IRng rng)
    {
        // If Accuracy is 100, always hit; otherwise, roll
        if (Accuracy >= 100) return true;
        return rng.Next(1, 101) <= Accuracy;
    }

    public bool isCrit(IRng rng)
    {
        // This method is used for testing - in actual gameplay, crit is calculated in CalculateDamage
        // For testing purposes, we'll use a simple threshold
        return rng.NextDouble() < 0.1; // 10% base crit chance for testing
    }  

    public int CalculateDamage(GladiatorBot attacker, GladiatorBot defender, IRng rng)
        {
            // Basic damage formula
            int baseDamage = (((2 * attacker.Level / 5 + 2) * Power * attacker.Strength / defender.Defense) / 50) + 2;

            // Apply random factor (85% to 100%)
            double randomFactor = rng.Next(85, 101) / 100.0;
            baseDamage = (int)(baseDamage * randomFactor);

            // TODO: Implement elemental multiplier if defender has an Element property and ElementalSystem.GetMultiplier exists.
            // For now, skip this step to avoid errors.

            // Critical hit check
            if (rng.NextDouble() * 100 < attacker.CriticalHitChance)
            {
                baseDamage = (int)(baseDamage * 1.5); // Critical hits do 1.5x damage
            }

            return Math.Max(1, baseDamage); // Ensure at least 1 damage is done
        }
    public int Power { get; set; }

        public static List<Move> BasicMoves()
        {
            return new List<Move>
            {
                new Move
                {
                    Name = "Jab",
                    Description = "A quick jab.",
                    Type = MoveType.Attack,
                    Power = 6,
                    Accuracy = 0.9,
                    EnergyCost = 0,
                    Element = null,
                    Category = MoveCategory.Attack
                }
            };
        }

        public string Use(GladiatorBot attacker, GladiatorBot defender)
        {
            if (attacker.Energy < EnergyCost)
                return $"{attacker.Name} tried to use {Name}, but didn't have enough energy.";

            attacker.UseEnergy(EnergyCost);

            var rng = new Random();
            if (rng.NextDouble() > Accuracy)
                return $"{attacker.Name} used {Name}, but it missed!";

            int damage = Type == MoveType.Attack
                ? Math.Max(1, Power + attacker.Strength - defender.Defense)
                : 0;

            defender.ReceiveDamage(damage);

            string result = $"{attacker.Name} used {Name}!";
            if (damage > 0) result += $" It dealt {damage} damage to {defender.Name}.";

            if (!string.IsNullOrEmpty(StatusEffect) && rng.Next(100) < EffectChance)
            {
                defender.StatusCondition = StatusEffect;
                result += $" {defender.Name} is now {StatusEffect}!";
            }

            return result;
        }
    }
}
