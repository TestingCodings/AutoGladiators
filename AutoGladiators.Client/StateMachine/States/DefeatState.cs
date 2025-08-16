using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.Services;
using AutoGladiators.Client.Services.Logging;
using Microsoft.Extensions.Logging;



namespace AutoGladiators.Client.StateMachine.States
{
    public sealed class DefeatState : IGameState
    {
        private static readonly IAppLogger Log = AppLog.For<DefeatState>();

        public GameStateId Id => GameStateId.Defeat;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            var payload = args?.Payload; // stats/log/etc if you have them
            ctx.Ui?.ShowDefeatScreen(payload);
            ctx.Ui?.SetStatus("You were defeated…");
            return Task.CompletedTask;
        }

        public Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            if (string.Equals(ctx.Game.GetFlag("DefeatAck"), "true", StringComparison.OrdinalIgnoreCase))
            {
                ctx.Game.SetFlag("DefeatAck", "false");
                return Task.FromResult<StateTransition?>(new StateTransition(
                    GameStateId.GameOver, new StateArgs { Reason = "DefeatAck" }));
            }
            return Task.FromResult<StateTransition?>(null);
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            ctx.Ui?.HideDefeatScreen();
            return Task.CompletedTask;
        }
    }
}




