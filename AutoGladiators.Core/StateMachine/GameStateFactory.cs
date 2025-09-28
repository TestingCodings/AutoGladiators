using AutoGladiators.Core.Core;
using AutoGladiators.Core.StateMachine.States;

namespace AutoGladiators.Core.StateMachine
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
                "Dialogue" => new DialogueState(),
                "Fusion" => new FusionState(),
                "Inventory" => new InventoryState(),
                _ => new IdleState(),
            };
        }
    }
}
// This factory method allows for easy instantiation of game states based on their names.
// It can be extended to include more states as the game evolves.
