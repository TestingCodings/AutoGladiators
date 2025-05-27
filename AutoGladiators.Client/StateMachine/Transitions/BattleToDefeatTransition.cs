using AutoGladiators.Client.Core;
using AutoGladiators.Client.StateMachine.States;

namespace AutoGladiators.Client.StateMachine.Transitions
{
    public class BattleToDefeatTransition : StateTransitionBase
    {
        public override string Name => "BattleToDefeat";
        public override IGameState TargetState => new DefeatState();

        public override bool ShouldTransition(GladiatorBot bot)
        {
            return !bot.IsAlive;
        }
    }
}
