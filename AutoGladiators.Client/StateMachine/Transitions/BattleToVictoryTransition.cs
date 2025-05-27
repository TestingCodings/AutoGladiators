using AutoGladiators.Client.Core;
using AutoGladiators.Client.StateMachine.States;

namespace AutoGladiators.Client.StateMachine.Transitions
{
    public class BattleToVictoryTransition : StateTransitionBase
    {
        public override string Name => "BattleToVictory";
        public override IGameState TargetState => new VictoryState();

        public override bool ShouldTransition(GladiatorBot bot)
        {
            return bot.IsAlive && bot.LastBattleResult?.ToString() == "Win";
        }
    }
}
