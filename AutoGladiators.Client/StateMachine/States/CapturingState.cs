using AutoGladiators.Client.Core;
using AutoGladiators.Client.Simulation;
using AutoGladiators.Client.StateMachine;

namespace AutoGladiators.Client.StateMachine.States
{
    public class CapturingState : IGameState
    {
        public string Name => "Capturing";

        public void Enter(GladiatorBot context, GladiatorBot? opponent = null)
        {
            // Optional: Entry logic for Capturing
        }

        public SimulationResult? Execute(GladiatorBot context, GladiatorBot? opponent = null)
        {
            return new SimulationResult
            {
                Outcome = $"{
                    context.Name
                } is in Capturing state.",
                Log = new List<string> { $"{
                    context.Name
                } performed Capturing." },
                Winner = null
            };
        }

        public void Exit(GladiatorBot context)
        {
            // Optional: Exit logic for Capturing
        }
    }
}
