
using AutoGladiators.Client.StateMachine.Interfaces;
using AutoGladiators.Client.Simulation;

namespace AutoGladiators.Client.StateMachine.States
{
    public class BattlingState : IGameState
    {
        public void Enter(IGameStateContext context)
        {
            context.Log("Battle initiated!");
            var result = context.BattleSimulator.Run(context.Self, context.Enemy);
            context.Log(result.Summary);

            context.TransitionTo(result.Winner == context.Self ? "Victory" : "Defeat");
        }

        public void Update(IGameStateContext context)
        {
            // Battle handled in Enter for now.
        }
    }
}
