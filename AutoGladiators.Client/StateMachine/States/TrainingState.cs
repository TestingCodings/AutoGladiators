
using AutoGladiators.Client.StateMachine.Interfaces;
using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.StateMachine.States
{
    public class TrainingState : IGameState
    {
        public void Enter(IGameStateContext context)
        {
            context.Log("Training session started.");
            var result = context.TrainingSimulator.Run(context.Self);
            context.Log(result.Summary);
        }

        public void Update(IGameStateContext context)
        {
            context.TransitionTo("Idle");
        }
    }
}
