using System;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.Core.Behaviors;


namespace AutoGladiators.Client.Core
{
    public class BalancedBehavior : IBehaviorProfile
    {
        public string Name => "Balanced";

        public double Aggression => 0.6;
        public double Caution => 0.4;
        public double ReactionTime => 0.7; // Good but not elite

        public void ExecuteStrategy(GladiatorBot bot, GladiatorBot opponent)
        {
            // Alternates between attack and defense
            if (bot.LastAction == GladiatorAction.Attack)
            {
                bot.Defend();
            }
            else if (bot.LastAction == GladiatorAction.Defend)
            {
                bot.Charge();
            }
            else
            {
                bot.Attack(opponent);
            }
        }
    }
}
