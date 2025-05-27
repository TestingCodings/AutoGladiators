using AutoGladiators.Client.Core;
using AutoGladiators.Client.StateMachine.States;

namespace AutoGladiators.Client.StateMachine.Transitions
{
    public class AnyToIdleTransition : StateTransitionBase
    {
        public override string Name => "AnyToIdle";
        public override IGameState TargetState => new IdleState();

        public override bool ShouldTransition(GladiatorBot bot)
        {
            // Example: transition to idle if energy is depleted
            return bot.Energy <= 0;
        }
    }
}
