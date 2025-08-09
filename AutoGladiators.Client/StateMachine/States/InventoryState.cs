using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.Services;
using AutoGladiators.Client.Models;

namespace AutoGladiators.Client.StateMachine.States
{
    // Optional payloads for clarity
    public readonly record struct InventoryOpenRequest(
        string? Reason,              // "PausedBattle", "Exploring", etc.
        Guid? TargetBotId = null     // Preselected bot to use an item on
    );

    public sealed record ItemUseRequest(
        string ItemId,
        Guid? TargetBotId = null     // If null, UI should ask which bot
    );

    public sealed record ItemUseResult(
        string ItemId,
        Guid TargetBotId,
        string[] Log,
        bool Success
    );

    public sealed class InventoryState : IGameState
    {
        public GameStateId Id => GameStateId.Inventory;

        private InventoryOpenRequest _openReq;
        private bool _cameFromBattle;
        private bool _done;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            // Use positional parameters for InventoryOpenRequest
            _openReq = (args?.Payload as InventoryOpenRequest?) ?? new InventoryOpenRequest("Unknown", null);
            _cameFromBattle = string.Equals(_openReq.Reason, "PausedBattle", StringComparison.OrdinalIgnoreCase);

            ctx.Ui?.ShowInventory(ctx.Game.Inventory);
            ctx.Ui?.SetStatus(_cameFromBattle ? "Inventory (Battle Paused)" : "Inventory");

            return Task.CompletedTask;
        }

        public async Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            if (_done) 
            {
                // Decide where to go after finishing
                return new StateTransition(
                    _cameFromBattle ? GameStateId.Battling : GameStateId.Exploring,
                    new StateArgs { Reason = _cameFromBattle ? "ResumeBattle" : "CloseInventory" }
                );
            }

            // 1) Close inventory?
            // Removed: ctx.Game.Flags.Contains("CloseInventory")
            if (ctx.Ui?.RequestedCloseInventory == true)
            {
                _done = true;
                return null; // transition next tick via block above
            }

            // 2) Use item? (UI should provide an ItemUseRequest payload)
            // Removed: var useReq = ctx.Ui?.GetItemUseRequest?.Invoke();
            // Simulate no item use request for now
            ItemUseRequest? useReq = null;
            // If you implement GetItemUseRequest, uncomment below:
            // var useReq = ctx.Ui?.GetItemUseRequest();

            if (useReq is ItemUseRequest r)
            {
                // Only use Guid? types for ?? chain
                Guid? targetId = r.TargetBotId ?? _openReq.TargetBotId ?? ctx.Game.GetCurrentBot()?.Id;
                if (targetId is null)
                {
                    // ctx.Ui?.ShowToast("Select a target for the item.");
                    return null;
                }

                // Removed: CanUseItem
                // bool canUse = ctx.Game.CanUseItem(r.ItemId, targetId.Value, inBattle: _cameFromBattle, out var reason);
                bool canUse = true;
                string reason = null;
                if (!canUse)
                {
                    // ctx.Ui?.ShowToast(reason ?? "Item cannot be used.");
                    return null;
                }

                // Removed: UseItemAsync
                // var result = await ctx.Game.UseItemAsync(r.ItemId, targetId.Value, inBattle: _cameFromBattle, ct: ct);
                var result = new ItemUseResult(r.ItemId, targetId.Value, new[] { "Used item." }, true);

                if (result is null || !result.Success)
                {
                    // ctx.Ui?.ShowToast("Item failed or had no effect.");
                    return null;
                }

                // Removed: ConsumeItem
                // ctx.Game.ConsumeItem(r.ItemId, count: 1);

                // Removed: RefreshInventory, ShowItemUseResult
                // ctx.Ui?.RefreshInventory(ctx.Game.Inventory);
                // ctx.Ui?.ShowItemUseResult(result);

                // Removed: ItemUseEndsTurn
                // if (_cameFromBattle && ctx.Game.ItemUseEndsTurn(r.ItemId))
                if (_cameFromBattle)
                {
                    _done = true; // next tick transitions back into Battling
                    return null;
                }

                // Otherwise remain in inventory for more actions
                return null;
            }

            // 3) Other actions (equip, sort, discard) – optional flags
            if (ctx.Ui?.RequestedDiscardItemId is string discardId && !string.IsNullOrWhiteSpace(discardId))
            {
                // Removed: DiscardItem
                // bool discarded = ctx.Game.DiscardItem(discardId, count: 1);
                bool discarded = true;
                if (discarded)
                {
                    // ctx.Ui?.RefreshInventory(ctx.Game.Inventory);
                    // ctx.Ui?.ShowToast("Item discarded.");
                }
                else
                {
                    // ctx.Ui?.ShowToast("Could not discard item.");
                }
                return null;
            }

            // Stay in inventory until the user acts
            return null;
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            ctx.Ui?.HideInventory();
            return Task.CompletedTask;
        }
    }
}

