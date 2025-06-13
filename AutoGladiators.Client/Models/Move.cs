using AutoGladiators.Client.Core;
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
        public int Power { get; set; } // e.g. base damage
        public int EnergyCost { get; set; }
        public double Accuracy { get; set; } = 1.0; // 1.0 = 100% hit chance
        public string Element { get; set; } // Fire, Water, Electric...

        public string StatusEffect { get; set; } // Optional: Poisoned, Stunned, etc.
        public int EffectChance { get; set; } // 0–100
        public bool IsMultiTurn { get; set; } = false;

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
