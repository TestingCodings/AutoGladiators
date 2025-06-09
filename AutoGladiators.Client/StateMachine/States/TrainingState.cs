using AutoGladiators.Client.Core;
using AutoGladiators.Client.Simulation;
using AutoGladiators.Client.StateMachine;

namespace AutoGladiators.Client.StateMachine.States
{
    public class TrainingState : IGameState
    {
        public string Name => "Training";

        public void Enter(GladiatorBot context, GladiatorBot? opponent = null)
        {
            // Optional: Entry logic for Training
        }

        public SimulationResult? Execute(GladiatorBot context, GladiatorBot? opponent = null)
        {
            return new SimulationResult
            {
                Outcome = $"{
                    context.Name
                } is in Training state.",
                Log = new List<string> { $"{
                    context.Name
                } performed Training." },
                Winner = null
            };
        }

        public void Exit(GladiatorBot context)
        {
            // Optional: Exit logic for Training
        }
    }
}
