using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Core.StateMachine.States
{
    public sealed class VictoryState : IGameState
    {
        private static readonly Microsoft.Extensions.Logging.ILogger Log = (Microsoft.Extensions.Logging.ILogger)AppLog.For<VictoryState>();

        public GameStateId Id => GameStateId.Victory;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            Log.LogInformation("Battle concluded. Victory!");
            ctx.Ui?.ShowVictoryScreen(args?.Payload);
            return Task.CompletedTask;
        }

        public Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            Log.LogInformation("Returning to adventure after victory.");
            return Task.FromResult<StateTransition?>(new StateTransition(
                GameStateId.Exploring,
                new StateArgs { Reason = "VictoryReturn" }
            ));
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            Console.WriteLine("Post-battle tasks completed.");
            ctx.Ui?.HideVictoryScreen();
            return Task.CompletedTask;
        }
    }
}

