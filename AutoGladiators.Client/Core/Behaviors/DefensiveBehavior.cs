using System;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.Core.Behaviors;



namespace AutoGladiators.Client.Core
{
    public class DefensiveBehavior : IBehaviorProfile
    {
        public string Name => "Defensive";

        public double Aggression => 0.2;
        public double Caution => 0.9;
        public double ReactionTime => 0.5; // Moderate reflexes

        public void ExecuteStrategy(GladiatorBot bot, GladiatorBot opponent)
        {
            // Prioritize defense and evasion
            if (bot.Health < 50)
            {
                bot.Defend();
            }
            else if (opponent.IsCharging)
            {
                bot.Evade();
            }
            else
            {
                bot.CounterAttack(opponent);
            }
        }
    }
}
