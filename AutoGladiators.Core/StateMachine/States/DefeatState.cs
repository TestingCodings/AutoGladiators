using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Services.Logging;
using Microsoft.Extensions.Logging;



namespace AutoGladiators.Core.StateMachine.States
{
    public sealed class DefeatState : IGameState
    {
        private static readonly Microsoft.Extensions.Logging.ILogger Log = (Microsoft.Extensions.Logging.ILogger)AppLog.For<DefeatState>();

        public GameStateId Id => GameStateId.Defeat;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            Log.LogInformation("Battle concluded. Defeat.");
            ctx.Ui?.ShowDefeatScreen(args?.Payload);
            ctx.Ui?.SetStatus("You were defeated");
            return Task.CompletedTask;
        }

        public Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            Log.LogInformation("Returning to adventure after defeat.");
            return Task.FromResult<StateTransition?>(new StateTransition(
                GameStateId.Exploring,
                new StateArgs { Reason = "DefeatReturn" }
            ));
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            ctx.Ui?.HideDefeatScreen();
            return Task.CompletedTask;
        }
    }
}




