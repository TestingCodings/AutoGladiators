using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.StateMachine.States
{
    public class DefendState : IGameState
    {
        public string Name => "Defend";

        public void Enter(GladiatorBot bot)
        {
            bot.Defend();
        }

        public void Execute(GladiatorBot bot)
        {
            // Maintain defense
        }

        public void Exit(GladiatorBot bot) { }
    }
}