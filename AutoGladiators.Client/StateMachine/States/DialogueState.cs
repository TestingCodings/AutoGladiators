using System;
using System.Threading;
using System.Threading.Tasks;
using AutoGladiators.Client.StateMachine;
using AutoGladiators.Client.Services;


namespace AutoGladiators.Client.StateMachine.States
{
    public sealed class DialogueState : IGameState
    {
        public GameStateId Id => GameStateId.Dialogue;

        private string? _npcId;

        public Task EnterAsync(GameStateContext ctx, StateArgs? args = null, CancellationToken ct = default)
        {
            // Prefer NPC id from args, fall back to whatever your game tracks as "current"
            _npcId = args?.Payload as string ?? ctx.Game.CurrentNpcId;

            Console.WriteLine("Entering dialogue" + (_npcId is null ? "…" : $" with NPC '{_npcId}'…"));

            // Hook up your dialogue UI/service (keep it generic so it compiles with or without a UI bridge)
            // Examples (implement these in your UiBridge or services as needed):
            //   ctx.Ui.ShowDialogueForNpc(_npcId);
            //   ctx.Game.Dialogue.Begin(_npcId);
            ctx.Ui?.ShowDialogueForNpc?.Invoke(_npcId);
            ctx.Game.SetFlag("InDialogue", "true");

            return Task.CompletedTask;
        }

        public Task<StateTransition?> ExecuteAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            // Drive dialogue via UI signals or a dialogue service state machine

            // If the dialogue triggers a battle (e.g., hostile NPC choice)
            if (ctx.Ui?.DialogueRequestedBattle == true)
            {
                // Option A: rely on BattlingState to pull bots from GameStateService
                return Task.FromResult<StateTransition?>(new StateTransition(
                    GameStateId.Battling,
                    new StateArgs { Reason = "NpcBattle" }
                ));

                // Option B: if you have an explicit enemy to pass:
                // var playerBot = ctx.Game.GetCurrentBot();
                // var enemyBot  = ctx.Game.SpawnNpcEncounter(_npcId);
                // var setup = new BattleSetup(playerBot!, enemyBot!, PlayerInitiated: false);
                // return Task.FromResult<StateTransition?>(new StateTransition(
                //     GameStateId.Battling,
                //     new StateArgs { Reason = "NpcBattle", Payload = setup }
                // ));
            }

            // If dialogue is finished (either through UI or service)
            if (ctx.Ui?.DialogueCompleted == true /* || ctx.Game.Dialogue.IsFinished */)
            {
                return Task.FromResult<StateTransition?>(new StateTransition(
                    GameStateId.Exploring,
                    new StateArgs { Reason = "DialogueEnd", Payload = _npcId }
                ));
            }

            // Otherwise, keep running this state
            return Task.FromResult<StateTransition?>(null);
        }

        public Task ExitAsync(GameStateContext ctx, CancellationToken ct = default)
        {
            Console.WriteLine("Exiting dialogue.");
            ctx.Ui?.HideDialogue?.Invoke();
            ctx.Game.SetFlag("InDialogue", "false");
            return Task.CompletedTask;
        }
    }
}
