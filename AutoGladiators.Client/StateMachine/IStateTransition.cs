using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.StateMachine
{
    public interface IStateTransition
    {
      
        bool ShouldTransition(GladiatorBot bot);


   
        bool CanTransition(GladiatorBot bot);







        IGameState TargetState { get; }



        string Name { get; }
    }
}
// This interface defines the contract for state transitions in the game state machine.
// It includes methods to check if a transition should occur, if it can legally happen, and properties for the target state and transition name.