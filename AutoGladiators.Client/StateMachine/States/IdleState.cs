using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.Services;

namespace AutoGladiators.Client.StateMachine.States
{
    public sealed class IdleState : IGameState
    {
        public GameStateId Id => GameStateId.Idle; // ensure your enum includes Idle

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            ctx.Ui?.ShowIdleScreen();
            ctx.Ui?.SetStatus("Idle…");
            return Task.CompletedTask;
        }

        public Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            // Drive transitions based on simple flags/buttons your UI/VM can set

            if (ctx.Ui?.RequestedStart == true)
            {
                return Task.FromResult<StateTransition?>(new StateTransition(
                    GameStateId.Exploring,
                    new StateArgs { Reason = "StartFromIdle" }
                ));
            }

            if (ctx.Ui?.RequestedOpenInventory == true)
            {
                return Task.FromResult<StateTransition?>(new StateTransition(
                    GameStateId.Inventory,
                    new StateArgs { Reason = "OpenInventoryFromIdle" }
                ));
            }

            if (ctx.Ui?.RequestedDialogueNpcId is string npcId && !string.IsNullOrWhiteSpace(npcId))
            {
                return Task.FromResult<StateTransition?>(new StateTransition(
                    GameStateId.Dialogue,
                    new StateArgs { Reason = "DialogueFromIdle", Payload = npcId }
                ));
            }

            // Otherwise, remain idle
            return Task.FromResult<StateTransition?>(null);
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            ctx.Ui?.HideIdleScreen();
            return Task.CompletedTask;
        }
    }
}

