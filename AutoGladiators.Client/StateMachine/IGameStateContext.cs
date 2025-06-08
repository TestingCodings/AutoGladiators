using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.StateMachine
{
    /// <summary>
    /// Interface that represents the data and control flow context passed to states and transitions.
    /// </summary>
    public interface IGameStateContext
    {
        GladiatorBot Self { get; }
        GladiatorBot Enemy { get; }

        bool IsBattleOver { get; }
        void TransitionTo(IGameState newState);
        void Log(string message);

        void ApplyPostBattleEffects();
        void ResetTurnState();
        void AwardXP(int amount);
    }
}
