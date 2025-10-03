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
        private static readonly IAppLogger Log = AppLog.For<DefeatState>();

        public GameStateId Id => GameStateId.Defeat;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            Log.Info("Battle concluded. Defeat.");
            ctx.Ui?.ShowDefeatScreen(args?.Payload);
            ctx.Ui?.SetStatus("You were defeated");
            return Task.CompletedTask;
        }

        public Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            Log.Info("Returning to main menu after defeat");
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




