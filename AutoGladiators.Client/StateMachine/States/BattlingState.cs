using AutoGladiators.Client.Core;
using AutoGladiators.Client.Simulation;
using AutoGladiators.Client.StateMachine;

namespace AutoGladiators.Client.StateMachine.States
{
    public class BattlingState : IGameState
    {
        public string Name => "Battling";

        public void Enter(GladiatorBot context, GladiatorBot? opponent = null)
        {
            // Optional: Entry logic for Battling
        }

        public SimulationResult? Execute(GladiatorBot context, GladiatorBot? opponent = null)
        {
            return new SimulationResult
            {
                Outcome = $"{
                    context.Name
                } is in Battling state.",
                Log = new List<string> { $"{
                    context.Name
                } performed Battling." },
                Winner = null
            };
        }

        public void Exit(GladiatorBot context)
        {
            // Optional: Exit logic for Battling
        }
    }
}
