using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.StateMachine
{
    public interface IStateTransition
    {
        /// <summary>
        /// Checks if the transition should occur based on logic involving the bot.

        
        /// <returns>True if the transition condition is met.</returns>
        bool ShouldTransition(GladiatorBot bot);


        /// Determines if the transition is allowed from the current state.

    
        /// <returns>True if the transition can legally occur.</returns>
        bool CanTransition(GladiatorBot bot);







        /// The state to transition to if conditions are met.
        IGameState TargetState { get; }


        /// <returns>The name of the transition.</returns>
        string Name { get; }
    }
}
// This interface defines the contract for state transitions in the game state machine.
// It includes methods to check if a transition should occur, if it can legally happen, and properties for the target state and transition name.