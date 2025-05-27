using System;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.Core.Behaviors;


namespace AutoGladiators.Client.Core
{
    public class ReactiveBehavior : IBehaviorProfile
    {
        public string Name => "Reactive";

        public double Aggression => 0.5;
        public double Caution => 0.5;
        public double ReactionTime => 0.9; // Fast reactions

        public void ExecuteStrategy(GladiatorBot bot, GladiatorBot opponent)
        {
            // Respond to opponent's last action
            switch (opponent.LastAction)
            {
                case GladiatorAction.Attack:
                    bot.Parry();
                    break;

                case GladiatorAction.Defend:
                    bot.PowerStrike(opponent);
                    break;

                case GladiatorAction.Evade:
                    bot.Charge();
                    break;

                default:
                    bot.Attack(opponent);
                    break;
            }
        }
    }
}
