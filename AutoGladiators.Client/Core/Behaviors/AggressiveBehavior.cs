using System;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.Core.Behaviors;



namespace AutoGladiators.Core.Behaviors
{
    public class AggressiveBehavior : IBehaviorProfile
    {
        public double Aggression => 0.9;
        public double Intelligence => 0.4;
        public double ReactionTime => 250;
        public double Adaptability => 0.3;

        public string DecideAction(GladiatorBot self, GladiatorBot opponent, string context)
        {
            if (context == "battle")
            {
                return self.Strength > opponent.Strength ? "PowerAttack" : "QuickStrike";
            }
            if (context == "race")
            {
                return "Sprint";
            }
            return "Advance";
        }
    }
}
