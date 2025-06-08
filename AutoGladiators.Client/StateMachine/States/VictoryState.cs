using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.StateMachine.States
{
    public class VictoryState : IGameState
    {
        public string Name => "Victory";

        public void Enter(GladiatorBot bot)
        {
            // Grant rewards
        }

        public void Execute(GladiatorBot bot)
        {
            // Wait for transition
        }

        public void Exit(GladiatorBot bot) { }
    }
}