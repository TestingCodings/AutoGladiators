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
        // Convert percentage accuracy to 0.0-1.0 range and compare with random double
        double threshold = Accuracy / 100.0;
        return rng.NextDouble() <= threshold;
    }

    public bool isCrit(IRng rng)
    {
        // This method is used for testing - in actual gameplay, crit is calculated in CalculateDamage
        // For testing purposes, we'll use a simple threshold
        return rng.NextDouble() < 0.1; // 10% base crit chance for testing
    }  

    public int CalculateDamage(GladiatorBot attacker, GladiatorBot defender, IRng rng)
        {
            // Simplified damage formula for MVP
            int attackPower = attacker.AttackPower;
            int defense = defender.Defense;
            
            // Base damage: attacker's power modified by move power
            double baseDamage = (attackPower + Power) * 0.7;
            
            // Defense reduces damage
            baseDamage = Math.Max(baseDamage - (defense * 0.5), baseDamage * 0.1);
            
            // Apply random factor (90% to 110%)
            double randomFactor = rng.Next(90, 111) / 100.0;
            baseDamage *= randomFactor;
            
            // Critical hit check (10% base chance)
            if (rng.NextDouble() < 0.1)
            {
                baseDamage *= 1.5; // Critical hits do 1.5x damage
            }

            return Math.Max(1, (int)baseDamage); // Ensure at least 1 damage is done
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

            var rng = new DefaultRng();
            if (!CheckAccuracy(rng))
                return $"{attacker.Name} used {Name}, but it missed!";

            int damage = 0;
            if (Type == MoveType.Attack)
            {
                // Use the more sophisticated damage calculation if available
                if (Power > 0)
                {
                    damage = CalculateDamage(attacker, defender, rng);
                }
                else
                {
                    // Fallback simple calculation
                    damage = Math.Max(1, attacker.AttackPower - defender.Defense / 2);
                }
            }

            if (damage > 0)
            {
                defender.ReceiveDamage(damage);
            }

            string result = $"{attacker.Name} used {Name}!";
            if (damage > 0) 
            {
                result += $" It dealt {damage} damage to {defender.Name}!";
                if (defender.CurrentHealth <= 0)
                {
                    result += $" {defender.Name} has been defeated!";
                }
            }

            return result;
        }
        
        // MVP: Create basic moves for starter bots
        public static List<Move> CreateMVPMoves()
        {
            return new List<Move>
            {
                new Move
                {
                    Name = "Tackle",
                    Description = "A straightforward physical attack",
                    Type = MoveType.Attack,
                    Power = 20,
                    Accuracy = 90.0,
                    EnergyCost = 5,
                    Category = MoveCategory.Attack
                },
                new Move
                {
                    Name = "Guard",
                    Description = "Reduce incoming damage for this turn",
                    Type = MoveType.Defense,
                    Power = 0,
                    Accuracy = 100.0,
                    EnergyCost = 3,
                    Category = MoveCategory.Buff
                },
                new Move
                {
                    Name = "Metal Strike",
                    Description = "A powerful metal-enhanced attack",
                    Type = MoveType.Attack,
                    Power = 35,
                    Accuracy = 85.0,
                    EnergyCost = 10,
                    Category = MoveCategory.Attack
                }
            };
        }
    }
}
