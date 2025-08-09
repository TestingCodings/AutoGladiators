using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.Services;
using AutoGladiators.Client.Models;

namespace AutoGladiators.Client.StateMachine.States
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
        public GameStateId Id => GameStateId.Training;

        private TrainingRequest? _request;
        private TrainingResult? _result;
        private bool _started;
        private bool _completed;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            _request = args?.Payload as TrainingRequest?;
            ctx.Ui?.ShowTrainingScreen();
            ctx.Ui?.SetStatus("Plan a training session…");
            return Task.CompletedTask;
        }

        public async Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            if (_completed)
            {
                return new StateTransition(
                    GameStateId.Exploring,
                    new StateArgs { Reason = "TrainingComplete", Payload = _result }
                );
            }

            if (ctx.Ui?.TrainingCancelled == true)
            {
                ctx.Ui?.ShowToast("Training cancelled.");
                return new StateTransition(GameStateId.Exploring, new StateArgs { Reason = "TrainingCancelled" });
            }

            if (_request is null)
            {
                return null;
            }

            if (!_started)
            {
                _started = true;

                // Simulate training result
                _result = new TrainingResult(
                    _request.Value.TargetBotId,
                    _request.Value.Mode,
                    10,
                    new[] { "STR +1" },
                    new[] { "Training completed." }
                );

                ctx.Ui?.SetStatus("Training complete.");
                _completed = true;
            }

            // Transition next tick (or wait for a “Continue” button via a UI flag)
            if (ctx.Ui?.TrainingContinueRequested == true)
            {
                // Continue training event
                ctx.Ui.TrainingContinueRequested?.Invoke(this, EventArgs.Empty);
                return new StateTransition(GameStateId.Exploring, new StateArgs { Reason = "TrainingContinue", Payload = _result });
            }

            return null;
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            // Hide training screen and progress
            ctx.Ui?.HideTrainingScreen?.Invoke();
            ctx.Ui?.HideTrainingProgress?.Invoke();
            return Task.CompletedTask;
        }
    }

    // ---- Suggested GameStateService hooks (implement as needed) ----
    // public partial class GameStateService
    // {
    //     public (bool IsValid, string? Message) ValidateTraining(TrainingRequest req) { ... }
    //     public void PayTrainingCost(TrainingRequest req) { ... }
    //     public Task<TrainingResult> PerformTrainingAsync(TrainingRequest req, CancellationToken ct) { ... }
    //     public void ApplyTrainingResult(TrainingResult res) { ... }
    // }
}
