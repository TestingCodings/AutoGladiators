using System;
using AutoGladiators.Client.Core.Behaviors;
using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.Core.Behaviors
{
    public class AggressiveBehavior : IBehaviorProfile
    {
        public string Name => "Aggressive";

        public double Aggression => 0.9;
        public double Caution => 0.1;
        public double ReactionTime => 0.85;
        public double Intelligence => 0.65;
        public double Adaptability => 0.4;

        private Random _random = new();

        public void ExecuteStrategy(GladiatorBot bot, GladiatorBot opponent)
        {
            bool opponentIsWeakened = opponent.Health < bot.Health * 0.8;
            bool recentlyDamaged = bot.LastDamageTaken > 0;

            double attackWeight = Aggression + (opponentIsWeakened ? 0.3 : 0);
            double chargeWeight = 0.3 + (recentlyDamaged ? 0.1 : 0);
            double defendWeight = 0.1;

            double total = attackWeight + chargeWeight + defendWeight;
            attackWeight /= total;
            chargeWeight /= total;
            defendWeight /= total;

            double roll = _random.NextDouble();
            if (roll < attackWeight)
                bot.Attack(opponent);
            else if (roll < attackWeight + chargeWeight)
                bot.Charge();
            else
                bot.Defend();
        }

        public GladiatorAction DecideAction(GladiatorBot bot, GladiatorBot opponent, string environment)
        {
            return _random.NextDouble() < 0.8 ? GladiatorAction.Attack : GladiatorAction.Charge;
        }
    }
}
