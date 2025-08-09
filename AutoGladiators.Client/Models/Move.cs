using AutoGladiators.Client.Core;
using AutoGladiators.Client.Enums;
using System;

namespace AutoGladiators.Client.Models
{
    public enum MoveType
    {
        Attack,
        Defense,
        Heal,
        Buff,
        Debuff,
        StatusEffect
    }

    public class Move
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public MoveType Type { get; set; }
        // e.g. base damage
        public int EnergyCost { get; set; }
        public double Accuracy { get; set; } = 100.0; // 100.0 = 100% hit chance, allows more decimal places
        public StatusEffectType? InflictsStatus { get; set; } = null;
        public double StatusChance { get; set; } = 0.0; // e.g. 25.0 for 25%
        public int Priority { get; set; } = 0; // For quick attack style moves
        public string Element { get; set; } // Fire, Water, Electric...

        public MoveCategory Category { get; set; } // Physical, Special, or Support

        public string StatusEffect { get; set; } // Optional: Poisoned, Stunned, etc.
        public int EffectChance { get; set; } // 0–100
        public bool IsMultiTurn { get; set; } = false;

        public int Power { get; set; }
        public ElementalType ElementalType { get; set; }



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
