using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.Services;

namespace AutoGladiators.Client.StateMachine.States
{
    public sealed class DefeatState : IGameState
    {
        public GameStateId Id => GameStateId.Defeat;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            Console.WriteLine("Defeat… recovering.");

            // Optional: details about the defeat passed from BattlingState
            var defeatSummary = args?.Payload; // replace with a concrete type if you have one

            // Apply defeat consequences if your service exposes them
            // e.g., currency penalty, durability loss, move player to safe spot, etc.
            // Implement these in GameStateService as needed:
            //   ctx.Game.ApplyDefeatPenalty(defeatSummary);
            //   ctx.Game.MovePlayerToNearestSafePoint();
            //   ctx.Game.RecoverTeamToMinimumHP();

            ctx.Ui?.ShowDefeatScreen?.Invoke(defeatSummary);
            ctx.Game.SetFlag("LastBattleOutcome", "Loss");

            return Task.CompletedTask;
        }

        public Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            // If your UI has a "Continue" button, wait for it; otherwise auto-advance
            if (ctx.Ui?.DefeatAcknowledged == true)
            {
                return Task.FromResult<StateTransition?>(new StateTransition(
                    GameStateId.Exploring,
                    new StateArgs { Reason = "DefeatReturn" }
                ));
            }

            // Auto-transition (comment out if you prefer explicit confirmation)
            return Task.FromResult<StateTransition?>(new StateTransition(
                GameStateId.Exploring,
                new StateArgs { Reason = "DefeatReturnAuto" }
            ));
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            ctx.Ui?.HideDefeatScreen?.Invoke();
            Console.WriteLine("Leaving Defeat state.");
            return Task.CompletedTask;
        }
    }
}
