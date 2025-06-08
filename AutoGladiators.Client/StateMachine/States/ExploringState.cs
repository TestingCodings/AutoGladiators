
using AutoGladiators.Client.StateMachine.Interfaces;
using AutoGladiators.Client.Services;

namespace AutoGladiators.Client.StateMachine.States
{
    public class ExploringState : IGameState
    {
        public void Enter(IGameStateContext context)
        {
            context.Log("Exploring the area...");
            context.ExplorationService.Explore(context.Player);
            context.TransitionTo("Idle");
        }

        public void Update(IGameStateContext context)
        {
            // No continuous logic needed for exploring.
        }
    }
}
