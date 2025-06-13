using AutoGladiators.Client.Core;
using AutoGladiators.Client.StateMachine.States;

namespace AutoGladiators.Client.StateMachine
{
    public static class GameStateFactory
    {
        public static IGameState CreateState(string stateName)
        {
            return stateName switch
            {
                "Idle" => new IdleState(),
                "Training" => new TrainingState(),
                "Exploring" => new ExploringState(),
                "Capturing" => new CapturingState(),
                "Battling" => new BattlingState(),
                "Victory" => new VictoryState(),
                "Defeat" => new DefeatState(),  
                _ => new IdleState(),
            };
        }
    }
}
// This factory method allows for easy instantiation of game states based on their names.
// It can be extended to include more states as the game evolves.
