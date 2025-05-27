using AutoGladiators.Client.Core;



namespace AutoGladiators.Client.Core.Behaviors
{
    public interface IBehaviorProfile
    {
        double Aggression { get; }
        double Intelligence { get; }
        double ReactionTime { get; } // in milliseconds
        double Adaptability { get; }

        string DecideAction(GladiatorBot self, GladiatorBot opponent, string context);
    }
}
