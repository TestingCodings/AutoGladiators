using System;
using AutoGladiators.Client.Core.Behaviors;
using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.Core.Behaviors
{
    public class ReactiveBehavior : IBehaviorProfile
    {
        public string Name => "Reactive";

        public double Aggression => 0.5;
        public double Caution => 0.5;
        public double ReactionTime => 0.95;
        public double Intelligence => 0.7;
        public double Adaptability => 0.9;

        private Random _random = new();

        public void ExecuteStrategy(GladiatorBot bot, GladiatorBot opponent)
        {
            switch (opponent.LastAction)
            {
                case GladiatorAction.Attack:
                    bot.Defend();
                    break;
                case GladiatorAction.Defend:
                    bot.Charge();
                    break;
                case GladiatorAction.Charge:
                    bot.Attack(opponent);
                    break;
                default:
                    bot.Attack(opponent);
                    break;
            }
        }

        public GladiatorAction DecideAction(GladiatorBot bot, GladiatorBot opponent, string environment)
        {
            switch (opponent.LastAction)
            {
                case GladiatorAction.Attack:
                    return GladiatorAction.Defend;
                case GladiatorAction.Defend:
                    return GladiatorAction.Charge;
                case GladiatorAction.Charge:
                    return GladiatorAction.Attack;
                default:
                    return GladiatorAction.Attack;
            }
        }
    }
}
