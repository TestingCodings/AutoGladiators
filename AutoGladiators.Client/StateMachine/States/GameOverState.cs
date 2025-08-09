using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.Services;

namespace AutoGladiators.Client.StateMachine.States
{
    public sealed class GameOverState : IGameState
    {
        public GameStateId Id => GameStateId.GameOver;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            // Keep it minimal for now; avoid calling services that may not exist/are partial.
            ctx.Ui?.SetStatus("Game Over. Returning to adventure…");
            return Task.CompletedTask;
        }

        public Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            // Immediately bounce back to Exploring; wire save/load later.
            return Task.FromResult<StateTransition?>(
                new StateTransition(GameStateId.Exploring, new StateArgs { Reason = "GameOverReturn" })
            );
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }
    }
}
