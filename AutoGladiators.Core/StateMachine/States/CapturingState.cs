using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Core;
using AutoGladiators.Core.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Core.StateMachine.States
{
    // Request coming from inventory/battle UI
    public readonly record struct CapturingRequest(
        string ItemId,            // e.g., "ControlChip"
        Guid TargetBotId,         // the wild/enemy bot id (we won't deref it yet)
        Guid? PlayerBotId = null, // optional
        double? ForcedChance = null // optional for tests
    );

    // Updated to match GameStateService usage
    public sealed record CapturingResult(
        bool Success,
        GladiatorBot? CapturedBot,
        string[] Log,
        string ItemId,
        Guid TargetBotId
    );

    public sealed class CapturingState : IGameState
    {
        private static readonly Microsoft.Extensions.Logging.ILogger Log = (Microsoft.Extensions.Logging.ILogger)AppLog.For<CapturingState>();

        public GameStateId Id => GameStateId.Capturing;

        private CapturingRequest? _request;
        private bool _attempted;
        private CapturingResult? _result;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            // Expect request in payload; do NOT try to pull from services to avoid missing members.
            _request = args?.Payload as CapturingRequest?;
            ctx.Ui?.ShowCaptureAnimation();
            ctx.Ui?.SetStatus("Attempting captureâ€¦");
            return Task.CompletedTask;
        }

        public Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            // If no request was provided, bail back to battle gracefully.
            if (_request is null)
            {
                ctx.Ui?.HideCaptureAnimation();
                ctx.Ui?.ShowToast("No capture request.");
                return Task.FromResult<StateTransition?>(
                    new StateTransition(GameStateId.Battling, new StateArgs { Reason = "CaptureNoRequest" })
                );
            }

            if (!_attempted)
            {
                _attempted = true;

                // Temporary RNG: later replace with GameStateService.AttemptCaptureAsync
                var chance = _request.Value.ForcedChance ?? 0.35; // default 35%
                var rng = new Random();
                var success = rng.NextDouble() < chance;

                GladiatorBot? capturedBot = success
                    ? new GladiatorBot
                    {
                        Id = unchecked(_request.Value.TargetBotId.GetHashCode()),
                        Name = "Captured Bot",
                        Level = 1
                    }
                    : null;

                _result = new CapturingResult(
                    Success: success,
                    CapturedBot: capturedBot,
                    Log: success
                        ? new[] { $"Capture succeeded with {Math.Round(chance * 100)}% chance." }
                        : new[] { $"Capture failed (chance {Math.Round(chance * 100)}%)." },
                    ItemId: _request.Value.ItemId,
                    TargetBotId: _request.Value.TargetBotId
                );

                // Tell the animation the outcome, then end the animation
                ctx.Ui?.UpdateCaptureAnimationOutcome(success);
                ctx.Ui?.HideCaptureAnimation();

                // Update status/toast
                if (success)
                {
                    ctx.Ui?.ShowCaptureSuccess(_result.Log);
                    ctx.Ui?.SetStatus("Captured!");
                }
                else
                {
                    ctx.Ui?.ShowCaptureFailed(_result.Log);
                    ctx.Ui?.SetStatus("Capture failed.");
                }

                // Transition immediately; if you want to show a modal first, insert a wait flag here.
                var next = success
                    ? new StateTransition(GameStateId.Victory, new StateArgs { Reason = "CaptureVictory", Payload = _result })
                    : new StateTransition(GameStateId.Battling, new StateArgs { Reason = "CaptureFailedReturn", Payload = _result });

                return Task.FromResult<StateTransition?>(next);
            }

            // Shouldnâ€™t reach here in this minimal flow.
            return Task.FromResult<StateTransition?>(null);
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            // Ensure any overlays are closed
            ctx.Ui?.HideCaptureAnimation();
            ctx.Ui?.HideCaptureSuccess();
            ctx.Ui?.HideCaptureFailed();
            return Task.CompletedTask;
        }
    }
}

