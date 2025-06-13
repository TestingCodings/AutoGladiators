using AutoGladiators.Client.StateMachine.States;

namespace AutoGladiators.Client.StateMachine.Transitions
{
    public class LowHealthTransition : StateTransition
    {
        private readonly int _threshold;

        public LowHealthTransition(int threshold)
        {
            _threshold = threshold;
        }

        public override bool ShouldTransition(IGameStateContext context)
        {
            return context.Self.CurrentHealth < _threshold;
        }

        public override IGameState GetNextState()
        {
            return new DefendState();
        }
    }
}
