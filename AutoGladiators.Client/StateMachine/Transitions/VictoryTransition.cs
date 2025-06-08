using AutoGladiators.Client.StateMachine.States;

namespace AutoGladiators.Client.StateMachine.Transitions
{
    public class VictoryTransition : StateTransition
    {
        public override bool ShouldTransition(IGameStateContext context)
        {
            return !context.Enemy.IsAlive;
        }

        public override IGameState GetNextState()
        {
            return new VictoryState();
        }
    }
}
