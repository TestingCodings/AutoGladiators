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
                "Racing" => new RacingState(),
                "Battling" => new BattlingState(),
                "Victory" => new VictoryState(),
                "Defeat" => new DefeatState(),
                "Training" => new TrainingState(),
                _ => new IdleState() // fallback
            };
        }
    }
}
