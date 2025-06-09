using AutoGladiators.Client.StateMachine.States;

namespace AutoGladiators.Client.StateMachine
{
    public interface IStateTransition
    {
        bool ShouldTransition(IGameStateContext context);
        IGameState GetNextState();
    }
    public interface IStateTransitionLegal : IStateTransition
    {
        bool CanTransition(IGameStateContext context);
        string TransitionName { get; }
        IGameState TargetState { get; }
    }
}

// This interface defines the contract for state transitions in the game state machine.
// It includes methods to check if a transition should occur, if it can legally happen, and properties for the target state and transition name.