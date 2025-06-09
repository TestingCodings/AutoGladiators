using AutoGladiators.Client.Core;
using AutoGladiators.Client.Simulation;
using AutoGladiators.Client.StateMachine;

namespace AutoGladiators.Client.StateMachine.States
{
    public class AttackState : IGameState
    {
        public string Name => "Attack";

        public void Enter(GladiatorBot context, GladiatorBot? opponent = null)
        {
            // Optional: Entry logic for Attack
        }

        public SimulationResult? Execute(GladiatorBot context, GladiatorBot? opponent = null)
        {
            return new SimulationResult
            {
                Outcome = $"{
                    context.Name
                } is in Attack state.",
                Log = new List<string> { $"{
                    context.Name
                } performed Attack." },
                Winner = null
            };
        }

        public void Exit(GladiatorBot context)
        {
            // Optional: Exit logic for Attack
        }
    }
}
