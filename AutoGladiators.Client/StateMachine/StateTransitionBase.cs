using AutoGladiators.Client.StateMachine.States;

namespace AutoGladiators.Client.StateMachine
{
    public abstract class StateTransitionBase : IStateTransition
    {
        public abstract bool ShouldTransition(IGameStateContext context);
        public abstract bool CanTransition(IGameStateContext context);
        public abstract string Name { get; }
        public abstract IGameState TargetState { get; }

        // Optional: Common functionality for all transitions can be added here.
        // For example, logging or validation methods that can be reused across different transitions.
    }
}
