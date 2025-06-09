using AutoGladiators.Client.Core;
using AutoGladiators.Client.Simulation;
using AutoGladiators.Client.StateMachine;

namespace AutoGladiators.Client.StateMachine.States
{
    public class ExploringState : IGameState
    {
        public string Name => "Exploring";

        public void Enter(GladiatorBot context, GladiatorBot? opponent = null)
        {
            // Optional: Entry logic for Exploring
        }

        public SimulationResult? Execute(GladiatorBot context, GladiatorBot? opponent = null)
        {
            return new SimulationResult
            {
                Outcome = $"{
                    context.Name
                } is in Exploring state.",
                Log = new List<string> { $"{
                    context.Name
                } performed Exploring." },
                Winner = null
            };
        }

        public void Exit(GladiatorBot context)
        {
            // Optional: Exit logic for Exploring
        }
    }
}
