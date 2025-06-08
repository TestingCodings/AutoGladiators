using AutoGladiators.Client.StateMachine.States;

namespace AutoGladiators.Client.StateMachine
{
    public abstract class StateTransitionBase : IStateTransition
    {
        protected readonly IGameState Target;

        protected StateTransitionBase(IGameState target)
        {
            Target = target;
        }

        public abstract bool ShouldTransition(IGameStateContext context);

        public IGameState GetNextState() => Target;
    }
}
