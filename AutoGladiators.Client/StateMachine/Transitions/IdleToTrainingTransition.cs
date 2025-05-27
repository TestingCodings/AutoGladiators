using AutoGladiators.Client.Core;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.StateMachine.States;

public class IdleToTrainingTransition : IStateTransition
{
    public bool CanTransition(GladiatorBot bot)
    {
        return bot.ShouldTrain(); // Define in GladiatorBot
    }
}
