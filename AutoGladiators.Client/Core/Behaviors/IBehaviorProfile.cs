using AutoGladiators.Client.Core;



namespace AutoGladiators.Client.Core.Behaviors
{
    public interface IBehaviorProfile
    {
        string Name { get; }
        double Aggression { get; }
        double Caution { get; }
        double ReactionTime { get; }
        double Intelligence { get; }
        double Adaptability { get; }

        void ExecuteStrategy(GladiatorBot bot, GladiatorBot opponent);
        GladiatorAction DecideAction(GladiatorBot bot, GladiatorBot opponent, string environment);
    }
}
