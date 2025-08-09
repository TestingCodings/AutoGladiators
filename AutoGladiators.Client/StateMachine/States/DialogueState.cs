using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutoGladiators.Client.StateMachine.States
{
    public sealed class DialogueState : IGameState
    {
        public GameStateId Id => GameStateId.Dialogue;
        private string? _npcId;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            _npcId = args?.Payload as string ?? ctx.Game.CurrentNpcId;
            if (!string.IsNullOrWhiteSpace(_npcId))
                ctx.Ui?.ShowDialogueForNpc(_npcId!, false);
            ctx.Game.SetFlag("InDialogue", "true");
            return Task.CompletedTask;
        }

        public Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            // Drive via flags you toggle from the UI
            if (string.Equals(ctx.Game.GetFlag("DialogueRequestedBattle"), "true", StringComparison.OrdinalIgnoreCase))
            {
                ctx.Game.SetFlag("DialogueRequestedBattle", "false");
                return Task.FromResult<StateTransition?>(new StateTransition(GameStateId.Battling,
                    new StateArgs { Reason = "NpcBattle" }));
            }

            if (string.Equals(ctx.Game.GetFlag("DialogueCompleted"), "true", StringComparison.OrdinalIgnoreCase))
            {
                ctx.Game.SetFlag("DialogueCompleted", "false");
                return Task.FromResult<StateTransition?>(new StateTransition(GameStateId.Exploring,
                    new StateArgs { Reason = "DialogueEnd", Payload = _npcId }));
            }

            return Task.FromResult<StateTransition?>(null);
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            ctx.Ui?.HideDialogue();
            ctx.Game.SetFlag("InDialogue", "false");
            return Task.CompletedTask;
        }
    }
}