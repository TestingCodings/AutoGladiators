
using AutoGladiators.Client.StateMachine.Interfaces;
using AutoGladiators.Client.Simulation;

namespace AutoGladiators.Client.StateMachine.States
{
    public class CapturingState : IGameState
    {
        public void Enter(IGameStateContext context)
        {
            context.Log("Attempting to capture bot...");
            var result = context.CaptureSimulator.Run(context.Self, context.Enemy);
            context.Log(result.Summary);

            if (result.Success)
                context.TransitionTo("Victory");
            else
                context.TransitionTo("Defeat");
        }

        public void Update(IGameStateContext context)
        {
            // Passive, no-op for now.
        }
    }
}
