using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.Services;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Client.StateMachine.States
{
    // Payload passed into FusionState via StateArgs.Payload (optional)
    public readonly record struct FusionRequest(
        Guid BotIdA,
        Guid BotIdB,
        bool ConsumeParents = true,
        string? CatalystItemId = null
    );

    // Result payload returned when fusion completes (keep for future use)
    public sealed record FusionResult(
        object? NewBot, // use object for now to avoid depending on GladiatorBot constructor/shape
        string[] Log
    );

    public sealed class FusionState : IGameState
    {
        private static readonly IAppLogger Log = AppLog.For<FusionState>();

        public GameStateId Id => GameStateId.Fusion;

        private FusionRequest? _request;
        private FusionResult? _result;
        private bool _started;
        private bool _completed;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            _request = args?.Payload as FusionRequest?;
            ctx.Ui?.SetStatus("Fusion lab: select two bots to fuse…");
            return Task.CompletedTask;
        }

        public async Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            if (_completed)
            {
                return new StateTransition(
                    GameStateId.Exploring,
                    new StateArgs { Reason = "FusionComplete", Payload = _result }
                );
            }

            if (_request is null)
            {
                // No UI picker wired yet; stay here until a request is provided externally.
                return null;
            }

            if (!_started)
            {
                _started = true;

                // Simulate a short operation; replace with real service calls later.
                ctx.Ui?.SetStatus("Fusing…");
                await Task.Delay(250, ct);

                _result = new FusionResult(
                    NewBot: null, // TODO: create the new GladiatorBot when your fusion logic is ready
                    Log: new[] { "Fusion complete." }
                );

                ctx.Ui?.ShowToast("Fusion complete.");
                ctx.Ui?.SetStatus("Fusion complete.");
                _completed = true;
            }

            // Next tick we’ll transition (handled at the top of this method).
            return null;
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }
    }
}

