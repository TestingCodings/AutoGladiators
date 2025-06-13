using AutoGladiators.Client.Core;
using AutoGladiators.Client.Simulation;

namespace AutoGladiators.Client.StateMachine
{
    public interface IGameState
    {
        string Name { get; }

        /// <summary>
        /// Called when entering the state.
        /// </summary>
        /// <param name="context">The bot entering the state.</param>
        /// <param name="opponent">Optional opponent for context-specific states (e.g., battle).</param>
        void Enter(GladiatorBot context, GladiatorBot? opponent = null);

        /// <summary>
        /// Performs the main logic of the state.
        /// </summary>
        /// <param name="context">The bot executing the state.</param>
        /// <param name="opponent">Optional opponent.</param>
        /// <returns>Simulation result, if applicable (e.g. battles, training outcomes).</returns>
        SimulationResult? Execute(GladiatorBot context, GladiatorBot? opponent = null);

        /// <summary>
        /// Called when exiting the state.
        /// </summary>
        /// <param name="context">The bot exiting the state.</param>
        void Exit(GladiatorBot context);
    }
}
// This interface defines the contract for game states in the Auto Gladiators client.
// Each state must implement methods for entering, executing, and exiting the state.
// The Execute method can return a SimulationResult for states that involve simulations, like battles or training.  