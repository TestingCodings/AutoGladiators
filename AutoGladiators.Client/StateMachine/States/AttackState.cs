using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.StateMachine.States
{
    public class AttackState : IGameState
    {
        public string Name => "Attack";

        public void Enter(GladiatorBot bot)
        {
            // Preparation for attack (maybe animation)
        }

        public void Execute(GladiatorBot bot)
        {
            // This should eventually receive an opponent param
            // bot.Attack(opponent);
        }

        public void Exit(GladiatorBot bot) { }
    }
}