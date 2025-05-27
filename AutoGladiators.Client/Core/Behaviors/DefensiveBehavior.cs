using System;
using AutoGladiators.Client.Core.Behaviors;
using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.Core.Behaviors
{
    public class DefensiveBehavior : IBehaviorProfile
    {
        public string Name => "Defensive";

        public double Aggression => 0.2;
        public double Caution => 0.8;
        public double ReactionTime => 0.9;
        public double Intelligence => 0.85;
        public double Adaptability => 0.6;

        private Random _random = new();

        public void ExecuteStrategy(GladiatorBot bot, GladiatorBot opponent)
        {
            double healthRatio = (double)bot.Health / bot.MaxHealth;
            bool opponentCharging = opponent.LastAction == GladiatorAction.Charge;

            double defendWeight = Caution + (1.0 - healthRatio);
            double attackWeight = Aggression * 0.5;
            double chargeWeight = 0.2 + (opponentCharging ? 0.3 : 0);

            double total = defendWeight + attackWeight + chargeWeight;
            defendWeight /= total;
            attackWeight /= total;
            chargeWeight /= total;

            double roll = _random.NextDouble();
            if (roll < defendWeight)
                bot.Defend();
            else if (roll < defendWeight + attackWeight)
                bot.Attack(opponent);
            else
                bot.Charge();
        }

        public GladiatorAction DecideAction(GladiatorBot bot, GladiatorBot opponent, string environment)
        {
            if (bot.Health < bot.MaxHealth * 0.4)
                return GladiatorAction.Defend;

            return _random.NextDouble() > 0.7 ? GladiatorAction.Charge : GladiatorAction.Defend;
        }
    }
}
