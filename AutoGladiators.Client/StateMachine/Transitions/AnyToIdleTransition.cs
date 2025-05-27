using AutoGladiators.Client.Core;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.StateMachine.States;

public class AnyToIdleTransition : IStateTransition
{
    public bool CanTransition(GladiatorBot bot)
    {
        return bot.Fatigue > 80 || bot.Health < 20; // Or user override
    }
}
