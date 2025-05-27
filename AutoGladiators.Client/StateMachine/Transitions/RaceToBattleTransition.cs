using AutoGladiators.Client.Core;
using AutoGladiators.Client.StateMachine.States;

namespace AutoGladiators.Client.StateMachine.Transitions
{
    public class RaceToBattleTransition : StateTransitionBase
    {
        public override string Name => "RaceToBattle";
        public override IGameState TargetState => new BattlingState();

        public override bool ShouldTransition(GladiatorBot bot)
        {
            return bot.LastRaceResult?.ToString() == "ChallengeIssued";
        }
    }
}
