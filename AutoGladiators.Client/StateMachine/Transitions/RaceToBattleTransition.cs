using AutoGladiators.Client.Core;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.StateMachine.States;

public class RaceToBattleTransition : IStateTransition
{
    public bool CanTransition(GladiatorBot bot)
    {
        return bot.LastRaceResult != null && bot.LastRaceResult.TimeTaken < 30;
    }
}
