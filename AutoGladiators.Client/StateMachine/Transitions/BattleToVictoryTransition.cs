using AutoGladiators.Client.Core;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.StateMachine.States;

public class BattleToVictoryTransition : IStateTransition
{
    public bool CanTransition(GladiatorBot bot)
    {
        return bot.LastBattleResult != null && bot.LastBattleResult.Won;
    }
}
