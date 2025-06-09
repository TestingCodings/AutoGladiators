using AutoGladiators.Client.StateMachine.States;

namespace AutoGladiators.Client.StateMachine
{
    public interface IStateTransition
    {
        bool ShouldTransition(IGameStateContext context);
        bool CanTransition(IGameStateContext context);
        string Name { get; }
        IGameState TargetState { get; }
    }

}

// This interface defines the contract for state transitions in the game state machine.
// It includes methods to determine if a transition should occur, check if it can occur, and retrieve the target state.