using AutoGladiators.Client.Core;
using AutoGladiators.Client.StateMachine.States;

namespace AutoGladiators.Client.StateMachine.Transitions
{
    public class IdleToTrainingTransition : StateTransitionBase
    {
        public override string Name => "IdleToTraining";
        public override IGameState TargetState => new TrainingState();

        public override bool ShouldTransition(GladiatorBot bot)
        {
            return bot.Energy >= 10 && bot.Health > 50;
        }
    }
}
