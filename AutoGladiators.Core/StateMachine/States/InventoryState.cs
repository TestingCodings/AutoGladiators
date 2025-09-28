using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Core.StateMachine;
using AutoGladiators.Core.Services;
using AutoGladiators.Core.Models;
using AutoGladiators.Core.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Core.StateMachine.States
{
    public sealed class InventoryState : IGameState
    {
        private static readonly Microsoft.Extensions.Logging.ILogger Log = (Microsoft.Extensions.Logging.ILogger)AppLog.For<InventoryState>();

        public GameStateId Id => GameStateId.Inventory;

        private bool _cameFromBattle;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            var reason = args?.Reason ?? string.Empty;
            _cameFromBattle = reason.Equals("PausedBattle", StringComparison.OrdinalIgnoreCase);

            ctx.Ui?.ShowInventory(ctx.Game.Inventory);
            ctx.Ui?.SetStatus(_cameFromBattle ? "Inventory (Battle Paused)" : "Inventory");

            // Clear any stale close flag
            if (string.Equals(ctx.Game.GetFlag("CloseInventory"), "true", StringComparison.OrdinalIgnoreCase))
                ctx.Game.SetFlag("CloseInventory", "false");

            return Task.CompletedTask;
        }

        public Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            // Use the GameStateService flag that your UI can set when the user taps "Close"
            if (string.Equals(ctx.Game.GetFlag("CloseInventory"), "true", StringComparison.OrdinalIgnoreCase))
            {
                // consume the flag
                ctx.Game.SetFlag("CloseInventory", "false");

                var next = _cameFromBattle ? GameStateId.Battling : GameStateId.Exploring;
                var reason = _cameFromBattle ? "ResumeBattle" : "CloseInventory";

                return Task.FromResult<StateTransition?>(new StateTransition(
                    next,
                    new StateArgs { Reason = reason }
                ));
            }

            // Stay in inventory until told to close
            return Task.FromResult<StateTransition?>(null);
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            ctx.Ui?.HideInventory();
            return Task.CompletedTask;
        }
    }
}


