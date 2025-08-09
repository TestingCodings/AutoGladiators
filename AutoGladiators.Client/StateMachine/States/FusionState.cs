using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.Services;
using AutoGladiators.Client.Core;

namespace AutoGladiators.Client.StateMachine.States
{
    // Payload passed into FusionState via StateArgs.Payload (optional)
    public readonly record struct FusionRequest(
        Guid BotIdA,
        Guid BotIdB,
        bool ConsumeParents = true,
        string? CatalystItemId = null
    );

    // Result payload returned when fusion completes
    public sealed record FusionResult(
        GladiatorBot NewBot,
        string[] Log
    );

    public sealed class FusionState : IGameState
    {
        public GameStateId Id => GameStateId.Fusion;

        private FusionRequest? _request;
        private FusionResult? _result;
        private bool _started;
        private bool _completed;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            Console.WriteLine("Fusion process initiated.");

            // Pull a pre-defined request if one was provided
            _request = args?.Payload as FusionRequest?;

            // Show UI with roster so the user can pick (even if we already have a request, show summary)
            ctx.Ui?.ShowFusionScreen?.Invoke(ctx.Game.BotRoster);
            ctx.Ui?.SetStatus?.Invoke("Select two bots to fuse…");
            return Task.CompletedTask;
        }

        public async Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            if (_completed) return new StateTransition(
                GameStateId.Exploring,
                new StateArgs { Reason = "FusionComplete", Payload = _result }
            );

            // Allow cancel
            if (ctx.Ui?.FusionCancelled == true)
            {
                Console.WriteLine("Fusion cancelled by user.");
                return new StateTransition(GameStateId.Exploring, new StateArgs { Reason = "FusionCancelled" });
            }

            // If no request provided, wait for user to pick two bots
            if (_request is null)
            {
                // The UI bridge should expose a way to retrieve the user's selection:
                // e.g., returns (Guid a, Guid b, bool consumeParents, string? catalystId)
                var sel = ctx.Ui?.GetFusionSelection?.Invoke();
                if (sel is null) return null; // still waiting for user

                _request = new FusionRequest(
                    sel.Value.BotIdA,
                    sel.Value.BotIdB,
                    sel.Value.ConsumeParents,
                    sel.Value.CatalystItemId
                );
            }

            // We have a request—validate it
            if (!_started)
            {
                _started = true;

                // You can implement these in GameStateService; for now they’re conceptual hooks
                var validation = ctx.Game.ValidateFusion(_request.Value);
                if (!validation.IsValid)
                {
                    ctx.Ui?.ShowToast?.Invoke(validation.Message ?? "Fusion not allowed.");
                    // Optionally remain in FusionState to let the user reselect:
                    _started = false;
                    _request = null;
                    return null;
                }

                // Deduct any cost up front (items/currency), optional
                ctx.Game.DeductFusionCost(_request.Value);

                // Perform the fusion (make it async if your logic touches disk/DB, RNG, sprite gen, etc.)
                var res = await ctx.Game.PerformFusionAsync(_request.Value, ct);

                // Apply changes to roster/inventory according to your rules
                ctx.Game.ApplyFusionResult(_request.Value, res);

                _result = res;
                _completed = true;

                // Show results
                ctx.Ui?.ShowFusionResult?.Invoke(res.NewBot, res.Log);
                ctx.Ui?.SetStatus?.Invoke($"Fusion complete: {res.NewBot.Name}");
            }

            // Transition next tick (or wait for a “Continue” UI flag if you prefer)
            return new StateTransition(
                GameStateId.Exploring,
                new StateArgs { Reason = "FusionComplete", Payload = _result }
            );
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            Console.WriteLine("Exiting Fusion State.");
            ctx.Ui?.HideFusionScreen?.Invoke();
            return Task.CompletedTask;
        }
    }

    // ---- Suggested GameStateService hooks (implement as needed) ----
    // public partial class GameStateService
    // {
    //     public (bool IsValid, string? Message) ValidateFusion(FusionRequest request) { ... }
    //     public void DeductFusionCost(FusionRequest request) { ... }
    //     public Task<FusionResult> PerformFusionAsync(FusionRequest request, CancellationToken ct) { ... }
    //     public void ApplyFusionResult(FusionRequest request, FusionResult result) { ... }
    // }
}
