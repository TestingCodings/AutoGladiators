using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Client.Models;
using AutoGladiators.Client.Services;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.Core;
using AutoGladiators.Client.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Client.StateMachine.States
{
    public sealed class ExploringState : IGameState
    {
        private static readonly IAppLogger Log = AppLog.For<ExploringState>();

        public GameStateId Id => GameStateId.Exploring;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            ctx.Ui?.ShowOverworld();
            ctx.Ui?.SetStatus("Exploring…");
            return Task.CompletedTask;
        }

        public Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            // No encounter generation yet – remain exploring
            return Task.FromResult<StateTransition?>(null);
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            ctx.Ui?.HideOverworld();
            return Task.CompletedTask;
        }
    }
}

