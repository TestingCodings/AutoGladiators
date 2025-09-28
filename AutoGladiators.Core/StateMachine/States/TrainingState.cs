using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Core.StateMachine.States
{
    // What the caller can pass to start training.
    public readonly record struct TrainingRequest(
        Guid TargetBotId,
        string Mode,          // e.g., "Strength", "Agility", "Accuracy", "MoveMastery:PowerStrike"
        TimeSpan Duration,    // simulated time; could be instant with a progress bar
        bool ConsumeItems = false,
        string? FacilityId = null
    );

    // What the game returns when training finishes.
    public sealed record TrainingResult(
        Guid TargetBotId,
        string Mode,
        int XpGained,
        string[] StatChanges, // e.g., ["STR +2", "ACC +1"]
        string[] Logs
    );

    public sealed class TrainingState : IGameState
    {
        private static readonly Microsoft.Extensions.Logging.ILogger Log = (Microsoft.Extensions.Logging.ILogger)AppLog.For<TrainingState>();

        public GameStateId Id => GameStateId.Training;

        private TrainingRequest? _request;
        private TrainingResult? _result;
        private bool _started;
        private bool _completed;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            _request = args?.Payload as TrainingRequest?;
            ctx.Ui?.ShowTrainingScreen();
            ctx.Ui?.SetStatus("Plan a training sessionâ€¦");
            return Task.CompletedTask;
        }

        public async Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            // Treat external cancellation (e.g., UI cancel wired to token) as training cancel
            if (ct.IsCancellationRequested)
            {
                ctx.Ui?.ShowToast("Training cancelled.");
                return new StateTransition(GameStateId.Exploring, new StateArgs { Reason = "TrainingCancelled" });
            }

            if (_completed)
            {
                // Leave training once weâ€™ve produced a result
                return new StateTransition(
                    GameStateId.Exploring,
                    new StateArgs { Reason = "TrainingComplete", Payload = _result }
                );
            }

            if (_request is null)
            {
                // No request yet; remain in this state
                return null;
            }

            if (!_started)
            {
                _started = true;

                // (Optional) show a trivial progress pulse if desired
                ctx.Ui?.ShowTrainingProgress(0, 1);

                // Simulate â€œdoingâ€ the training
                await Task.Delay(TimeSpan.FromMilliseconds(250), ct);

                _result = new TrainingResult(
                    _request.Value.TargetBotId,
                    _request.Value.Mode,
                    10,
                    new[] { "STR +1" },
                    new[] { "Training completed." }
                );

                ctx.Ui?.SetStatus("Training complete.");
                ctx.Ui?.ShowTrainingProgress(1, 1);

                _completed = true;
            }

            // Wait one tick; next ExecuteAsync will perform the transition above
            return null;
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            ctx.Ui?.HideTrainingProgress();
            ctx.Ui?.HideTrainingScreen();
            return Task.CompletedTask;
        }
    }
}

