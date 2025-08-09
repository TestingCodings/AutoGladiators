using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.Services;

namespace AutoGladiators.Client.StateMachine.States
{
    public sealed class IdleState : IGameState
    {
        public GameStateId Id => GameStateId.Idle;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            // Use Overworld as the idle screen in this build.
            ctx.Ui?.ShowOverworld();
            ctx.Ui?.SetStatus("Idle…");
            return Task.CompletedTask;
        }

        public Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            // No UI flags available yet; stay idle until something external triggers a transition.
            return Task.FromResult<StateTransition?>(null);
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            ctx.Ui?.HideOverworld();
            return Task.CompletedTask;
        }
    }
}
