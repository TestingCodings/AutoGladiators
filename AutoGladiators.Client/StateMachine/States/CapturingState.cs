using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.Services;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.Core;
// If you want to re-enter battle with existing setup:
// using AutoGladiators.Client.States; // for BattleSetup

namespace AutoGladiators.Client.StateMachine.States
{
    // ---------- Shared request/result models ----------
    public readonly record struct CapturingRequest(
        string ItemId,          // e.g., "ControlChip"
        Guid TargetBotId,       // the wild/enemy bot
        Guid? PlayerBotId = null,
        double? ForcedChance = null   // for tests/debug
    );

    public sealed record CapturingResult(
        bool Success,
        GladiatorBot? CapturedBot,
        string[] Log,
        string ItemId,
        Guid TargetBotId
    );

    // ==================================================
    // CapturingState: performs the capture attempt
    // ==================================================
    public sealed class CapturingState : IGameState
    {
        public GameStateId Id => GameStateId.Capturing;

        private CapturingRequest? _request;
        private bool _attempted;
        private CapturingResult? _result;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            // Expect a CapturingRequest in payload (from Inventory or Battle UI).
            _request = args?.Payload as CapturingRequest?;
            if (_request is null)
            {
                // Try to infer target from current battle if omitted
                var enemy = ctx.Battle.CurrentEnemy ?? ctx.Game.GetEncounteredBot();
                if (enemy is not null)
                {
                    // Fallback request with default item id – adapt as needed
                    _request = new CapturingRequest(ItemId: "ControlChip", TargetBotId: enemy.Id);
                }
            }

            // Show some UI/animation while capturing
            ctx.Ui?.ShowCaptureAnimation?.Invoke();
            ctx.Ui?.SetStatus?.Invoke("Attempting capture…");

            return Task.CompletedTask;
        }

        public async Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            if (_attempted) // already computed; route to result state
            {
                return _result!.Success
                    ? new StateTransition(GameStateId.CapturingSuccess, new StateArgs { Reason = "CaptureSuccess", Payload = _result })
                    : new StateTransition(GameStateId.CapturingFailed,  new StateArgs { Reason = "CaptureFailed",  Payload = _result });
            }

            if (_request is null)
            {
                // Nothing to do; bail out back to battle
                return new StateTransition(GameStateId.Battling, new StateArgs { Reason = "CaptureNoRequest" });
            }

            _attempted = true;

            // Ensure item exists & can be used (handles in-battle restrictions, counts, etc.)
            if (!ctx.Game.CanUseItem(_request.Value.ItemId, _request.Value.PlayerBotId ?? ctx.Game.GetCurrentBot()?.Id ?? Guid.Empty, inBattle: true, out var reason))
            {
                ctx.Ui?.ShowToast?.Invoke(reason ?? "Cannot use item right now.");
                _result = new CapturingResult(false, null, new[] { reason ?? "UseItem blocked." }, _request.Value.ItemId, _request.Value.TargetBotId);
                return null;
            }

            // Consume item (if consumable)
            ctx.Game.ConsumeItem(_request.Value.ItemId, 1);

            // Perform the capture math/logic in your service (HP %, status, rarity, chip tier, bonuses, etc.)
            var res = await ctx.Game.AttemptCaptureAsync(
                itemId: _request.Value.ItemId,
                targetBotId: _request.Value.TargetBotId,
                forcedChance: _request.Value.ForcedChance,
                ct: ct);

            _result = res ?? new CapturingResult(false, null, new[] { "Capture attempt failed (null result)." }, _request.Value.ItemId, _request.Value.TargetBotId);

            // Let the animation know outcome (optional)
            ctx.Ui?.UpdateCaptureAnimationOutcome?.Invoke(_result.Success);

            // Next tick: transition to success/failed
            return null;
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            ctx.Ui?.HideCaptureAnimation?.Invoke();
            return Task.CompletedTask;
        }
    }

    // ==================================================
    // CapturingSuccessState: show result, add to roster, end battle
    // ==================================================
    public sealed class CapturingSuccessState : IGameState
    {
        public GameStateId Id => GameStateId.CapturingSuccess;

        private CapturingResult? _result;
        private bool _applied;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            _result = args?.Payload as CapturingResult;

            // Show success UI
            if (_result?.CapturedBot is GladiatorBot bot)
            {
                ctx.Ui?.ShowCaptureSuccess?.Invoke(bot, _result.Log);
                ctx.Ui?.SetStatus?.Invoke($"Captured {bot.Name}!");
            }
            else
            {
                ctx.Ui?.ShowCaptureSuccess?.Invoke(null, _result?.Log ?? Array.Empty<string>());
            }

            return Task.CompletedTask;
        }

        public Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            if (!_applied)
            {
                _applied = true;

                // Apply capture effects if not already done by AttemptCaptureAsync:
                // - Add to roster
                // - Remove from wild pool / mark encounter resolved
                // - Award bonuses/XP if desired
                if (_result?.CapturedBot is GladiatorBot bot)
                {
                    ctx.Game.AddBotToRoster(bot);
                    ctx.Game.MarkEncounterResolved(bot.Id);
                }

                // In most designs, a successful capture **ends the battle** as a victory.
                // If you prefer to go straight to Exploring, swap to GameStateId.Exploring.
                return Task.FromResult<StateTransition?>(new StateTransition(
                    GameStateId.Victory,
                    new StateArgs { Reason = "CaptureVictory", Payload = _result }
                ));
            }

            return Task.FromResult<StateTransition?>(null);
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            ctx.Ui?.HideCaptureSuccess?.Invoke();
            return Task.CompletedTask;
        }
    }

    // ==================================================
    // CapturingFailedState: show failure, return to battle (turn consumed)
    // ==================================================
    public sealed class CapturingFailedState : IGameState
    {
        public GameStateId Id => GameStateId.CapturingFailed;

        private CapturingResult? _result;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            _result = args?.Payload as CapturingResult;
            ctx.Ui?.ShowCaptureFailed?.Invoke(_result?.Log ?? Array.Empty<string>());
            ctx.Ui?.SetStatus?.Invoke("Capture failed!");
            return Task.CompletedTask;
        }

        public Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            // Usually, a failed capture **consumes the player's turn** and battle continues.
            // If your BattleManager needs to know, set a flag or call a method here.
            ctx.Game.SetFlag("CaptureFailed", "true");

            return Task.FromResult<StateTransition?>(new StateTransition(
                GameStateId.Battling,
                new StateArgs { Reason = "CaptureFailedReturn" }
            ));
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            ctx.Ui?.HideCaptureFailed?.Invoke();
            return Task.CompletedTask;
        }
    }
}
