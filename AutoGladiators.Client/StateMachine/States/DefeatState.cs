
using AutoGladiators.Client.StateMachine.Interfaces;

namespace AutoGladiators.Client.StateMachine.States
{
    public class DefeatState : IGameState
    {
        public void Enter(IGameStateContext context)
        {
            context.Log("You lost the battle.");
            // TODO: Add consequences (e.g., faint, respawn, money loss).
        }

        public void Update(IGameStateContext context)
        {
            context.TransitionTo("Idle");
        }
    }
}
