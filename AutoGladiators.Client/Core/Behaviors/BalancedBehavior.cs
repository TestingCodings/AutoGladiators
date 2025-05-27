using System;
using AutoGladiators.Client.Core.Behaviors;
using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.Core.Behaviors
{
    public class BalancedBehavior : IBehaviorProfile
    {
        public string Name => "Balanced";

        public double Aggression => 0.6;
        public double Caution => 0.4;
        public double ReactionTime => 0.7; // Simulates faster decisions (e.g., lower delay threshold)
        public double Intelligence => 0.75;
        public double Adaptability => 0.65;

        private Random _random = new();

        public void ExecuteStrategy(GladiatorBot bot, GladiatorBot opponent)
        {
            double healthRatio = (double)bot.Health / bot.MaxHealth;
            double opponentHealthRatio = (double)opponent.Health / opponent.MaxHealth;
            double aggressionScore = Aggression * (1.0 - opponentHealthRatio);
            double cautionScore = Caution * (1.0 - healthRatio);

            // Consider recent opponent behavior
            bool opponentIsAggressive = opponent.LastAction == GladiatorAction.Attack;
            bool recentlyHurt = bot.LastDamageTaken > 0;

            // Calculate action weights
            double attackWeight = aggressionScore + (recentlyHurt ? 0.2 : 0);
            double defendWeight = cautionScore + (opponentIsAggressive ? 0.3 : 0);
            double chargeWeight = 0.3 + (0.1 * Adaptability);

            // Normalize weights
            double totalWeight = attackWeight + defendWeight + chargeWeight;
            attackWeight /= totalWeight;
            defendWeight /= totalWeight;
            chargeWeight /= totalWeight;

            // Random weighted decision
            double roll = _random.NextDouble();
            if (roll < attackWeight)
            {
                bot.Attack(opponent);
            }
            else if (roll < attackWeight + defendWeight)
            {
                bot.Defend();
            }
            else
            {
                bot.Charge();
            }
        }

        public GladiatorAction DecideAction(GladiatorBot bot, GladiatorBot opponent, string environment)
        {
            // Optional: influence by environment (e.g. "desert", "arena", "jungle")
            if (environment == "arena" && Adaptability > 0.5)
            {
                // In familiar environment, become more offensive
                return GladiatorAction.Attack;
            }

            // Mirror behavior from ExecuteStrategy if needed outside full loop
            double healthRatio = (double)bot.Health / bot.MaxHealth;
            double opponentHealthRatio = (double)opponent.Health / opponent.MaxHealth;
            double aggressionScore = Aggression * (1.0 - opponentHealthRatio);
            double cautionScore = Caution * (1.0 - healthRatio);

            double attackWeight = aggressionScore;
            double defendWeight = cautionScore;
            double chargeWeight = 0.2;

            double totalWeight = attackWeight + defendWeight + chargeWeight;
            attackWeight /= totalWeight;
            defendWeight /= totalWeight;
            chargeWeight /= totalWeight;

            double roll = _random.NextDouble();
            if (roll < attackWeight)
                return GladiatorAction.Attack;
            else if (roll < attackWeight + defendWeight)
                return GladiatorAction.Defend;
            else
                return GladiatorAction.Charge;
        }
    }
}
