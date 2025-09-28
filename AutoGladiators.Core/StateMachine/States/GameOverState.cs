using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.Services;
    using AutoGladiators.Core.Services.Logging;
using Microsoft.Extensions.Logging;


namespace AutoGladiators.Core.StateMachine.States
{
    public sealed class GameOverState : IGameState
    {
        private static readonly Microsoft.Extensions.Logging.ILogger Log = (Microsoft.Extensions.Logging.ILogger)AppLog.For<GameOverState>();

        public GameStateId Id => GameStateId.GameOver;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            // Keep it minimal for now; avoid calling services that may not exist/are partial.
            ctx.Ui?.SetStatus("Game Over. Returning to adventureâ€¦");
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

